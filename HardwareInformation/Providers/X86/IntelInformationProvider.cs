#region using

using System;
using System.Linq;
using System.Text;
using HardwareInformation.Information.Cpu;

#endregion

namespace HardwareInformation.Providers.X86;

public class IntelInformationProvider : InformationProvider
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

        return vendorString == Vendors.Intel;
    }

    protected override void GatherPerCpuInformation(int cpuIndex, MachineInformation information)
    {
        GatherCpuName(cpuIndex, information);
        GatherCpuFeatureFlagInformation(cpuIndex, information);
    }

    protected override void GatherPerCoreInformation(int cpuIndex, int coreIndex, MachineInformation information)
    {
        GatherApicCoreId(cpuIndex, coreIndex, information);
        GatherCacheTopology(cpuIndex, coreIndex, information);
    }

    public override void PostProviderUpdateInformation(MachineInformation information)
    {
        foreach (var cpu in information.Cpus)
        {
            cpu.PhysicalCores = (uint)cpu.Cores.Select(core => core.CoreId).Distinct().Count();
            cpu.Nodes = (uint)Math.Max(cpu.Cores.Select(core => core.Node).Distinct().Count(), 1);
            cpu.LogicalCoresPerNode = cpu.LogicalCores / cpu.Nodes;

            foreach (var cache in cpu.Caches)
            {
                cache.TimesPresent = (uint)MathF.Round(cache.TimesPresent / (float)cache.LogicalCoresPerCache, MidpointRounding.AwayFromZero);
                cache.CoresPerCache = (uint)MathF.Round(cache.LogicalCoresPerCache * (cpu.PhysicalCores / (float)cpu.LogicalCores),
                    MidpointRounding.AwayFromZero);
            }
        }
    }

    private void GatherCpuName(int cpuIndex, MachineInformation information)
    {
        if (information.Cpus[cpuIndex].MaxCpuIdExtendedFeatureLevel >= 4)
        {
            Opcode.Cpuid(out var partOne, 0x80000002, 0);
            Opcode.Cpuid(out var partTwo, 0x80000003, 0);
            Opcode.Cpuid(out var partThree, 0x80000004, 0);

            var results = new[] { partOne, partTwo, partThree };
            var sb = new StringBuilder();

            foreach (var res in results)
            {
                sb.Append(string.Format("{0}{1}{2}{3}",
                    string.Join("", $"{res.eax:X}".HexStringToString().Reverse()),
                    string.Join("", $"{res.ebx:X}".HexStringToString().Reverse()),
                    string.Join("", $"{res.ecx:X}".HexStringToString().Reverse()),
                    string.Join("", $"{res.edx:X}".HexStringToString().Reverse())));
            }

            information.Cpus[cpuIndex].Name = sb.ToString().Trim();
        }
    }

    private void GatherCpuFeatureFlagInformation(int cpuIndex, MachineInformation information)
    {
        if (information.Cpus[cpuIndex].MaxCpuIdExtendedFeatureLevel >= 1)
        {
            Opcode.Cpuid(out var result, 0x80000001, 0);

            information.Cpus[cpuIndex].IntelFeatureFlags.ExtendedFeatureFlagsF81One =
                (IntelFeatureFlags.ExtendedFeatureFlagsF81ECX)result.ecx;
            information.Cpus[cpuIndex].IntelFeatureFlags.ExtendedFeatureFlagsF81Two =
                (IntelFeatureFlags.ExtendedFeatureFlagsF81EDX)result.edx;
        }

        if (information.Cpus[cpuIndex].MaxCpuIdFeatureLevel >= 6)
        {
            Opcode.Cpuid(out var result, 6, 0);

            information.Cpus[cpuIndex].IntelFeatureFlags.FeatureFlagsTpm =
                (IntelFeatureFlags.FeatureFlagsTPM)result.eax;
        }

        if (information.Cpus[cpuIndex].MaxCpuIdExtendedFeatureLevel >= 7)
        {
            Opcode.Cpuid(out var result, 0x80000001, 0);

            information.Cpus[cpuIndex].IntelFeatureFlags.FeatureFlagsApm =
                (IntelFeatureFlags.FeatureFlagsAPM)result.edx;
        }
    }

    /// <summary>
    ///     We could iterate over 0x1f or 0xb here and get the whole topology...
    ///     but Intel made such a mess of this already. For example, in BIG.little designs, there seems to be an issue
    ///     that it never exits and endlessly climbs up a "virtual" topology or something bullshitty like that.
    ///     0x1f "supersedes" 0xb but like, come on. Fuck this. It's literally the same and both do not work for shit.
    ///     And what the fuck is this?
    ///     "Number of bits to shift right on x2APIC ID to get a unique topology ID of the next level type. All logical
    ///     processors with the same next level ID share current level"
    ///     That makes NO sense?! "Share current level"?! That'd mean that two logical cores with the same physical core APIC
    ///     ID would be the same logical processor.
    ///     UGH
    /// </summary>
    /// <param name="cpuIndex"></param>
    /// <param name="coreIndex"></param>
    /// <param name="information"></param>
    private void GatherApicCoreId(int cpuIndex, int coreIndex, MachineInformation information)
    {
        if (information.Cpus[cpuIndex].MaxCpuIdFeatureLevel >= 0x1f)
        {
            Opcode.Cpuid(out var result, 0x1f, 0u);
            var shift = Util.ExtractBits(result.eax, 0, 4);
            var coreApicId = result.edx >> (int)shift;
            information.Cpus[cpuIndex].Cores[coreIndex].CoreId = coreApicId;

            // We need to skip "Module"s. There is no reference in any intel documentation what a "module" is, but I guess it refers to the grouping of
            // P-Cores and E-Cores, each forming a respective module. I hope so at least.
            // Therefore we skip straight to a "Tile", which should come much closer to the meaning of "Node" we have in this Library.
            // The ecx check is a safety check.
            var ecx = 1u;
            var apicId = coreApicId;
            uint levelType;
            do
            {
                Opcode.Cpuid(out result, 0x1f, ecx);
                ecx++;
                shift = Util.ExtractBits(result.eax, 0, 4);
                apicId >>= (int)shift;
                levelType = Util.ExtractBits(result.ecx, 8, 15);
            } while ((levelType == 3 || levelType == 2 || levelType == 1) && ecx < 100);

            // Check if this CPU has a Tile (currently there are none, I think? Well, Sapphire Rapids are "right around the corner" since 2020)
            if (Util.ExtractBits(result.ecx, 8, 15) == 4)
            {
                information.Cpus[cpuIndex].Cores[coreIndex].Node = apicId;
            }
        }
        else if (information.Cpus[cpuIndex].MaxCpuIdFeatureLevel >= 0xb)
        {
            // This has no concept of "Node" so we only get the Core information from it
            Opcode.Cpuid(out var result, 0xb, 0u);
            var shift = Util.ExtractBits(result.eax, 0, 4);
            var coreApicId = result.edx >> (int)shift;
            information.Cpus[cpuIndex].Cores[coreIndex].CoreId = coreApicId;
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
    ///     This is essentially the same as AMD's extended cache topology. Who copied from whom is your exercise to figure out
    ///     :)
    ///     Why they didn't both just implement 0x4 and instead went for THREE different implementations is beyond my wildest
    ///     imaginations.
    /// </summary>
    /// <param name="cpuIndex"></param>
    /// <param name="coreIndex"></param>
    /// <param name="information"></param>
    private void GatherCacheTopology(int cpuIndex, int coreIndex, MachineInformation information)
    {
        var ecx = 0u;

        // It is very unfathomable that there could ever be 100 cache levels but better make this limit too high than too low
        for (var i = 0; i < 100; i++)
        {
            Opcode.Cpuid(out var result, 4, ecx);

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
}