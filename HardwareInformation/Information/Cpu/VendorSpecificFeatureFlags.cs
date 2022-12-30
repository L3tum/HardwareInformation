#region using

using System;

#endregion

namespace HardwareInformation.Information.Cpu
{
	/// <summary>
	///     Feature flags only present on AMD CPUs
	/// </summary>
	public class AMDFeatureFlags
    {
	    /// <summary>
	    ///     AMD-specific feature flags found in CPUID 8000_0001
	    /// </summary>
	    public ExtendedFeatureFlagsF81ECX ExtendedFeatureFlagsF81One { get; set; }

	    /// <summary>
	    ///     Continuation of AMD-specific feature flags found in CPUID 8000_0001
	    /// </summary>
	    public ExtendedFeatureFlagsF81EDX ExtendedFeatureFlagsF81Two { get; set; }

	    /// <summary>
	    ///     AMD-specific feature flags for SVM (secure virtual machine)
	    /// </summary>
	    public FeatureFlagsSVM FeatureFlagsSvm { get; set; }

	    /// <summary>
	    ///     AMD-specific feature flags for the APM
	    /// </summary>
	    public FeatureFlagsAPM FeatureFlagsApm { get; set; }
#pragma warning disable 1591
        [Flags]
        public enum ExtendedFeatureFlagsF81ECX : uint
        {
            NONE = 0b0,
            LAHF_LM = 0b1,
            CMP_LEGACY = 0b10,
            SVM = 0b100,
            EXTAPIC = 0b1000,
            CR8_LEGACY = 0b10000,
            ABM = 0b100000,
            SSE4A = 0b1000000,
            MISALIGNSSE = 0b10000000,
            THREEDNOW_PREFETCH = 0b100000000,
            OS_VW = 0b1000000000,
            IBS = 0b10000000000,
            XOP = 0b100000000000,
            SKINIT = 0b1000000000000,
            WDT = 0b10000000000000,
            RESERVED = 0b100000000000000,
            LWP = 0b1000000000000000,
            FMA4 = 0b10000000000000000,
            TCE = 0b100000000000000000,
            RESERVED2 = 0b1000000000000000000,
            NODEID_MSR = 0b10000000000000000000,
            RESERVED3 = 0b100000000000000000000,
            TBM = 0b1000000000000000000000,
            TOPOEXT = 0b10000000000000000000000,
            PERFCTR_CORE = 0b100000000000000000000000,
            PERFCTR_NB = 0b1000000000000000000000000,
            RESERVED4 = 0b10000000000000000000000000,
            DBX = 0b100000000000000000000000000,
            PERFTSC = 0b1000000000000000000000000000,
            PCX_L2I = 0b10000000000000000000000000000,
            RESERVED5 = 0b100000000000000000000000000000,
            RESERVED6 = 0b1000000000000000000000000000000,
            RESERVED7 = 0b10000000000000000000000000000000
        }

        [Flags]
        public enum ExtendedFeatureFlagsF81EDX : uint
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
            SYSCALL = 0b100000000000,
            MTRR = 0b1000000000000,
            PGE = 0b10000000000000,
            MCA = 0b100000000000000,
            CMOV = 0b1000000000000000,
            PAT = 0b10000000000000000,
            PSE36 = 0b100000000000000000,
            RESERVED2 = 0b1000000000000000000,
            MP = 0b10000000000000000000,
            NX = 0b100000000000000000000,
            RESERVED3 = 0b1000000000000000000000,
            MMXEXT = 0b10000000000000000000000,
            MMX = 0b100000000000000000000000,
            FXSR = 0b1000000000000000000000000,
            FXSR_OPT = 0b10000000000000000000000000,
            PDPE_1GB = 0b100000000000000000000000000,
            RDTSCP = 0b1000000000000000000000000000,
            RESERVED4 = 0b10000000000000000000000000000,
            LM = 0b100000000000000000000000000000,
            THREEDNOW_EXR = 0b1000000000000000000000000000000,
            THREEDNOW = 0b10000000000000000000000000000000
        }

