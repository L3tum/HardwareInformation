#region using

using System;
using System.Linq;
using HardwareInformation.Information;
using HardwareInformation.Information.Cpu;

#endregion

namespace HardwareInformation.Providers.X86
{
    public class AmdInformationProvider : InformationProvider
    {
        public override bool Available(MachineInformation information)
        {
            Opcode.Open();

            if (!Opcode.IsOpen)
            {
                return false;
            }

            Opcode.Cpuid(out var result, 0, 0);
            var vendorString = string.Format("{0}{1}{2}",
                string.Join("", $"{result.ebx:X}".HexStringToString().Reverse()),
                string.Join("", $"{result.edx:X}".HexStringToString().Reverse()),
                string.Join("", $"{result.ecx:X}".HexStringToString().Reverse()));

            return vendorString == Vendors.AMD || vendorString == Vendors.AMD_LEGACY;
        }

        protected override void GatherPerCpuInformation(int cpuIndex, MachineInformation information)
        {
            GatherAmdSpecificFeatureFlags(cpuIndex, information);
        }

        protected override void GatherPerCoreInformation(int cpuIndex, int coreIndex, MachineInformation information)
        {
            GatherCoreAndNodeIds(cpuIndex, coreIndex, information);
            GatherCacheTopology(cpuIndex, coreIndex, information);
        }

        public override void PostProviderUpdateInformation(MachineInformation information)
        {
            foreach (var cpu in information.Cpus)
            {
                // We only need to do this if we used the new interface <see cref="GatherCoreAndNodeIds"/>
                if (cpu.Family >= 0x15 && cpu.MaxCpuIdExtendedFeatureLevel >= 0x1E)
                {
                    cpu.PhysicalCores = (uint)cpu.Cores.Select(core => core.CoreId).Distinct().Count();
                    cpu.Nodes = (uint)cpu.Cores.Select(core => core.Node).Distinct().Count();
                    cpu.LogicalCoresPerNode = (uint)cpu.Cores.Select(core => core.Node).Count(node => node == cpu.Cores.FirstOrDefault()?.Node);
                }

                foreach (var cache in cpu.Caches)
                {
                    cache.TimesPresent = (uint)MathF.Round(cache.TimesPresent / (float)cache.LogicalCoresPerCache, MidpointRounding.AwayFromZero);
                    cache.CoresPerCache = (uint)MathF.Round(cache.LogicalCoresPerCache * (cpu.PhysicalCores / (float)cpu.LogicalCores),
                        MidpointRounding.AwayFromZero);
                }
            }
        }

        private void GatherAmdSpecificFeatureFlags(int cpuIndex, MachineInformation information)
        {
            if (information.Cpus[cpuIndex].MaxCpuIdExtendedFeatureLevel >= 1)
            {
                Opcode.Cpuid(out var result, 0x80000001, 0);

                information.Cpus[cpuIndex].AMDFeatureFlags.ExtendedFeatureFlagsF81One = (AMDFeatureFlags.ExtendedFeatureFlagsF81ECX)result.ecx;
                information.Cpus[cpuIndex].AMDFeatureFlags.ExtendedFeatureFlagsF81Two = (AMDFeatureFlags.ExtendedFeatureFlagsF81EDX)result.edx;
            }

            if (information.Cpus[cpuIndex].MaxCpuIdExtendedFeatureLevel >= 7)
            {
                Opcode.Cpuid(out var result, 0x80000007, 0);

                information.Cpus[cpuIndex].AMDFeatureFlags.FeatureFlagsApm = (AMDFeatureFlags.FeatureFlagsAPM)result.edx;
            }

            if (information.Cpus[cpuIndex].MaxCpuIdExtendedFeatureLevel >= 8)
            {
                Opcode.Cpuid(out var result, 0x80000008, 0);

                information.Cpus[cpuIndex].PhysicalCores = (result.ecx & 0xFF) + 1;

                // Check if hyper-threading is activated, in that case the number of cores reported by CPUID is the logical cores
                if (information.Cpus[cpuIndex].FeatureFlagsOne.HasFlag(CPU.FeatureFlagEDX.HTT) &&
                    information.Cpus[cpuIndex].PhysicalCores == information.Cpus[cpuIndex].LogicalCores)
                {
                    information.Cpus[cpuIndex].PhysicalCores /= 2;
                }
            }

            if (information.Cpus[cpuIndex].MaxCpuIdExtendedFeatureLevel >= 0xA)
            {
                Opcode.Cpuid(out var result, 0x8000000A, 0);

                information.Cpus[cpuIndex].AMDFeatureFlags.FeatureFlagsSvm = (AMDFeatureFlags.FeatureFlagsSVM)result.edx;
            }
        }

