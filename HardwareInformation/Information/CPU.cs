#region using

using System;
using System.Collections.Generic;
using HardwareInformation.Information.Cpu;

#endregion

namespace HardwareInformation.Information
{
	/// <summary>
	///     Construct to represent a CPU
	/// </summary>
	public class CPU
    {
	    /// <summary>
	    ///     Feature flags found in CPUID 7
	    /// </summary>
	    public ExtendedFeatureFlagsF7EBX ExtendedFeatureFlagsF7One { get; internal set; }

	    /// <summary>
	    ///     Continuation of Feature flags found in CPUID 7
	    /// </summary>
	    public ExtendedFeatureFlagsF7ECX ExtendedFeatureFlagsF7Two { get; internal set; }

	    /// <summary>
	    ///     Continuation of Feature flags found in CPUID 7
	    /// </summary>
	    public ExtendedFeatureFlagsF7EDX ExtendedFeatureFlagsF7Three { get; internal set; }

	    /// <summary>
	    ///     Amount of physical cores in the CPU
	    /// </summary>
	    public uint PhysicalCores { get; internal set; }

	    /// <summary>
	    ///     Amount of logical cores in the CPU
	    /// </summary>
	    public uint LogicalCores { get; internal set; }

	    /// <summary>
	    ///     Amount of NUMA Nodes in the CPU
	    /// </summary>
	    public uint Nodes { get; internal set; }

	    /// <summary>
	    ///     Amount of logical cores per NUMA node
	    /// </summary>
	    public uint LogicalCoresPerNode { get; internal set; }

	    /// <summary>
	    ///     The architecture (e.g. x86/x64), supplied by .NET
	    /// </summary>
	    public string Architecture { get; internal set; }

	    /// <summary>
	    ///     Caption of the CPU, often including vendor and stepping
	    /// </summary>
	    public string Caption { get; internal set; }

	    /// <summary>
	    ///     The actual name of the CPU as people know it
	    /// </summary>
	    public string Name { get; internal set; }

	    /// <summary>
	    ///     Vendor/Manufacturer of the CPU. Corresponds to Vendors enum
	    /// </summary>
	    public string Vendor { get; internal set; }

	    /// <summary>
	    ///     Stepping: Current iteration of the same model, mostly to fix errata
	    /// </summary>
	    public uint Stepping { get; internal set; }

	    /// <summary>
	    ///     Model of the CPU
	    /// </summary>
	    public uint Model { get; internal set; }

	    /// <summary>
	    ///     Family of the CPU. For example 17h for Ryzen, 15h for Bulldozer.
	    /// </summary>
	    public uint Family { get; internal set; }

	    /// <summary>
	    ///     Type of processor, mostly for Intel
	    /// </summary>
	    public ProcessorType Type { get; internal set; }

	    /// <summary>
	    ///     Feature flags found in CPUID 1
	    /// </summary>
	    public FeatureFlagEDX FeatureFlagsOne { get; internal set; }

	    /// <summary>
	    ///     Continuation of Feature flags found in CPUID 1
	    /// </summary>
	    public FeatureFlagECX FeatureFlagsTwo { get; internal set; }

	    /// <summary>
	    ///     Maximum CPUID level supported by the CPU.
	    /// </summary>
	    public uint MaxCpuIdFeatureLevel { get; internal set; }

	    /// <summary>
	    ///     Maximum extended CPUID level supported by the CPU. Often returns 0xfffffff, so not a good indicator
	    /// </summary>
	    public uint MaxCpuIdExtendedFeatureLevel { get; internal set; }

	    /// <summary>
	    ///     Maximum clock speed reached by any core of the CPU. Needs the clockSpeedTest.
	    /// </summary>
	    public uint MaxClockSpeed { get; internal set; }

	    /// <summary>
	    ///     Normal clock speed as supplied by OS.
	    /// </summary>
	    public uint NormalClockSpeed { get; internal set; }

	    /// <summary>
	    ///     List of logical cores in the CPU
	    /// </summary>
	    public IReadOnlyList<Core> Cores { get; internal set; }

	    /// <summary>
	    ///     AMD-specific feature flags
	    /// </summary>
	    public AMDFeatureFlags AMDFeatureFlags { get; internal set; } = new AMDFeatureFlags();

