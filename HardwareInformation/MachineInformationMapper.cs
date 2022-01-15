using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HardwareInformation.Information;
using HardwareInformation.Information.Cpu;
using HardwareInformation.Information.Gpu;
using HardwareInformation.Providers;

namespace HardwareInformation
{
    internal static class MachineInformationMapper
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static void MapMachineInformation(ref MachineInformation dest, MachineInformation src)
        {
            dest.Disks = MapList(dest.Disks, src.Disks);
            dest.Displays = MapList(dest.Displays, src.Displays);
            dest.Gpus = MapList(dest.Gpus, src.Gpus);
            dest.SmBios = MapSmBios(dest.SmBios, src.SmBios);
            dest.UsbDevices = MapList(dest.UsbDevices, src.UsbDevices);
            dest.RAMSticks = MapList(dest.RAMSticks, src.RAMSticks);
            dest.Platform = src.Platform != MachineInformation.Platforms.Unknown ? src.Platform : dest.Platform;
            dest.OperatingSystem = src.OperatingSystem.Platform != PlatformID.Other
                ? src.OperatingSystem
                : dest.OperatingSystem;
            dest.Cpu = MapCpuInformation(dest.Cpu, src.Cpu);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        internal static SMBios MapSmBios(SMBios dest, SMBios src)
        {
            if (dest == null)
            {
                return src;
            }

            dest.BoardName = MapString(dest.BoardName, src.BoardName);
            dest.BoardVendor = MapString(dest.BoardVendor, src.BoardVendor);
            dest.BoardVersion = MapString(dest.BoardVersion, src.BoardVersion);
            dest.BIOSCodename = MapString(dest.BIOSCodename, src.BIOSCodename);
            dest.BIOSVendor = MapString(dest.BIOSVendor, src.BIOSVendor);
            dest.BIOSVersion = MapString(dest.BIOSVersion, src.BIOSVersion);

            return dest;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        internal static CPU MapCpuInformation(CPU dest, CPU src)
        {
            if (dest == null)
            {
                return src;
            }

            dest.Architecture = MapString(dest.Architecture, src.Architecture);
            dest.Caches = MapList(dest.Caches, src.Caches);
            dest.Caption = MapString(dest.Caption, src.Caption);
            dest.Chipset = MapString(dest.Chipset, src.Chipset);
            dest.Cores = MapList(dest.Cores, src.Cores);
            dest.Family = MapUint(dest.Family, src.Family);
            dest.Model = MapUint(dest.Model, src.Model);
            dest.Name = MapString(dest.Name, src.Name);
            dest.Nodes = MapUint(dest.Nodes, src.Nodes);
            dest.Socket = MapString(dest.Socket, src.Socket);
            dest.Stepping = MapUint(dest.Stepping, src.Stepping);
            dest.Type = src.Type == CPU.ProcessorType.Reserved ? dest.Type : src.Type;
            dest.Vendor = MapString(dest.Vendor, src.Vendor);
            dest.LogicalCores = MapUint(dest.LogicalCores, src.LogicalCores);
            dest.PhysicalCores = MapUint(dest.PhysicalCores, src.PhysicalCores);
            dest.FeatureFlagsOne |= src.FeatureFlagsOne;
            dest.FeatureFlagsTwo |= src.FeatureFlagsTwo;
            dest.IntelFeatureFlags = MapIntelFeatureFlags(dest.IntelFeatureFlags, src.IntelFeatureFlags);
            dest.MaxClockSpeed = MapUint(dest.MaxClockSpeed, src.MaxClockSpeed);
            dest.NormalClockSpeed = MapUint(dest.NormalClockSpeed, src.NormalClockSpeed);
            dest.LogicalCoresPerNode = MapUint(dest.LogicalCoresPerNode, src.LogicalCoresPerNode);
            dest.AMDFeatureFlags = MapAmdFeatureFlags(dest.AMDFeatureFlags, src.AMDFeatureFlags);
            dest.ExtendedFeatureFlagsF7One |= src.ExtendedFeatureFlagsF7One;
            dest.ExtendedFeatureFlagsF7Three |= src.ExtendedFeatureFlagsF7Three;
            dest.ExtendedFeatureFlagsF7Two |= src.ExtendedFeatureFlagsF7Two;
            dest.MaxCpuIdFeatureLevel = MapUint(dest.MaxCpuIdFeatureLevel, src.MaxCpuIdFeatureLevel);
            dest.MaxCpuIdExtendedFeatureLevel =
                MapUint(dest.MaxCpuIdExtendedFeatureLevel, src.MaxCpuIdExtendedFeatureLevel);

            return dest;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        internal static AMDFeatureFlags MapAmdFeatureFlags(AMDFeatureFlags dest, AMDFeatureFlags src)
        {
            if (dest == null)
            {
                return src;
            }

            dest.FeatureFlagsApm |= src.FeatureFlagsApm;
            dest.FeatureFlagsSvm |= src.FeatureFlagsSvm;
            dest.ExtendedFeatureFlagsF81One |= src.ExtendedFeatureFlagsF81One;
            dest.ExtendedFeatureFlagsF81Two |= src.ExtendedFeatureFlagsF81Two;

            return dest;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        internal static IntelFeatureFlags MapIntelFeatureFlags(IntelFeatureFlags dest, IntelFeatureFlags src)
        {
            if (dest == null)
            {
                return src;
            }

            dest.FeatureFlagsApm |= src.FeatureFlagsApm;
            dest.ExtendedFeatureFlagsF81One |= src.ExtendedFeatureFlagsF81One;
            dest.ExtendedFeatureFlagsF81Two |= src.ExtendedFeatureFlagsF81Two;
            dest.TPMFeatureFlags |= src.TPMFeatureFlags;

            return dest;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static IReadOnlyList<Core> MapCoreList(IReadOnlyList<Core> dest, IReadOnlyList<Core> src)
        {
            var finalCores = new List<Core>();

            foreach (var srcCore in src)
            {
                var destCore = dest.FirstOrDefault(finalCore => finalCore.Number == srcCore.Number);

                if (destCore == null)
                {
                    finalCores.Add(srcCore);
                }
                else
                {
                    destCore = MapCore(destCore, srcCore);
                    finalCores.Add(destCore);
                }
            }

            foreach (var destCore in dest)
            {
                if (finalCores.All(finalCore => finalCore.Number != destCore.Number))
                {
                    finalCores.Add(destCore);
                }
            }

            return finalCores.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static IReadOnlyList<Cache> MapCacheList(IReadOnlyList<Cache> dest, IReadOnlyList<Cache> src)
        {
            var finalCaches = new List<Cache>();

            foreach (var srcCache in src)
            {
                var destCache = dest.FirstOrDefault(destCached =>
                    destCached.Level == srcCache.Level && destCached.Type == srcCache.Type);

                if (destCache == null)
                {
                    finalCaches.Add(srcCache);
                }
                else
                {
                    destCache.Associativity = MapUint(destCache.Associativity, srcCache.Associativity);
                    destCache.Capacity = MapUlong(destCache.Capacity, srcCache.Capacity);
                    destCache.Partitions = MapUint(destCache.Partitions, srcCache.Partitions);
                    destCache.Sets = MapUint(destCache.Sets, srcCache.Sets);
                    destCache.LineSize = MapUint(destCache.LineSize, srcCache.LineSize);
                    destCache.TimesPresent = MapUint(destCache.TimesPresent, srcCache.TimesPresent);
                    destCache.CoresPerCache = MapUint(destCache.CoresPerCache, srcCache.CoresPerCache);
                    destCache.WBINVD = srcCache.WBINVD ? srcCache.WBINVD : destCache.WBINVD;
                    destCache.CapacityHRF = Util.FormatBytes(destCache.Capacity);
                    finalCaches.Add(destCache);
                }
            }

            foreach (var cache in dest)
            {
                if (finalCaches.All(finalCache => finalCache.Level != cache.Level || finalCache.Type != cache.Type))
                {
                    finalCaches.Add(cache);
                }
            }

            return finalCaches.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static IReadOnlyList<Disk> MapDiskList(IReadOnlyList<Disk> dest, IReadOnlyList<Disk> src)
        {
            var finalDisks = new List<Disk>(dest.Count == 0 ? src.Count : dest.Count);

            foreach (var disk in src)
            {
                var destDisk = dest.FirstOrDefault(destDisked => destDisked.DeviceID == disk.DeviceID);

                if (destDisk == null)
                {
                    finalDisks.Add(disk);
                }
                else
                {
                    destDisk.Capacity = MapUlong(destDisk.Capacity, disk.Capacity);
                    destDisk.Caption = MapString(destDisk.Caption, disk.Caption);
                    destDisk.Model = MapString(destDisk.Model, disk.Model);
                    destDisk.Partitions = MapUint(destDisk.Partitions, disk.Partitions);
                    destDisk.Vendor = MapString(destDisk.Vendor, disk.Vendor);
                    destDisk.DeviceID = MapString(destDisk.DeviceID, disk.DeviceID);
                    destDisk.CapacityHRF = Util.FormatBytes(destDisk.Capacity);
                    finalDisks.Add(destDisk);
                }
            }

            foreach (var disk in dest)
            {
                if (finalDisks.All(finalDisk => finalDisk.DeviceID != disk.DeviceID))
                {
                    finalDisks.Add(disk);
                }
            }

            return finalDisks.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static IReadOnlyList<Display> MapDisplayList(IReadOnlyList<Display> dest, IReadOnlyList<Display> src)
        {
            var finalDisplays = new List<Display>(dest.Count == 0 ? src.Count : dest.Count);

            foreach (var display in src)
            {
                var destDisplay = dest.FirstOrDefault(destDisplayed => destDisplayed.DeviceID == display.DeviceID);

                if (destDisplay == null)
                {
                    finalDisplays.Add(display);
                }
                else
                {
                    destDisplay.Name = MapString(destDisplay.Name, display.Name);
                    destDisplay.Manufacturer = MapString(destDisplay.Manufacturer, display.Manufacturer);
                    finalDisplays.Add(destDisplay);
                }
            }

            foreach (var display in dest)
            {
                if (finalDisplays.All(finalDisplay => finalDisplay.DeviceID != display.DeviceID))
                {
                    finalDisplays.Add(display);
                }
            }

            return finalDisplays.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static IReadOnlyList<GPU> MapGpuList(IReadOnlyList<GPU> dest, IReadOnlyList<GPU> src)
        {
            var finalGpus = new List<GPU>(dest.Count == 0 ? src.Count : dest.Count);

            if (dest.Any(gpu => gpu.DeviceID == null))
            {
                var temp = dest;
                dest = src;
                src = temp;
            }

            foreach (var gpu in src)
            {
                if (gpu.DeviceID == null)
                {
                    foreach (var destGpu in dest)
                    {
                        if (destGpu.Vendor == gpu.Vendor && destGpu.Caption == gpu.Caption)
                        {
                            finalGpus.Add(MapGpu(destGpu, gpu));
                        }
                    }
                }
                else
                {
                    var destGpu = dest.FirstOrDefault(destGpued => destGpued.DeviceID == gpu.DeviceID);

                    if (destGpu == null)
                    {
                        finalGpus.Add(gpu);
                    }
                    else
                    {
                        finalGpus.Add(MapGpu(destGpu, gpu));
                    }
                }
            }

            foreach (var gpu in dest)
            {
                if (gpu.DeviceID == null)
                {
                    continue;
                }

                if (finalGpus.All(finalGpu => finalGpu.DeviceID != gpu.DeviceID))
                {
                    finalGpus.Add(gpu);
                }
            }

            return finalGpus.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static GPU MapGpu(GPU dest, GPU src)
        {
            dest.Description = MapString(dest.Description, src.Description);
            dest.Name = MapString(dest.Name, src.Name);
            dest.Status = MapString(dest.Status, src.Status);
            dest.DriverDate = MapString(dest.DriverDate, src.DriverDate);
            dest.DriverVersion = MapString(dest.DriverVersion, src.DriverVersion);
            dest.AvailableVideoMemory = MapUlong(dest.AvailableVideoMemory, src.AvailableVideoMemory);
            dest.SupportedVulkanApiVersion = MapString(dest.SupportedVulkanApiVersion, src.SupportedVulkanApiVersion);
            dest.AvailableVideoMemoryHRF = Util.FormatBytes(dest.AvailableVideoMemory);
            dest.Type = dest.Type == DeviceType.UNKNOWN ? src.Type : dest.Type;
            dest.VendorID = VulkanInformationProvider.GetVendorNameFromVendorId(uint.Parse(dest.VendorID)) != "Unknown"
                ? dest.VendorID
                : src.VendorID;
            dest.DeviceID = VulkanInformationProvider.GetVendorNameFromVendorId(uint.Parse(dest.VendorID)) != "Unknown"
                ? dest.DeviceID
                : src.DeviceID;
            dest.Vendor = VulkanInformationProvider.GetVendorNameFromVendorId(uint.Parse(dest.VendorID)) != "Unknown"
                ? dest.Vendor
                : src.Vendor;

            return dest;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static IReadOnlyList<T> MapList<T>(IReadOnlyList<T> dest, IReadOnlyList<T> src)
        {
            if (dest == null)
            {
                return src;
            }

            if (src == null)
            {
                return dest;
            }

            if (dest.Count == 0)
            {
                return src;
            }

            if (typeof(T) == typeof(Core))
            {
                var destCores = (IReadOnlyList<Core>) dest;
                var srcCores = (IReadOnlyList<Core>) src;

                return (IReadOnlyList<T>) MapCoreList(destCores, srcCores);
            }

            if (typeof(T) == typeof(Disk))
            {
                var destDisks = (IReadOnlyList<Disk>) dest;
                var srcDisks = (IReadOnlyList<Disk>) src;

                return (IReadOnlyList<T>) MapDiskList(destDisks, srcDisks);
            }

            if (typeof(T) == typeof(Cache))
            {
                var destCaches = (IReadOnlyList<Cache>) dest;
                var srcCaches = (IReadOnlyList<Cache>) src;

                return (IReadOnlyList<T>) MapCacheList(destCaches, srcCaches);
            }

            if (typeof(T) == typeof(Display))
            {
                var destDisplays = (IReadOnlyList<Display>) dest;
                var srcDisplays = (IReadOnlyList<Display>) src;

                return (IReadOnlyList<T>) MapDisplayList(destDisplays, srcDisplays);
            }

            if (typeof(T) == typeof(GPU))
            {
                var destGpus = (IReadOnlyList<GPU>) dest;
                var srcGpus = (IReadOnlyList<GPU>) src;

                return (IReadOnlyList<T>) MapGpuList(destGpus, srcGpus);
            }

            return dest;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static string MapString(string dest, string src)
        {
            if (dest == null)
            {
                return src;
            }

            if (string.IsNullOrWhiteSpace(dest) && src != null)
            {
                return src;
            }

            return dest;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static uint MapUint(uint dest, uint src)
        {
            return src == 0 ? dest : src;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static ulong MapUlong(ulong dest, ulong src)
        {
            return src == 0 ? dest : src;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [SkipLocalsInit]
        internal static Core MapCore(Core dest, Core src)
        {
            dest.Node = MapUint(dest.Node, src.Node);
            dest.CoreId = MapUint(dest.CoreId, src.CoreId);
            dest.MaxClockSpeed = MapUint(dest.MaxClockSpeed, src.MaxClockSpeed);
            dest.NormalClockSpeed = MapUint(dest.NormalClockSpeed, src.NormalClockSpeed);
            dest.ReferenceBusSpeed = MapUint(dest.ReferenceBusSpeed, src.ReferenceBusSpeed);
            dest.ReferenceMaxClockSpeed = MapUint(dest.ReferenceMaxClockSpeed, src.ReferenceMaxClockSpeed);
            dest.ReferenceNormalClockSpeed = MapUint(dest.ReferenceNormalClockSpeed, src.ReferenceNormalClockSpeed);

            return dest;
        }
    }
}