        private void GatherCoreAndNodeIds(int cpuIndex, int coreIndex, MachineInformation information)
        {
            // Lots of CPUs just report 0xff as max feature level so we need to check for family as well
            if (
                information.Cpus[cpuIndex].Family >= 0x15
                && information.Cpus[cpuIndex].MaxCpuIdExtendedFeatureLevel >= 0x1E
                && information.Cpus[cpuIndex].AMDFeatureFlags.ExtendedFeatureFlagsF81One.HasFlag(AMDFeatureFlags.ExtendedFeatureFlagsF81ECX.TOPOEXT)
            )
            {
                Opcode.Cpuid(out var result, 0x8000001E, 0);

                var unitId = Util.ExtractBits(result.ebx, 0, 7);
                var nodeId = Util.ExtractBits(result.ecx, 0, 7);

                information.Cpus[cpuIndex].Cores[coreIndex].CoreId = unitId;
                information.Cpus[cpuIndex].Cores[coreIndex].Node = nodeId;
            }
        }

        private void GatherCacheTopology(int cpuIndex, int coreIndex, MachineInformation information)
        {
            if (information.Cpus[cpuIndex].AMDFeatureFlags.ExtendedFeatureFlagsF81One.HasFlag(AMDFeatureFlags.ExtendedFeatureFlagsF81ECX.TOPOEXT))
            {
                GatherCacheTopologyExtended(cpuIndex, coreIndex, information);
            }
            else
            {
                GatherCacheTopologyBasic(cpuIndex, coreIndex, information);
            }
        }

        private void AddCache(int cpuIndex, MachineInformation information, Cache cache)
        {
            lock (information.Cpus[cpuIndex].Caches)
            {
                var presentCache = information.Cpus[cpuIndex].Caches.FirstOrDefault(c => c.CustomEquals(cache));

                if (presentCache == null)
                {
                    information.Cpus[cpuIndex].Caches = information.Cpus[cpuIndex].Caches.Append(cache).ToList().AsReadOnly();
                }
                else
                {
                    presentCache.TimesPresent++;
                }
            }
        }

        /// <summary>
        ///     0x8000001D feature from AMD for extended topology information
        /// </summary>
        /// <param name="cpuIndex"></param>
        /// <param name="coreIndex"></param>
        /// <param name="information"></param>
        private void GatherCacheTopologyExtended(int cpuIndex, int coreIndex, MachineInformation information)
        {
            var ecx = 0u;

            // It is very unfathomable that there could ever be 100 cache levels but better make this limit too high than too low
            for (var i = 0; i < 100; i++)
            {
                Opcode.Cpuid(out var result, 0x8000001D, ecx);

                var type = (Cache.CacheType)(result.eax & 0xF);

                // Null, no more caches
                if (type == Cache.CacheType.NONE)
                {
                    break;
                }

                var cache = new Cache
                {
                    LogicalCoresPerCache = ((result.eax & 0x3FFC000) >> 14) + 1u,
                    Level = (Cache.CacheLevel)((result.eax & 0xF0) >> 5),
                    Type = type,
                    LineSize = (result.ebx & 0xFFF) + 1u,
                    WBINVD = (result.edx & 0b1) == 0,
                    Sets = (result.eax & 0b1000000000) == 1 ? 0x1 : result.ecx + 1,
                    Partitions = ((result.ebx & 0x3FF000) >> 12) + 1,
                    Associativity = (result.eax & 0b1000000000) == 1
                        ? 0xffffffff
                        : ((result.ebx & 0x7FE00000) >> 22) + 1
                };

                cache.Capacity = cache.LineSize * cache.Associativity * cache.Partitions * cache.Sets;
                cache.CapacityHRF = Util.FormatBytes(cache.Capacity);

                AddCache(cpuIndex, information, cache);

                ecx++;
            }
        }