	    /// <summary>
	    ///     Intel-specific feature flags
	    /// </summary>
	    public IntelFeatureFlags IntelFeatureFlags { get; internal set; } = new IntelFeatureFlags();

	    /// <summary>
	    ///     The socket the CPU is installed in/needs
	    /// </summary>
	    public string Socket { get; internal set; }

	    /// <summary>
	    ///     List of caches in the CPU
	    /// </summary>
	    public IReadOnlyList<Cache> Caches { get; internal set; }
#pragma warning disable 1591
        [Flags]
        public enum ExtendedFeatureFlagsF7EBX : uint
        {
            NONE = 0b0,
            FSGSBASE = 0b1,
            TSC_ADJ = 0b10,
            SGX = 0b100,
            BMI1 = 0b1000,
            HLE = 0b10000,
            AVX2 = 0b100000,
            RESERVED = 0b1000000,
            SMEP = 0b10000000,
            BMI2 = 0b100000000,
            ERMS = 0b1000000000,
            INVPCID = 0b10000000000,
            RTM = 0b100000000000,
            PQM = 0b1000000000000,
            FPU_DEP = 0b10000000000000,
            MPX = 0b100000000000000,
            PGE = 0b1000000000000000,
            AVX512_F = 0b10000000000000000,
            AVX512_DQ = 0b100000000000000000,
            RDSEED = 0b1000000000000000000,
            ADX = 0b10000000000000000000,
            SMAP = 0b100000000000000000000,
            AVX512_IFMA = 0b1000000000000000000000,
            PCOMMIT = 0b10000000000000000000000,
            CLFLUSHOPT = 0b100000000000000000000000,
            CLWB = 0b1000000000000000000000000,
            INTEL_PT = 0b10000000000000000000000000,
            AVX512_PF = 0b100000000000000000000000000,
            AVX512_ER = 0b1000000000000000000000000000,
            AVX512_CD = 0b10000000000000000000000000000,
            SHA = 0b100000000000000000000000000000,
            AVX512_BW = 0b1000000000000000000000000000000,
            AVX512_VL = 0b10000000000000000000000000000000
        }

        [Flags]
        public enum ExtendedFeatureFlagsF7ECX : uint
        {
            NONE = 0b0,
            PREFETCHWT1 = 0b1,
            AVX512_VBMI = 0b10,
            UMIP = 0b100,
            PKU = 0b1000,
            OS_PKE = 0b10000,
            WAITPKG = 0b100000,
            AVX512_VBMI2 = 0b1000000,
            SHSTK = 0b10000000,
            GFNI = 0b100000000,
            VAES = 0b1000000000,
            VPCLMULQDQ = 0b10000000000,
            AVX512_VNNI = 0b100000000000,
            AVX512_BITALG = 0b1000000000000,
            RESERVED = 0b10000000000000,
            AVX512_VPOPCNTDQ = 0b100000000000000,
            RESERVED2 = 0b1000000000000000,
            RESERVED3 = 0b10000000000000000,
            MAWAU_0 = 0b100000000000000000,
            MAWAU_1 = 0b1000000000000000000,
            MAWAU_2 = 0b10000000000000000000,
            MAWAU_3 = 0b100000000000000000000,
            MAWAU_4 = 0b1000000000000000000000,
            RDPID = 0b10000000000000000000000,
            RESERVED4 = 0b100000000000000000000000,
            RESERVED5 = 0b1000000000000000000000000,
            CLDEMOTE = 0b10000000000000000000000000,
            RESERVED6 = 0b100000000000000000000000000,
            MOVDIR = 0b1000000000000000000000000000,
            MOVDIR64B = 0b10000000000000000000000000000,
            ENQCMD = 0b100000000000000000000000000000,
            SGX_LC = 0b1000000000000000000000000000000,
            RESERVED7 = 0b10000000000000000000000000000000
        }

