#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using HardwareInformation.Providers;
using HardwareInformation.Providers.General;
using HardwareInformation.Providers.Linux;
using HardwareInformation.Providers.MacOs;
using HardwareInformation.Providers.Windows;
using HardwareInformation.Providers.X86;
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

        /// <summary>
        ///     This list should be ordered from specific to less-specific. For example x86 is more specific than Windows
        ///     One example for an outlier is the AMD Provider, since that one only expands on the info already gathered by x86 and
        ///     is thus "less" specific.
        ///     Vulkan is run after the Windows provider because the Windows provider can actually supply more information than the
        ///     Vulkan one
        /// </summary>
        private static readonly InformationProvider[] InformationProviders =
        {
            new DotNetInformationProvider(),
            new X86InformationProvider(),
            new AmdInformationProvider(),
            new IntelInformationProvider(),
            new WindowsSystemInformationProvider(),
            new WindowsCpuInformationProvider(),
            new WindowsDisplayInformationProvider(),
            new WindowsRamInformationProvider(),
            new WindowsDiskInformationProvider(),
            new WindowsUsbInformationProvider(),
            new WindowsPciInformationProvider(),
            new WindowsGpuInformationProvider(),
            new LinuxSystemInformationProvider(),
            new LinuxCpuInformationProvider(),
            new LinuxDiskInformationProvider(),
            new LinuxUsbInformationProvider(),
            new LinuxPciInformationProvider(),
            new LinuxGpuInformationProvider(),
            new OSXInformationProvider(),
            new VulkanInformationProvider(),
            new EasterEggInformationProvider()
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

            // ThreadAffinity.SetCurrentProcess();

            machineInformation = GetInformation(skipClockspeedTest);

            return machineInformation;
        }

        private static MachineInformation GetInformation(bool skipClockspeedTest)
        {
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
                    var sw = Stopwatch.StartNew();
                    informationProvider.GatherInformation(info);
                    Logger.LogDebug("{Provider} took {time}ms", informationProvider.GetType().Name, sw.ElapsedMilliseconds);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when collecting information from {Provider}",
                        informationProvider.GetType().Name);
                }
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

                try
                {
                    informationProvider.PostProviderUpdateInformation(info);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception when running post-update from {Provider}",
                        informationProvider.GetType().Name);
                }
            }

            machineInformation = info;

            return info;
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
                    value = (uint)(counter.NextValue() / 100.0f * value);
                    counter.Dispose();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    try
                    {
                        // KHz
                        var freq = ulong.Parse(
                            File.ReadAllText($"/sys/devices/system/cpu/cpu{i}/cpufreq/scaling_cur_freq"));

                        value = (uint)(freq / 1000);
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