        private void GatherCacheTopologyBasic(int cpuIndex, int coreIndex, MachineInformation information)
        {
            // Support for L1 Caches
            if (information.Cpus[cpuIndex].MaxCpuIdExtendedFeatureLevel >= 5)
            {
                Opcode.Cpuid(out var result, 0x80000005, 0);

                var l1DataCacheSize = ((result.ecx & 0xff000000) >> 24) * 1000uL; // KB to bytes
                var l1DataCacheAssociativity = (result.ecx & 0xff0000) >> 16;
                var l1DataCacheLineSize = result.ecx & 0xff;

                var cache = new Cache
                {
                    Type = Cache.CacheType.DATA,
                    LineSize = l1DataCacheLineSize,
                    Capacity = l1DataCacheSize,
                    Associativity = l1DataCacheAssociativity,
                    CapacityHRF = Util.FormatBytes(l1DataCacheSize)
                };

                AddCache(cpuIndex, information, cache);

                var l1InstCacheSize = ((result.edx & 0xff000000) >> 24) * 1000uL; // KB to bytes
                var l1InstCacheAssociativity = (result.edx & 0xff0000) >> 16;
                var l1InstCacheLineSize = result.edx & 0xff;

                cache = new Cache
                {
                    Type = Cache.CacheType.INSTRUCTION,
                    LineSize = l1InstCacheLineSize,
                    Capacity = l1InstCacheSize,
                    Associativity = l1InstCacheAssociativity,
                    CapacityHRF = Util.FormatBytes(l1InstCacheSize)
                };

                AddCache(cpuIndex, information, cache);
            }

            // Support for L2 and L3 caches (usually same as L1, but better safe than sorry)
            if (information.Cpus[cpuIndex].MaxCpuIdExtendedFeatureLevel >= 6)
            {
                Opcode.Cpuid(out var result, 0x80000006, 0);

                var l2CacheSize = ((result.ecx & 0xffff0000) >> 16) * 1000uL; //KB to bytes
                var l2CacheAssociativity = (result.ecx & 0xF000) >> 12;
                var l2CacheLineSize = result.ecx & 0xFF;

                var l2Cache = new Cache
                {
                    Type = Cache.CacheType.UNIFIED,
                    LineSize = l2CacheLineSize,
                    Capacity = l2CacheSize,
                    Associativity = l2CacheAssociativity,
                    CapacityHRF = Util.FormatBytes(l2CacheSize)
                };

                AddCache(cpuIndex, information, l2Cache);

                var l3CacheSize = (ulong)((result.edx & 0xFFFC0000) >> 18);

                // times 512 KB
                l3CacheSize *= 512000uL;

                var l3CacheAssociativity = (result.edx & 0xF000) >> 12;
                var l3CacheLineSize = result.ecx & 0xFF;

                var l3Cache = new Cache
                {
                    Type = Cache.CacheType.UNIFIED,
                    LineSize = l3CacheLineSize,
                    Capacity = l3CacheSize,
                    Associativity = l3CacheAssociativity,
                    CapacityHRF = Util.FormatBytes(l3CacheSize)
                };

                AddCache(cpuIndex, information, l3Cache);
            }
        }
    }
}