        [Flags]
        public enum ExtendedFeatureFlagsF7EDX : uint
        {
            NONE = 0b0,
            RESERVED = 0b1,
            RESERVED2 = 0b10,
            AVX512_4VNNIW = 0b100,
            AVX512_4FMAPS = 0b1000,
            FSRM = 0b10000,
            RESERVED3 = 0b100000,
            RESERVED4 = 0b1000000,
            RESERVED5 = 0b10000000,
            AVX512_VP2INTERSECT = 0b100000000,
            RESERVED6 = 0b1000000000,
            RESERVED7 = 0b10000000000,
            RESERVED8 = 0b100000000000,
            RESERVED9 = 0b1000000000000,
            RESERVED10 = 0b10000000000000,
            RESERVED11 = 0b100000000000000,
            RESERVED12 = 0b1000000000000000,
            RESERVED13 = 0b10000000000000000,
            RESERVED14 = 0b100000000000000000,
            PCONFIG = 0b1000000000000000000,
            RESERVED15 = 0b10000000000000000000,
            IBT = 0b100000000000000000000,
            RESERVED16 = 0b1000000000000000000000,
            RESERVED17 = 0b10000000000000000000000,
            RESERVED18 = 0b100000000000000000000000,
            RESERVED19 = 0b1000000000000000000000000,
            RESERVED20 = 0b10000000000000000000000000,
            IBRS_IBPB_IBC = 0b100000000000000000000000000,
            STIBP = 0b1000000000000000000000000000,
            RESERVED21 = 0b10000000000000000000000000000,
            CAPABILITIES = 0b100000000000000000000000000000,
            RESERVED22 = 0b1000000000000000000000000000000,
            SSBD = 0b10000000000000000000000000000000
        }

        [Flags]
        public enum FeatureFlagECX : uint
        {
            NONE = 0b0,
            SSE3 = 0b1,
            PCLMULQDQ = 0b10,
            DTES64 = 0b100,
            MONITOR = 0b1000,
            DS_CPL = 0b10000,
            VMX = 0b100000,
            SMX = 0b1000000,
            EST = 0b10000000,
            TM2 = 0b100000000,
            SSSE3 = 0b1000000000,
            CNXT_ID = 0b10000000000,
            SDBG = 0b100000000000,
            FMA = 0b1000000000000,
            CX16 = 0b10000000000000,
            XTPR = 0b100000000000000,
            PDCM = 0b1000000000000000,
            RESERVED = 0b10000000000000000,
            PCID = 0b100000000000000000,
            DCA = 0b1000000000000000000,
            SSE4_1 = 0b10000000000000000000,
            SSE4_2 = 0b100000000000000000000,
            X2APIC = 0b1000000000000000000000,
            MOVBE = 0b10000000000000000000000,
            POPCNT = 0b100000000000000000000000,
            TSC_DL = 0b1000000000000000000000000,
            AES = 0b10000000000000000000000000,
            XSAVE = 0b100000000000000000000000000,
            OS_XSAVE = 0b1000000000000000000000000000,
            AVX = 0b10000000000000000000000000000,
            F16C = 0b100000000000000000000000000000,
            RDRND = 0b1000000000000000000000000000000,
            HV = 0b10000000000000000000000000000000
        }

        [Flags]
        public enum FeatureFlagEDX : uint
        {
            NONE = 0b0,
            FPU = 0b1,
            VME = 0b10,
            DE = 0b100,
            PSE = 0b1000,
            TSC = 0b10000,
            MSR = 0b100000,
            PAE = 0b1000000,
            MCE = 0b10000000,
            CX8 = 0b100000000,
            APIC = 0b1000000000,
            RESERVED = 0b10000000000,
            SEP = 0b100000000000,
            MTRR = 0b1000000000000,
            PGE = 0b10000000000000,
            MCA = 0b100000000000000,
            CMOV = 0b1000000000000000,
            PAT = 0b10000000000000000,
            PSE36 = 0b100000000000000000,
            PSN = 0b1000000000000000000,
            CLFSH = 0b10000000000000000000,
            RESERVED2 = 0b100000000000000000000,
            DS = 0b1000000000000000000000,
            ACPI = 0b10000000000000000000000,
            MMX = 0b100000000000000000000000,
            FXSR = 0b1000000000000000000000000,
            SSE = 0b10000000000000000000000000,
            SSE2 = 0b100000000000000000000000000,
            SS = 0b1000000000000000000000000000,
            HTT = 0b10000000000000000000000000000,
            TM = 0b100000000000000000000000000000,
            IA64 = 0b1000000000000000000000000000000,
            PBE = 0b10000000000000000000000000000000
        }

        [Flags]
        public enum ProcessorType : uint
        {
            Original_OEM = 00,
            Intel_Overdrive = 01,
            Dual_Processor = 10,
            Reserved = 11
        }
#pragma warning restore 1591
    }
}