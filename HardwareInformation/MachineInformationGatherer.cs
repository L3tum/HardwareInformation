#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using HardwareInformation.Information.Cpu;
using HardwareInformation.Providers;
using HardwareInformation.Providers.Windows;
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
        internal static ILogger Logger;
        internal static GatheringOptions Options;

        private static readonly InformationProvider[] InformationProviders =
        {
            new CommonInformationProvider(),
            new AMDInformationProvider(),
            new IntelInformationProvider(),
            new VulkanInformationProvider(),
            new WindowsInformationProvider(),
            new LinuxInformationProvider(),
            new OSXInformationProvider(),
            new RyzenMasterProvider()
        };

        private static MachineInformation machineInformation;

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
        /// <param name="options">NULL means default all</param>
        /// <returns></returns>
        public static MachineInformation GatherInformation(bool skipClockspeedTest = true, bool invalidateCache = false,
            ILogger logger = null, GatheringOptions options = null)
        {
            Logger = logger ?? new NullLogger<MachineInformation>();

            if (machineInformation != null && !invalidateCache)
            {
                Logger.LogInformation("Returning cached information");
                return machineInformation;
            }

            Options = options ?? new GatheringOptions();
            
            ThreadAffinity.SetCurrentProcess();

            machineInformation = GetInformation(skipClockspeedTest);

            PostOperations();

            return machineInformation;
        }

        private static MachineInformation GetInformation(bool skipClockspeedTest)
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.X86 ||
                RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                Logger.LogInformation("Loading OpCodes for CPUID");
                Opcode.Open();

                AppDomain.CurrentDomain.DomainUnload += (sender, args) => { Opcode.Close(); };
            }
            else
            {
                Logger.LogInformation("No CPUID available on non-x86 CPUs");
            }

            var informationProviders = new List<InformationProvider>();
            var info = new MachineInformation();

            foreach (var provider in InformationProviders)
            {
                if (provider.Available(info))
                {
                    informationProviders.Add(provider);
                }
                else
                {
                    Logger.LogWarning("{Provider} is not available", provider.GetType().Name);
                }
            }

            foreach (var informationProvider in informationProviders)
            {
                Logger.LogInformation("Collecting General System information from {Provider}",
                    informationProvider.GetType().Name);

                try
                {
                    informationProvider.GatherGeneralSystemInformation(ref info);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            var informations = new Dictionary<string, MachineInformation>();

            foreach (var informationProvider in informationProviders)
            {
                Logger.LogInformation("Collecting information from {Provider}",
                    informationProvider.GetType().Name);

                // Copy general system info from before onto a MachineInformation for each provider
                var information = new MachineInformation();
                MachineInformationMapper.MapMachineInformation(ref information, info);

                GatherInformationFromProvider(informationProvider, ref information);

                informations.Add(informationProvider.GetType().Name, information);
            }

            if (!skipClockspeedTest && (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                                        RuntimeInformation.IsOSPlatform(OSPlatform.Linux)))
            {
                Logger.LogWarning("Running clockspeed tests");
                GetCoreSpeeds();
            }

            foreach (var informationProvider in informationProviders)
            {
                Logger.LogInformation("Running post update on {Provider}", informationProvider.GetType().Name);

                var information = informations[informationProvider.GetType().Name];
                try
                {
                    informationProvider.PostProviderUpdateInformation(ref information);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when running post-update from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            var dest = new MachineInformation();

            foreach (var kvp in informations)
            {
                MachineInformationMapper.MapMachineInformation(ref dest, kvp.Value);
            }

            return dest;
        }

        private static void GatherInformationFromProvider(InformationProvider informationProvider,
            ref MachineInformation information)
        {
            if (Options.GatherCpuInformation)
            {
                try
                {
                    informationProvider.GatherCpuInformation(ref information);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            if (Options.GatherCpuFeatureFlagInformation)
            {
                try
                {
                    informationProvider.GatherCpuFeatureFlagInformation(ref information);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            if (Options.GatherCpuSpeedInformation)
            {
                try
                {
                    informationProvider.GatherCpuSpeedInformation(ref information);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            if (Options.GatherCpuCacheTopologyInformation)
            {
                try
                {
                    informationProvider.GatherCpuCacheTopologyInformation(ref information);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            if (Options.GatherMainboardInformation)
            {
                try
                {
                    informationProvider.GatherMainboardInformation(ref information);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            if (Options.GatherRamInformation)
            {
                try
                {
                    informationProvider.GatherRamInformation(ref information);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            if (Options.GatherDiskInformation)
            {
                try
                {
                    informationProvider.GatherDiskInformation(ref information);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            if (Options.GatherGpuInformation)
            {
                try
                {
                    informationProvider.GatherGpuInformation(ref information);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            if (Options.GatherUsbInformation)
            {
                try
                {
                    informationProvider.GatherUsbInformation(ref information);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            if (Options.GatherMonitorInformation)
            {
                try
                {
                    informationProvider.GatherMonitorInformation(ref information);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
            }
        }

        private static void PostOperations()
        {
            // Fix some things that may be propagated from lower-level information providers

            if (machineInformation.Cpu != null)
            {
                if (machineInformation.Cpu.Name != null)
                {
                    machineInformation.Cpu.Name = machineInformation.Cpu.Name.Trim();
                }

                if (machineInformation.Cpu.Caption != null)
                {
                    machineInformation.Cpu.Caption = machineInformation.Cpu.Caption.Trim();
                }

                if (Options.GatherPerCoreInformation && machineInformation.Cpu.Cores != null)
                {
                    foreach (var cpuCore in machineInformation.Cpu.Cores)
                    {
                        if (cpuCore.NormalClockSpeed == 0)
                        {
                            cpuCore.NormalClockSpeed = machineInformation.Cpu.NormalClockSpeed;
                        }

                        if (cpuCore.MaxClockSpeed == 0)
                        {
                            cpuCore.MaxClockSpeed = machineInformation.Cpu.MaxClockSpeed;
                        }
                    }
                }

                if (!Options.GatherPerCoreInformation)
                {
                    machineInformation.Cpu.Cores = new List<Core>().AsReadOnly();
                }
            }
        }

        private static void GetCoreSpeeds()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            for (var i = 0; i < machineInformation.Cpu.LogicalCores; i++)
            {
                if (i > 64)
                {
                    // Too long for long
                    break;
                }

                var core = machineInformation.Cpu.Cores.First(c => c.Number == i);

                core.NormalClockSpeed = machineInformation.Cpu.NormalClockSpeed;

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
            machineInformation.Cpu.MaxClockSpeed =
                machineInformation.Cpu.Cores.Count > 0 ? machineInformation.Cpu.Cores.Max(c => c.MaxClockSpeed) : 0;
        }
    }
}