        [Flags]
        public enum FeatureFlagsAPM : uint
        {
            NONE = 0b0,
            TS = 0b1,
            FID = 0b10,
            VID = 0b100,
            TTP = 0b1000,
            TM = 0b10000,
            RESERVED = 0b100000,
            HUNDRED_MHZ_STEPS = 0b1000000,
            HW_STATE = 0b10000000,
            TSC_INVARIANT = 0b100000000,
            CPB = 0b1000000000,
            EFF_FREQ_RO = 0b10000000000,
            PROC_FEEDBACK_INTERFACE = 0b100000000000,
            PROC_POWER_REPORTING = 0b1000000000000,
            RESERVED2 = 0b10000000000000,
            RESERVED3 = 0b100000000000000,
            RESERVED4 = 0b1000000000000000,
            RESERVED5 = 0b10000000000000000,
            RESERVED6 = 0b100000000000000000,
            RESERVED7 = 0b1000000000000000000,
            RESERVED8 = 0b10000000000000000000,
            RESERVED9 = 0b100000000000000000000,
            RESERVED10 = 0b1000000000000000000000,
            RESERVED11 = 0b10000000000000000000000,
            RESERVED12 = 0b100000000000000000000000,
            RESERVED13 = 0b1000000000000000000000000,
            RESERVED14 = 0b10000000000000000000000000,
            RESERVED15 = 0b100000000000000000000000000,
            RESERVED16 = 0b1000000000000000000000000000,
            RESERVED17 = 0b10000000000000000000000000000,
            RESERVED18 = 0b100000000000000000000000000000,
            RESERVED19 = 0b1000000000000000000000000000000,
            RESERVED20 = 0b10000000000000000000000000000000
        }

