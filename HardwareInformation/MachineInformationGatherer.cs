#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;
using HardwareInformation.Information;
using HardwareInformation.Information.Cpu;
using HardwareInformation.Providers;

#endregion

namespace HardwareInformation
{
	/// <summary>
	///     Main entry class to gather information on the system hardware.
	/// </summary>
	public static class MachineInformationGatherer
    {
        private static readonly InformationProvider[] InformationProviders =
        {
            new AMDInformationProvider(),
            new IntelInformationProvider(),
            new WindowsInformationProvider(),
            new LinuxInformationProvider(),
            new OSXInformationProvider()
        };

        private static MachineInformation information;

        private static bool lastSkipClockspeedTest = true;

        /// <summary>
        ///     Gathers lots of information about the running processor.
        ///     Currently does NOT support multi-processor setups (e.g. two Intel Xeon CPUs).
        ///     For detailed information about the information provided please see the readme.
        /// </summary>
        /// <param name="skipClockspeedTest">
        ///     Default true. If false it will run a quick speed test of all cores to determine
        ///     maximum frequency.
        /// </param>
        /// <returns></returns>
        public static MachineInformation GatherInformation(bool skipClockspeedTest = true)
        {
            if (information != null && lastSkipClockspeedTest == skipClockspeedTest)
            {
                return information;
            }

            if (RuntimeInformation.ProcessArchitecture == Architecture.X86 ||
                RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                Opcode.Open();

                AppDomain.CurrentDomain.DomainUnload += (sender, args) => { Opcode.Close(); };
            }

            lastSkipClockspeedTest = skipClockspeedTest;
            information = new MachineInformation();

            GetCommonCpuInformation();

            GatherCommonPerCoreInformation();

            foreach (var informationProvider in InformationProviders)
            {
                try
                {
                    if (informationProvider.Available(information))
                    {
                        informationProvider.GatherInformation(ref information);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }

            foreach (var cpuCore in information.Cpu.Cores)
            {
                cpuCore.NormalClockSpeed = information.Cpu.NormalClockSpeed;
                cpuCore.MaxClockSpeed = information.Cpu.MaxClockSpeed;
            }

            foreach (var informationProvider in InformationProviders)
            {
                try
                {
                    if (informationProvider.Available(information))
                    {
                        informationProvider.PostProviderUpdateInformation(ref information);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }

            if (!skipClockspeedTest && (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                                        RuntimeInformation.IsOSPlatform(OSPlatform.Linux)))
            {
                GetCoreSpeeds();
            }

            // Fix some things that may be propagated from lower-level information providers

            if (information.Cpu != null)
            {
                if (information.Cpu.Name != null)
                {
                    information.Cpu.Name = information.Cpu.Name.Trim();
                }

                if (information.Cpu.Caption != null)
                {
                    information.Cpu.Caption = information.Cpu.Caption.Trim();
                }
            }

            return information;
        }

        // TODO: Tests

        private static void GetCommonCpuInformation()
        {
            information.OperatingSystem = Environment.OSVersion;
            information.Platform = Expression.Empty() switch
            {
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => MachineInformation.Platforms.Windows,
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => MachineInformation.Platforms.Linux,
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => MachineInformation.Platforms.OSX,
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => MachineInformation.Platforms.Windows,
                null or not null => MachineInformation.Platforms.Unknown
            };
            information.Cpu.LogicalCores = (uint) Environment.ProcessorCount;
            information.Cpu.LogicalCoresPerNode = information.Cpu.LogicalCores;
            information.Cpu.Nodes = 1;
            information.Cpu.Architecture = RuntimeInformation.ProcessArchitecture.ToString();
            information.Cpu.Caption = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? default;
            information.Cpu.Name = information.Cpu.Caption;

            var cores = new List<Core>();
            for (var i = 0; i < information.Cpu.LogicalCores; i++)
            {
                cores.Add(new Core
                {
                    Number = (uint) i
                });
            }

            information.Cpu.Cores = cores.AsReadOnly();

            if (RuntimeInformation.ProcessArchitecture != Architecture.X86 &&
                RuntimeInformation.ProcessArchitecture != Architecture.X64)
            {
                return;
            }

            Opcode.Cpuid(out var result, 0, 0);

            var vendorString = string.Format("{0}{1}{2}",
                string.Join("", $"{result.ebx:X}".HexStringToString().Reverse()),
                string.Join("", $"{result.edx:X}".HexStringToString().Reverse()),
                string.Join("", $"{result.ecx:X}".HexStringToString().Reverse()));

            information.Cpu.Vendor = vendorString;
            information.Cpu.MaxCpuIdFeatureLevel = result.eax;

            if (information.Cpu.MaxCpuIdFeatureLevel >= 1)
            {
                Opcode.Cpuid(out result, 1, 0);

                information.Cpu.Stepping = result.eax & 0xF;

                var familyId = (result.eax & 0xF00) >> 8;

                if (familyId == 6 || familyId == 15)
                {
                    information.Cpu.Model = (((result.eax & 0xF0000) >> 16) << 4) + ((result.eax & 0xF0) >> 4);
                }
                else
                {
                    information.Cpu.Model = (result.eax & 0xF0) >> 4;
                }

                if (familyId == 15)
                {
                    information.Cpu.Family = ((result.eax & 0xFF00000) >> 20) + familyId;
                }
                else
                {
                    information.Cpu.Family = familyId;
                }

                information.Cpu.Type =
                    (CPU.ProcessorType) ((result.eax & 0b11000000000000) >> 12);
                information.Cpu.FeatureFlagsOne = (CPU.FeatureFlagEDX) result.edx;
                information.Cpu.FeatureFlagsTwo = (CPU.FeatureFlagECX) result.ecx;
            }

            if (information.Cpu.MaxCpuIdFeatureLevel >= 7)
            {
                Opcode.Cpuid(out result, 7, 0);

                information.Cpu.ExtendedFeatureFlagsF7One =
                    (CPU.ExtendedFeatureFlagsF7EBX) result.ebx;
                information.Cpu.ExtendedFeatureFlagsF7Two =
                    (CPU.ExtendedFeatureFlagsF7ECX) result.ecx;
                information.Cpu.ExtendedFeatureFlagsF7Three =
                    (CPU.ExtendedFeatureFlagsF7EDX) result.edx;
            }

            Opcode.Cpuid(out result, 0x80000000, 0);

            information.Cpu.MaxCpuIdExtendedFeatureLevel = result.eax;
        }

        private static void GatherCommonPerCoreInformation()
        {
            if (RuntimeInformation.ProcessArchitecture != Architecture.X86 &&
                RuntimeInformation.ProcessArchitecture != Architecture.X64)
            {
                return;
            }

            for (var i = 0; i < information.Cpu.LogicalCores; i++)
            {
                var core = information.Cpu.Cores.First(c => c.Number == i);
                var thread = Util.RunAffinity(1uL << i, () =>
                {
                    if (information.Cpu.MaxCpuIdFeatureLevel >= 16)
                    {
                        Opcode.Cpuid(out var result, 0x16, 0);

                        core.ReferenceNormalClockSpeed = result.eax;
                        core.ReferenceMaxClockSpeed = result.ebx;
                        core.ReferenceBusSpeed = result.ecx;
                    }
                });

                thread.Wait();
            }
        }

        private static void GetCoreSpeeds()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            for (var i = 0; i < information.Cpu.LogicalCores; i++)
            {
                if (i > 64)
                {
                    // Too long for long
                    break;
                }

                var core = information.Cpu.Cores.First(c => c.Number == i);

                core.NormalClockSpeed = information.Cpu.NormalClockSpeed;

                using var ct = new CancellationTokenSource();
                PerformanceCounter counter = null;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    counter =
                        new PerformanceCounter("Processor Information", "% Processor Performance", "0," + i);

                    counter.NextValue();
                }

                var thread = Util.RunAffinity(1uL << i, () =>
                {
                    var g = 0;

                    while (!ct.IsCancellationRequested)
                    {
                        g++;
                    }
                });

                Thread.Sleep(1000);

                var value = core.NormalClockSpeed;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    value = (uint) (counter.NextValue() / 100.0f * value);
                    counter.Dispose();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    try
                    {
                        // KHz
                        var freq = ulong.Parse(
                            File.ReadAllText($"/sys/devices/system/cpu/cpu{i}/cpufreq/scaling_cur_freq"));

                        value = (uint) (freq / 1000);
                    }
                    catch (Exception)
                    {
                        // Abort early since failing once means we'll most likely fail always.
                        ct.Cancel();
                        break;
                    }
                }

                core.MaxClockSpeed = value;
                ct.Cancel();
                thread.Wait();
            }

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
            information.Cpu.MaxClockSpeed =
                information.Cpu.Cores.Count > 0 ? information.Cpu.Cores.Max(c => c.MaxClockSpeed) : 0;
        }
    }
}