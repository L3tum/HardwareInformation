using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using HardwareInformation.Information.Cpu;
using RyzenMasterBindings;

namespace HardwareInformation.Providers.Windows
{
    internal class RyzenMasterProvider : InformationProvider
    {
        private static bool hasLoadedNativeLibraries;
        private static bool hasSuccessfullyLoadedNativeLibraries;
        private static Platform platform;
        private static Cpu cpu;
        private static Bios bios;

        public override bool Available(MachineInformation information)
        {
            if (!OperatingSystem.IsWindows())
            {
                return false;
            }

            if (!hasLoadedNativeLibraries)
            {
                var success = RyzenMasterLibrary.Init(MachineInformationGatherer.Logger);
                hasLoadedNativeLibraries = true;
                hasSuccessfullyLoadedNativeLibraries = success;

                if (success)
                {
                    platform = Platform.GetPlatform();
                    success = platform.Init();
                    hasSuccessfullyLoadedNativeLibraries = success;

                    if (success)
                    {
                        GetDevices();
                    }
                }
            }

            return hasSuccessfullyLoadedNativeLibraries;
        }

        [SupportedOSPlatform("windows")]
        public override void PostProviderUpdateInformation(ref MachineInformation information)
        {
            cpu.Dispose();
            bios.Dispose();
            platform.Dispose();
            RyzenMasterLibrary.UnInit();
        }

        [SupportedOSPlatform("windows")]
        public override void GatherCpuCacheTopologyInformation(ref MachineInformation information)
        {
            if (cpu == null)
            {
                return;
            }

            var cacheInfo = cpu.GetL1InstructionCacheInfo();

            if (cacheInfo != null)
            {
                var info = cacheInfo.Value;
                var cache = information.Cpu.Caches.FirstOrDefault(cache =>
                    cache.Level == Cache.CacheLevel.LEVEL1 && cache.Type == Cache.CacheType.INSTRUCTION);

                if (cache == null)
                {
                    cache = new Cache
                    {
                        Level = Cache.CacheLevel.LEVEL1,
                        Type = Cache.CacheType.INSTRUCTION
                    };
                    var caches = new List<Cache>();
                    caches.AddRange(information.Cpu.Caches);
                    caches.Add(cache);
                    information.Cpu.Caches = caches;
                }

                cache.Associativity = (uint) info.Associativity;
                cache.Partitions = (uint) info.Lines;
                cache.LineSize = (uint) info.LineSize;
                cache.Capacity = (ulong) (info.Size * 1024uL);
                cache.CapacityHRF = Util.FormatBytes(cache.Capacity);
            }
        }

        [SupportedOSPlatform("windows")]
        public override void GatherCpuInformation(ref MachineInformation information)
        {
            if (cpu == null)
            {
                return;
            }

            information.Cpu.Vendor = cpu.GetVendor();
            information.Cpu.Socket = cpu.GetPackage();
            information.Cpu.Chipset = cpu.GetChipsetName();
            information.Cpu.PhysicalCores = cpu.GetCoreCount() switch
            {
                null => 0,
                uint number when number != null => number,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        [SupportedOSPlatform("windows")]
        private void GetDevices()
        {
            using var deviceManager = platform.GetDeviceManager();
            var count = deviceManager.GetTotalDeviceCount();

            for (var i = 0uL; i < count; i++)
            {
                var device = deviceManager.GetDevice(i);

                if (device.GetDeviceType() == DeviceType.DT_CPU)
                {
                    cpu = device.AsCpu();
                }
                else if (device.GetDeviceType() == DeviceType.DT_BIOS)
                {
                    bios = device.AsBios();
                }
            }
        }
    }
}