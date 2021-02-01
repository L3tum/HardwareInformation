#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using HardwareInformation.Information;
using HardwareInformation.Information.Cpu;
using HardwareInformation.Information.Gpu;
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
        /// <param name="options">NULL means default all</param>
        /// <returns></returns>
        public static MachineInformation GatherInformation(bool skipClockspeedTest = true, bool invalidateCache = false,
            ILogger logger = null, GatheringOptions options = null)
        {
            Logger = logger ?? new NullLogger<MachineInformation>();

            if (machineInformation != null && lastSkipClockspeedTest == skipClockspeedTest && !invalidateCache)
            {
                Logger.LogInformation("Returning cached information");
                return machineInformation;
            }

            Options = options ?? new GatheringOptions();
            var mapper = CreateMapper();

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

            lastSkipClockspeedTest = skipClockspeedTest;
            machineInformation = new MachineInformation();
            var informationProviders = new List<InformationProvider>();

            foreach (var provider in InformationProviders)
            {
                if (provider.Available(machineInformation))
                {
                    informationProviders.Add(provider);
                }
                else
                {
                    Logger.LogWarning("{Provider} is not available", provider.GetType().Name);
                }
            }

            var info = new MachineInformation();
            
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
                information = mapper.Map(info, information);

                foreach (var fieldInfo in typeof(GatheringOptions).GetProperties(BindingFlags.Instance |
                    BindingFlags.Public))
                {
                    try
                    {
                        switch (fieldInfo.Name)
                        {
                            case nameof(GatheringOptions.GatherCpuInformation):
                            {
                                informationProvider.GatherCpuInformation(ref information);
                                break;
                            }
                            case nameof(GatheringOptions.GatherCpuSpeedInformation):
                            {
                                informationProvider.GatherCpuSpeedInformation(ref information);
                                break;
                            }
                            case nameof(GatheringOptions.GatherCpuCacheTopologyInformation):
                            {
                                informationProvider.GatherCpuCacheTopologyInformation(ref information);
                                break;
                            }
                            case nameof(GatheringOptions.GatherCpuFeatureFlagInformation):
                            {
                                informationProvider.GatherCpuFeatureFlagInformation(ref information);
                                break;
                            }
                            case nameof(GatheringOptions.GatherMainboardInformation):
                            {
                                informationProvider.GatherMainboardInformation(ref information);
                                break;
                            }
                            case nameof(GatheringOptions.GatherRamInformation):
                            {
                                informationProvider.GatherRamInformation(ref information);
                                break;
                            }
                            case nameof(GatheringOptions.GatherDiskInformation):
                            {
                                informationProvider.GatherDiskInformation(ref information);
                                break;
                            }
                            case nameof(GatheringOptions.GatherGpuInformation):
                            {
                                informationProvider.GatherGpuInformation(ref information);
                                break;
                            }
                            case nameof(GatheringOptions.GatherUsbInformation):
                            {
                                informationProvider.GatherUsbInformation(ref information);
                                break;
                            }
                            case nameof(GatheringOptions.GatherMonitorInformation):
                            {
                                informationProvider.GatherMonitorInformation(ref information);
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Exception when collecting information from {Provider}",
                            informationProvider.GetType().Name);
                    }
                }

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
                dest = mapper.Map(kvp.Value, dest);
            }

            machineInformation = dest;

            PostOperations();

            return machineInformation;
        }

        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddCollectionMappers();
                cfg.CreateMap<MachineInformation, MachineInformation>().ForAllMembers(o =>
                    o.Condition((src, dst, srcMember) => CanBeMapped(srcMember)));
                cfg.CreateMap<CPU, CPU>().ForAllMembers(o =>
                    o.Condition((src, dst, srcMember) => CanBeMapped(srcMember)));
                cfg.CreateMap<SMBios, SMBios>().ForAllMembers(o =>
                    o.Condition((src, dst, srcMember) => CanBeMapped(srcMember)));
                cfg.CreateMap<GPU, GPU>().EqualityComparison((dst, src) => dst.Name == src.Name);
                cfg.CreateMap<RAM, RAM>().EqualityComparison((dst, src) => dst.Name == src.Name);
                cfg.CreateMap<Core, Core>().EqualityComparison((dst, src) => dst.CoreId == src.CoreId);
                cfg.CreateMap<Cache, Cache>()
                    .EqualityComparison((dst, src) => dst.Level == src.Level && dst.Type == src.Type);
                cfg.CreateMap<USBDevice, USBDevice>().EqualityComparison((dst, src) => dst.DeviceID == src.DeviceID);
                cfg.CreateMap<Display, Display>().EqualityComparison((dst, src) =>
                    dst.Name == src.Name && dst.Manufacturer == src.Manufacturer);
                cfg.CreateMap<Disk, Disk>().EqualityComparison((dst, src) =>
                    dst.Caption == src.Caption && dst.Model == src.Model && dst.Vendor == src.Vendor);
                cfg.CreateMap<MachineInformation.Platforms, MachineInformation.Platforms>()
                    .ConvertUsing((src, dst) => src == MachineInformation.Platforms.Unknown ? dst : src);

                cfg.CreateMap<CPU.FeatureFlagECX, CPU.FeatureFlagECX>()
                    .ConvertUsing((src, dst) => src == CPU.FeatureFlagECX.NONE ? dst : src);
                cfg.CreateMap<CPU.FeatureFlagEDX, CPU.FeatureFlagEDX>()
                    .ConvertUsing((src, dst) => src == CPU.FeatureFlagEDX.NONE ? dst : src);
                cfg.CreateMap<CPU.ExtendedFeatureFlagsF7EBX, CPU.ExtendedFeatureFlagsF7EBX>()
                    .ConvertUsing((src, dst) => src == CPU.ExtendedFeatureFlagsF7EBX.NONE ? dst : src);
                cfg.CreateMap<CPU.ExtendedFeatureFlagsF7ECX, CPU.ExtendedFeatureFlagsF7ECX>()
                    .ConvertUsing((src, dst) => src == CPU.ExtendedFeatureFlagsF7ECX.NONE ? dst : src);
                cfg.CreateMap<CPU.ExtendedFeatureFlagsF7EDX, CPU.ExtendedFeatureFlagsF7EDX>()
                    .ConvertUsing((src, dst) => src == CPU.ExtendedFeatureFlagsF7EDX.NONE ? dst : src);
                cfg.CreateMap<AMDFeatureFlags.FeatureFlagsAPM, AMDFeatureFlags.FeatureFlagsAPM>()
                    .ConvertUsing((src, dst) => src == AMDFeatureFlags.FeatureFlagsAPM.NONE ? dst : src);
                cfg.CreateMap<AMDFeatureFlags.FeatureFlagsSVM, AMDFeatureFlags.FeatureFlagsSVM>()
                    .ConvertUsing((src, dst) => src == AMDFeatureFlags.FeatureFlagsSVM.NONE ? dst : src);
                cfg.CreateMap<AMDFeatureFlags.ExtendedFeatureFlagsF81ECX, AMDFeatureFlags.ExtendedFeatureFlagsF81ECX>()
                    .ConvertUsing((src, dst) => src == AMDFeatureFlags.ExtendedFeatureFlagsF81ECX.NONE ? dst : src);
                cfg.CreateMap<AMDFeatureFlags.ExtendedFeatureFlagsF81EDX, AMDFeatureFlags.ExtendedFeatureFlagsF81EDX>()
                    .ConvertUsing((src, dst) => src == AMDFeatureFlags.ExtendedFeatureFlagsF81EDX.NONE ? dst : src);
                cfg.CreateMap<IntelFeatureFlags.FeatureFlagsAPM, IntelFeatureFlags.FeatureFlagsAPM>()
                    .ConvertUsing((src, dst) => src == IntelFeatureFlags.FeatureFlagsAPM.NONE ? dst : src);
                cfg.CreateMap<IntelFeatureFlags.TPMFeatureFlagsEAX, IntelFeatureFlags.TPMFeatureFlagsEAX>()
                    .ConvertUsing((src, dst) => src == IntelFeatureFlags.TPMFeatureFlagsEAX.NONE ? dst : src);
                cfg.CreateMap<IntelFeatureFlags.ExtendedFeatureFlagsF81ECX, IntelFeatureFlags.ExtendedFeatureFlagsF81ECX
                    >()
                    .ConvertUsing((src, dst) => src == IntelFeatureFlags.ExtendedFeatureFlagsF81ECX.NONE ? dst : src);
                cfg.CreateMap<IntelFeatureFlags.ExtendedFeatureFlagsF81EDX, IntelFeatureFlags.ExtendedFeatureFlagsF81EDX
                    >()
                    .ConvertUsing((src, dst) => src == IntelFeatureFlags.ExtendedFeatureFlagsF81EDX.NONE ? dst : src);

                cfg.CreateMap<DeviceType, DeviceType>()
                    .ConvertUsing((src, dst) => src == DeviceType.UNKNOWN ? dst : src);
            });

            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }

        private static bool CanBeMapped(object src)
        {
            if (src == null)
            {
                return false;
            }

            if (src is string srcStr && string.IsNullOrEmpty(srcStr))
            {
                return false;
            }

            if (src is uint srcUint && srcUint == 0)
            {
                return false;
            }

            if (src is int srcInt && srcInt == 0)
            {
                return false;
            }

            if (src is ulong srcUlong && srcUlong == 0)
            {
                return false;
            }

            if (src is long srcLong && srcLong == 0)
            {
                return false;
            }

            if (src is double srcDouble && srcDouble == 0)
            {
                return false;
            }

            return true;
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