        [Flags]
        public enum FeatureFlagsSVM : uint
        {
            NONE = 0b0,
            NP = 0b1,
            LBR_VIRT = 0b10,
            SVML = 0b100,
            NRIPS = 0b1000,
            TSC_RATE_MSR = 0b10000,
            VMCB_CLEAN = 0b100000,
            FLUSH_BY_ASID = 0b1000000,
            DECODE_ASSISTS = 0b10000000,
            RESERVED = 0b100000000,
            RESERVED2 = 0b1000000000,
            PAUSE_FILTER = 0b10000000000,
            RESERVED3 = 0b100000000000,
            PAUSE_FILTER_THRESH = 0b1000000000000,
            AVIC = 0b10000000000000,
            RESERVED4 = 0b100000000000000,
            VMSAVE_VIRT = 0b1000000000000000,
            VGIF = 0b10000000000000000,
            RESERVED5 = 0b100000000000000000,
            RESERVED6 = 0b1000000000000000000,
            RESERVED7 = 0b10000000000000000000,
            RESERVED8 = 0b100000000000000000000,
            RESERVED9 = 0b1000000000000000000000,
            RESERVED10 = 0b10000000000000000000000,
            RESERVED11 = 0b100000000000000000000000,
            RESERVED12 = 0b1000000000000000000000000,
            RESERVED13 = 0b10000000000000000000000000,
            RESERVED14 = 0b100000000000000000000000000,
            RESERVED15 = 0b1000000000000000000000000000,
            RESERVED16 = 0b10000000000000000000000000000,
            RESERVED17 = 0b100000000000000000000000000000,
            RESERVED18 = 0b1000000000000000000000000000000,
            RESERVED19 = 0b10000000000000000000000000000000
        }
#pragma warning restore 1591
    }

	/// <summary>
	///     Feature flags only present on Intel CPUs
	/// </summary>
	public class IntelFeatureFlags
	{
		/// <summary>
		///     Intel-specific TPM feature flags (thermal power management)
		/// </summary>
		[Obsolete("Use FeatureFlagsTpm instead")]
		public FeatureFlagsTPM TPMFeatureFlags => FeatureFlagsTpm;

	    /// <summary>
	    ///     Intel-specific TPM feature flags (thermal power management)
	    /// </summary>
	    public FeatureFlagsTPM FeatureFlagsTpm { get; set; }

	    /// <summary>
	    ///     Feature flags only present on Intel systems, mostly just longmode.
	    /// </summary>
	    public ExtendedFeatureFlagsF81ECX ExtendedFeatureFlagsF81One { get; set; }

	    /// <summary>
	    ///     Feature flags only present on Intel systems, mostly just things to make the CPU compatible with AMD
	    /// </summary>
	    public ExtendedFeatureFlagsF81EDX ExtendedFeatureFlagsF81Two { get; set; }

	    /// <summary>
	    ///     APM feature flags for Intel, only TSC invariant right now
	    /// </summary>
	    public FeatureFlagsAPM FeatureFlagsApm { get; set; }

#pragma warning disable 1591
        [Flags]
        public enum ExtendedFeatureFlagsF81ECX : uint
        {
            NONE = 0b0,
            LAHF_LM = 0b1,
            RESERVED = 0b10,
            RESERVED2 = 0b100,
            RESERVED3 = 0b1000,
            RESERVED4 = 0b10000,
            LZNT = 0b100000,
            RESERVED5 = 0b1000000,
            RESERVED6 = 0b10000000,
            PREFETCHW = 0b100000000,
            RESERVED7 = 0b1000000000,
            RESERVED8 = 0b10000000000,
            RESERVED9 = 0b100000000000,
            RESERVED10 = 0b1000000000000,
            RESERVED11 = 0b10000000000000,
            RESERVED12 = 0b100000000000000,
            RESERVED13 = 0b1000000000000000,
            RESERVED14 = 0b10000000000000000,
            RESERVED15 = 0b100000000000000000,
            RESERVED16 = 0b1000000000000000000,
            RESERVED17 = 0b10000000000000000000,
            RESERVED18 = 0b100000000000000000000,
            RESERVED19 = 0b1000000000000000000000,
            RESERVED20 = 0b10000000000000000000000,
            RESERVED21 = 0b100000000000000000000000,
            RESERVED22 = 0b1000000000000000000000000,
            RESERVED23 = 0b10000000000000000000000000,
            RESERVED24 = 0b100000000000000000000000000,
            RESERVED25 = 0b1000000000000000000000000000,
            RESERVED26 = 0b10000000000000000000000000000,
            RESERVED27 = 0b100000000000000000000000000000,
            RESERVED28 = 0b1000000000000000000000000000000,
            RESERVED29 = 0b10000000000000000000000000000000
        }

        [Flags]
        public enum ExtendedFeatureFlagsF81EDX : uint
        {
            NONE = 0b0,
            RESERVED = 0b1,
            RESERVED1 = 0b10,
            RESERVED2 = 0b100,
            RESERVED3 = 0b1000,
            RESERVED4 = 0b10000,
            RESERVED5 = 0b100000,
            RESERVED6 = 0b1000000,
            RESERVED7 = 0b10000000,
            RESERVED8 = 0b100000000,
            RESERVED9 = 0b1000000000,
            RESERVED10 = 0b10000000000,
            SYSCALL = 0b100000000000,
            RESERVED11 = 0b1000000000000,
            RESERVED12 = 0b10000000000000,
            RESERVED13 = 0b100000000000000,
            RESERVED14 = 0b1000000000000000,
            RESERVED15 = 0b10000000000000000,
            RESERVED16 = 0b100000000000000000,
            RESERVED17 = 0b1000000000000000000,
            RESERVED18 = 0b10000000000000000000,
            NX = 0b100000000000000000000,
            RESERVED19 = 0b1000000000000000000000,
            RESERVED20 = 0b10000000000000000000000,
            RESERVED21 = 0b100000000000000000000000,
            RESERVED22 = 0b1000000000000000000000000,
            RESERVED23 = 0b10000000000000000000000000,
            PDPE_1GB = 0b100000000000000000000000000,
            RDTSCP = 0b1000000000000000000000000000,
            RESERVED24 = 0b10000000000000000000000000000,
            LM = 0b100000000000000000000000000000,
            RESERVED25 = 0b1000000000000000000000000000000,
            RESERVED26 = 0b10000000000000000000000000000000
        }

        [Flags]
        public enum FeatureFlagsAPM : uint
        {
            NONE = 0b0,
            RESERVED = 0b1,
            RESERVED1 = 0b10,
            RESERVED2 = 0b100,
            RESERVED3 = 0b1000,
            RESERVED4 = 0b10000,
            RESERVED5 = 0b100000,
            RESERVED6 = 0b1000000,
            RESERVED7 = 0b10000000,
            TSC_INVARIANT = 0b100000000,
            RESERVED8 = 0b1000000000,
            RESERVED9 = 0b10000000000,
            RESERVED10 = 0b100000000000,
            RESERVED11 = 0b1000000000000,
            RESERVED12 = 0b10000000000000,
            RESERVED13 = 0b100000000000000,
            RESERVED14 = 0b1000000000000000,
            RESERVED15 = 0b10000000000000000,
            RESERVED16 = 0b100000000000000000,
            RESERVED17 = 0b1000000000000000000,
            RESERVED18 = 0b10000000000000000000,
            RESERVED19 = 0b100000000000000000000,
            RESERVED20 = 0b1000000000000000000000,
            RESERVED21 = 0b10000000000000000000000,
            RESERVED22 = 0b100000000000000000000000,
            RESERVED23 = 0b1000000000000000000000000,
            RESERVED24 = 0b10000000000000000000000000,
            RESERVED25 = 0b100000000000000000000000000,
            RESERVED26 = 0b1000000000000000000000000000,
            RESERVED27 = 0b10000000000000000000000000000,
            RESERVED28 = 0b100000000000000000000000000000,
            RESERVED29 = 0b1000000000000000000000000000000,
            RESERVED30 = 0b10000000000000000000000000000000
        }

        [Flags]
        [Obsolete("Use FeatureFlagsTPM instead")]
        public enum TPMFeatureFlagsEAX : uint
        {
            NONE = 0b0,
            DTS = 0b1,
            TURBO = 0b10,
            ARAT = 0b100,
            RESERVED = 0b1000,
            PLN = 0b10000,
            ECMD = 0b100000,
            PTM = 0b1000000,
            HWP = 0b10000000,
            HWP_NOT = 0b100000000,
            HWP_AW = 0b1000000000,
            HWP_EPP = 0b10000000000,
            HWP_PLR = 0b100000000000,
            RESERVED2 = 0b1000000000000,
            HDC = 0b10000000000000,
            BOOST_MAX = 0b100000000000000,
            HWP_CAP = 0b1000000000000000,
            HWP_PECI = 0b10000000000000000,
            HWP_FLEX = 0b100000000000000000,
            HWP_FAST = 0b1000000000000000000,
            HWP_FEEDBACK = 0b10000000000000000000,
            HWP_IGNORE_IDLE = 0b100000000000000000000,
            RESERVED3 = 0b1000000000000000000000,
            RESERVED4 = 0b10000000000000000000000,
            RESERVED5 = 0b100000000000000000000000,
            RESERVED6 = 0b1000000000000000000000000,
            RESERVED7 = 0b10000000000000000000000000,
            RESERVED8 = 0b100000000000000000000000000,
            RESERVED9 = 0b1000000000000000000000000000,
            RESERVED10 = 0b10000000000000000000000000000,
            RESERVED11 = 0b100000000000000000000000000000,
            RESERVED12 = 0b1000000000000000000000000000000,
            RESERVED13 = 0b10000000000000000000000000000000
        }
        [Flags]
        public enum FeatureFlagsTPM : uint
        {
	        NONE = 0b0,
	        DTS = 0b1,
	        TURBO = 0b10,
	        ARAT = 0b100,
	        RESERVED = 0b1000,
	        PLN = 0b10000,
	        ECMD = 0b100000,
	        PTM = 0b1000000,
	        HWP = 0b10000000,
	        HWP_NOT = 0b100000000,
	        HWP_AW = 0b1000000000,
	        HWP_EPP = 0b10000000000,
	        HWP_PLR = 0b100000000000,
	        RESERVED2 = 0b1000000000000,
	        HDC = 0b10000000000000,
	        BOOST_MAX = 0b100000000000000,
	        HWP_CAP = 0b1000000000000000,
	        HWP_PECI = 0b10000000000000000,
	        HWP_FLEX = 0b100000000000000000,
	        HWP_FAST = 0b1000000000000000000,
	        HWP_FEEDBACK = 0b10000000000000000000,
	        HWP_IGNORE_IDLE = 0b100000000000000000000,
	        RESERVED3 = 0b1000000000000000000000,
	        RESERVED4 = 0b10000000000000000000000,
	        RESERVED5 = 0b100000000000000000000000,
	        RESERVED6 = 0b1000000000000000000000000,
	        RESERVED7 = 0b10000000000000000000000000,
	        RESERVED8 = 0b100000000000000000000000000,
	        RESERVED9 = 0b1000000000000000000000000000,
	        RESERVED10 = 0b10000000000000000000000000000,
	        RESERVED11 = 0b100000000000000000000000000000,
	        RESERVED12 = 0b1000000000000000000000000000000,
	        RESERVED13 = 0b10000000000000000000000000000000
        }
#pragma warning restore 1591
    }
}