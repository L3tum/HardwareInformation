#region using

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HardwareInformation.Information.Cpu;

#endregion

namespace HardwareInformation.Providers
{
    internal class IntelInformationProvider : InformationProvider
    {
        public override bool Available(MachineInformation information)
        {
            if (string.IsNullOrEmpty(information.Cpu.Vendor) && Opcode.IsOpen)
            {
                Opcode.Cpuid(out var result, 0, 0);

                var vendorString = string.Format("{0}{1}{2}",
                    string.Join("", $"{result.ebx:X}".HexStringToString().Reverse()),
                    string.Join("", $"{result.edx:X}".HexStringToString().Reverse()),
                    string.Join("", $"{result.ecx:X}".HexStringToString().Reverse()));

                information.Cpu.Vendor = vendorString;
            }

            return information.Cpu.Vendor == Vendors.Intel &&
                   (RuntimeInformation.ProcessArchitecture == Architecture.X86 ||
                    RuntimeInformation.ProcessArchitecture == Architecture.X64);
        }

        public override void PostProviderUpdateInformation(ref MachineInformation information)
        {
            if (information.Cpu.PhysicalCores != 0)
            {
                foreach (var cache in information.Cpu.Caches)
                {
                    var threadsPerCore = information.Cpu.LogicalCores / information.Cpu.PhysicalCores;

                    cache.TimesPresent++;
                    cache.TimesPresent /= cache.CoresPerCache;
                    cache.CoresPerCache /= threadsPerCore;

                    if (cache.TimesPresent == 0)
                    {
                        cache.TimesPresent++;
                    }
                }
            }
        }

        public override void GatherCpuCacheTopologyInformation(ref MachineInformation information)
        {
            var supportsCacheTopologyExtensions = information.Cpu.MaxCpuIdFeatureLevel >= 4;

            if (!supportsCacheTopologyExtensions)
            {
                return;
            }

            var threads = new List<Task>();
            var caches = new List<Cache>();

            foreach (var core in information.Cpu.Cores)
            {
                threads.Add(Util.RunAffinity(1uL << (int) core.Number, () =>
                {
                    var ecx = 0u;

                    while (true)
                    {
                        Opcode.Cpuid(out var result, 4, ecx);

                        var type = (Cache.CacheType) (result.eax & 0xF);

                        // Null, no more caches
                        if (type == Cache.CacheType.NONE)
                        {
                            break;
                        }

                        var cache = new Cache
                        {
                            CoresPerCache = ((result.eax & 0x3FFC000) >> 14) + 1u,
                            Level = (Cache.CacheLevel) ((result.eax & 0xF0) >> 5),
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

                        lock (caches)
                        {
                            var orig = caches.FirstOrDefault(c => c.CustomEquals(cache));

                            if (orig == null)
                            {
                                caches.Add(cache);
                            }
                            else
                            {
                                orig.TimesPresent++;
                            }
                        }

                        ecx++;
                    }
                }));
            }

            Task.WaitAll(threads.ToArray());

            information.Cpu.Caches = caches;
        }

        public override void GatherCpuInformation(ref MachineInformation information)
        {
            if (information.Cpu.MaxCpuIdFeatureLevel >= 0x1f)
            {
                GatherNumberOfPhysicalCores(ref information);
            }
            else if (information.Cpu.MaxCpuIdFeatureLevel >= 11)
            {
                GatherNumberOfPhysicalCoresLegacy(ref information);
            }

            if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 4)
            {
                GatherCpuName(ref information);
            }
        }

        private static void GatherNumberOfPhysicalCores(ref MachineInformation information)
        {
            var ecx = 0u;
            var apicIds = new Dictionary<uint, uint>();

            while (true)
            {
                Opcode.Cpuid(out var result, 0x1f, ecx);

                var type = Util.ExtractBits(result.ecx, 8, 15);

                if (type == 0)
                {
                    break;
                }

                if (type > 1)
                {
                    continue;
                }

                var shift = Util.ExtractBits(result.eax, 0, 4);
                var coreApicId = result.edx >> (int) shift;

                if (apicIds.ContainsKey(coreApicId))
                {
                    apicIds[coreApicId]++;
                }
                else
                {
                    apicIds.Add(coreApicId, 1);
                }
            }

            information.Cpu.PhysicalCores = (uint) apicIds.Count;
        }

        private static void GatherNumberOfPhysicalCoresLegacy(ref MachineInformation information)
        {
            var threads = new List<Task>();
            var cores = 0u;

            for (var i = 0; i < information.Cpu.LogicalCores; i++)
            {
                threads.Add(Util.RunAffinity(1uL << i, () =>
                {
                    Opcode.Cpuid(out var result, 11, 0);

                    if ((result.edx & 0b1) != 1)
                    {
                        cores++;
                    }
                }));
            }

            Task.WaitAll(threads.ToArray());
            information.Cpu.PhysicalCores = cores;
        }

        private static void GatherCpuName(ref MachineInformation information)
        {
            Opcode.Cpuid(out var partOne, 0x80000002, 0);
            Opcode.Cpuid(out var partTwo, 0x80000003, 0);
            Opcode.Cpuid(out var partThree, 0x80000004, 0);

            var results = new[] {partOne, partTwo, partThree};
            var sb = new StringBuilder();

            foreach (var res in results)
            {
                sb.Append(string.Format("{0}{1}{2}{3}",
                    string.Join("", $"{res.eax:X}".HexStringToString().Reverse()),
                    string.Join("", $"{res.ebx:X}".HexStringToString().Reverse()),
                    string.Join("", $"{res.ecx:X}".HexStringToString().Reverse()),
                    string.Join("", $"{res.edx:X}".HexStringToString().Reverse())));
            }

            information.Cpu.Name = sb.ToString();
        }

        public override void GatherCpuFeatureFlagInformation(ref MachineInformation information)
        {
            if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 1)
            {
                Opcode.Cpuid(out var result, 0x80000001, 0);

                information.Cpu.IntelFeatureFlags.ExtendedFeatureFlagsF81One =
                    (IntelFeatureFlags.ExtendedFeatureFlagsF81ECX) result.ecx;
                information.Cpu.IntelFeatureFlags.ExtendedFeatureFlagsF81Two =
                    (IntelFeatureFlags.ExtendedFeatureFlagsF81EDX) result.edx;
            }

            if (information.Cpu.MaxCpuIdFeatureLevel >= 6)
            {
                Opcode.Cpuid(out var result, 6, 0);

                information.Cpu.IntelFeatureFlags.TPMFeatureFlags =
                    (IntelFeatureFlags.TPMFeatureFlagsEAX) result.eax;
            }

            if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 7)
            {
                Opcode.Cpuid(out var result, 0x80000001, 0);

                information.Cpu.IntelFeatureFlags.FeatureFlagsApm =
                    (IntelFeatureFlags.FeatureFlagsAPM) result.edx;
            }
        }
    }
}