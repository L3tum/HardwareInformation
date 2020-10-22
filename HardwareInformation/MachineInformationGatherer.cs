#region using

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using HardwareInformation.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
            new CommonInformationProvider(),
            new AMDInformationProvider(),
            new IntelInformationProvider(),
            new VulkanInformationProvider(),
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
        /// <param name="invalidateCache">
        ///     Default false. Whether to re-gather all the informations.
        /// </param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static MachineInformation GatherInformation(bool skipClockspeedTest = true, bool invalidateCache = false,
            ILogger<MachineInformation> logger = null)
        {
            if (logger == null)
            {
                logger = new NullLogger<MachineInformation>();
            }

            if (information != null && lastSkipClockspeedTest == skipClockspeedTest && !invalidateCache)
            {
                logger.LogInformation("Returning cached information");
                return information;
            }

            if (RuntimeInformation.ProcessArchitecture == Architecture.X86 ||
                RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                logger.LogInformation("Loading OpCodes for CPUID");
                Opcode.Open();

                AppDomain.CurrentDomain.DomainUnload += (sender, args) => { Opcode.Close(); };
            }
            else
            {
                logger.LogInformation("No CPUID available on non-x86 CPUs");
            }

            lastSkipClockspeedTest = skipClockspeedTest;
            information = new MachineInformation();

            foreach (var informationProvider in InformationProviders)
            {
                logger.LogInformation("Collecting information from {Provider}",
                    informationProvider.GetType().Name);

                try
                {
                    if (informationProvider.Available(information))
                    {
                        informationProvider.GatherInformation(ref information);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            foreach (var cpuCore in information.Cpu.Cores)
            {
                if (cpuCore.NormalClockSpeed == 0)
                {
                    cpuCore.NormalClockSpeed = information.Cpu.NormalClockSpeed;
                }

                if (cpuCore.MaxClockSpeed == 0)
                {
                    cpuCore.MaxClockSpeed = information.Cpu.MaxClockSpeed;
                }
            }

            foreach (var informationProvider in InformationProviders)
            {
                logger.LogInformation("Running post update on {Provider}", informationProvider.GetType().Name);

                try
                {
                    if (informationProvider.Available(information))
                    {
                        informationProvider.PostProviderUpdateInformation(ref information);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception when running post-update from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            if (!skipClockspeedTest && (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                                        RuntimeInformation.IsOSPlatform(OSPlatform.Linux)))
            {
                logger.LogWarning("Running clockspeed tests");
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