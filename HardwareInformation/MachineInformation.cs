#region using

using System;
using System.Collections.Generic;

#endregion

namespace HardwareInformation
{
	public class MachineInformation
	{
		public MachineInformation()
		{
			Cpu = new CPU();
			RAMSticks = new List<RAM>();
			SmBios = new SMBios();
		}

		public OperatingSystem OperatingSystem { get; set; }

		public CPU Cpu { get; set; }

		public SMBios SmBios { get; set; }

		public List<RAM> RAMSticks { get; set; }

		public class CPU
		{
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

			public ExtendedFeatureFlagsF7EBX ExtendedFeatureFlagsF7One { get; set; }

			public ExtendedFeatureFlagsF7ECX ExtendedFeatureFlagsF7Two { get; set; }

			public ExtendedFeatureFlagsF7EDX ExtendedFeatureFlagsF7Three { get; set; }

			public uint PhysicalCores { get; set; }

			public uint LogicalCores { get; set; }

			public uint Nodes { get; set; }

			public uint LogicalCoresPerNode { get; set; }

			public string Architecture { get; set; }

			public string Caption { get; set; }

			public string Name { get; set; }

			public string Vendor { get; set; }

			public uint Stepping { get; set; }

			public uint Model { get; set; }

			public uint Family { get; set; }

			public ProcessorType Type { get; set; }

			public FeatureFlagEDX FeatureFlagsOne { get; set; }

			public FeatureFlagECX FeatureFlagsTwo { get; set; }

			public uint MaxCpuIdFeatureLevel { get; set; }

			public uint MaxCpuIdExtendedFeatureLevel { get; set; }

			public uint MaxClockSpeed { get; set; }

			public uint NormalClockSpeed { get; set; }

			public List<Core> Cores { get; set; } = new List<Core>();

			public AMDFeatureFlags AMDFeatureFlags { get; set; } = new AMDFeatureFlags();

			public IntelFeatureFlags IntelFeatureFlags { get; set; } = new IntelFeatureFlags();

			public string Socket { get; set; }

			public List<Cache> Caches { get; set; } = new List<Cache>();
		}

		public class RAM
		{
			/// <summary>
			///     Speed in MHz
			/// </summary>
			public uint Speed { get; set; }

			public string Manfucturer { get; set; }

			/// <summary>
			///     Capacity in bytes
			/// </summary>
			public ulong Capacity { get; set; }

			public string CapacityHRF { get; set; }

			public string Name { get; set; }

			public string PartNumber { get; set; }

			public FormFactors FormFactor { get; set; }

			public enum FormFactors
			{
				UNKNOWN = 0,
				OTHER = 1,
				SIP = 2,
				DIP = 3,
				ZIP = 4,
				SOJ = 5,
				PROPRIETARY = 6,
				SIMM = 7,
				DIMM = 8,
				TSOP = 9,
				PGA = 10,
				RIMM = 11,
				SODIMM = 12,
				SRIMM = 13,
				SMD = 14,
				SSMP = 15,
				QFP = 16,
				TQFP = 17,
				SOIC = 18,
				LCC = 19,
				PLCC = 20,
				BGA = 21,
				FPBGA = 22,
				LGA = 23
			}
		}

		/// <summary>
		///     BIOS and Mainboard information
		/// </summary>
		public class SMBios
		{
			public string BIOSVersion { get; set; }

			public string BIOSVendor { get; set; }

			public string BIOSCodename { get; set; }

			public string BoardVendor { get; set; }

			public string BoardName { get; set; }

			public string BoardVersion { get; set; }
		}

		/// <summary>
		///     Feature flags only present on AMD CPUs
		/// </summary>
		public class AMDFeatureFlags
		{
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

			public ExtendedFeatureFlagsF81ECX ExtendedFeatureFlagsF81One { get; set; }

			public ExtendedFeatureFlagsF81EDX ExtendedFeatureFlagsF81Two { get; set; }

			public FeatureFlagsSVM FeatureFlagsSvm { get; set; }

			public FeatureFlagsAPM FeatureFlagsApm { get; set; }
		}

		/// <summary>
		///     Feature flags only present on Intel CPUs
		/// </summary>
		public class IntelFeatureFlags
		{
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

			public TPMFeatureFlagsEAX TPMFeatureFlags { get; set; }

			public ExtendedFeatureFlagsF81ECX ExtendedFeatureFlagsF81One { get; set; }

			public ExtendedFeatureFlagsF81EDX ExtendedFeatureFlagsF81Two { get; set; }

			public FeatureFlagsAPM FeatureFlagsApm { get; set; }
		}

		/// <summary>
		///     Different CPU vendor strings.
		/// </summary>
		public static class Vendors
		{
			public static string ACRN = "ACRNACRNACRN";
			public static string AMD = "AuthenticAMD";
			public static string AMD_LEGACY = "AMDisbetter!";
			public static string Bhyve = "bhyve bhyve";
			public static string Centaur = "CentaurHauls";
			public static string Cyrix = "CyrixInstead";
			public static string Hygon = "HygonGenuine";
			public static string HyperV = "Microsoft Hv";
			public static string Intel = "GenuineIntel";
			public static string KVM = "KVMKVMKVM";
			public static string NexGen = "NexGenDriven";
			public static string NS = "Geode by NSC";
			public static string Parallels = " lrpepyh vr"; // RIP
			public static string Rise = "RiseRiseRise";
			public static string SiS = "SiS SiS SiS";
			public static string Transmeta_1 = "TransmetaCPU";
			public static string Transmeta_2 = "GenuineTMx86";
			public static string UMC = "UMC UMC UMC";
			public static string VIA = "VIA VIA VIA";
			public static string VMware = "VMwareVMware";
			public static string Vortex = "Vortex86 SoC"; // Johnny English was right!
			public static string XenHVM = "XenVMMXenVMM";
		}

		public class Core
		{
			/// <summary>
			///     Core "ID"
			/// </summary>
			public uint Number { get; set; }

			/// <summary>
			///     Maximum reached clock speed in MHz. This measurement can be disabled.
			/// </summary>
			public uint MaxClockSpeed { get; set; }

			/// <summary>
			///     Nominal clock speed in MHz (without downclocking, turbo, pbo etc.)
			/// </summary>
			public uint NormalClockSpeed { get; set; }

			/// <summary>
			///     Reference maximum frequency as reported by CPUID 0x16
			/// </summary>
			public uint ReferenceMaxClockSpeed { get; set; }

			/// <summary>
			///     Reference base frequency as reported by CPUID 0x16
			/// </summary>
			public uint ReferenceNormalClockSpeed { get; set; }


			/// <summary>
			///     Reference bus frequency as reported by CPUID 0x16
			/// </summary>
			public uint ReferenceBusSpeed { get; set; }


			/// <summary>
			///		NUMA Node this core resides in
			/// </summary>
			public uint Node { get; set; } = 0;

			/// <summary>
			///		The physical core this logical core resides in
			/// </summary>
			public uint CoreId { get; set; } = 0;
		}

		public class Cache
		{
			public enum CacheLevel : uint
			{
				RESERVED = 0b0,
				LEVEL1 = 0b1,
				LEVEL2 = 0b10,
				LEVEL3 = 0b11,
				RESERVED2 = 0b100,
				RESERVED3 = 0b101,
				RESERVED4 = 0b110,
				RESERVED5 = 0b111
			}

			public enum CacheType : uint
			{
				NONE = 0x0,
				DATA = 0x1,
				INSTRUCTION = 0x2,
				UNIFIED = 0x3,
				RESERVED = 0x4,
				RESERVED2 = 0x5,
				RESERVED3 = 0x6,
				RESERVED4 = 0x7,
				RESERVED5 = 0x8,
				RESERVED6 = 0x9,
				RESERVED7 = 0xA,
				RESERVED8 = 0xB,
				RESERVED9 = 0xC,
				RESERVED10 = 0xD,
				RESERVED11 = 0xE,
				RESERVED12 = 0xF
			}

			/// <summary>
			///     Cache level (Level1, 2 or 3 Cache)
			/// </summary>
			public CacheLevel Level { get; set; }

			/// <summary>
			///     Cache type (instruction, data, unified)
			/// </summary>
			public CacheType Type { get; set; }

			/// <summary>
			///     Write-back invalidate.
			///     If 0, writing back to this cache automatically invalidates all lower-level caches of cores sharing this cache.
			///     If 1, this is not guaranteed to happen.
			///     Cast to bool this means 0 => true, 1 => false.
			/// </summary>
			public bool WBINVD { get; set; }


			/// <summary>
			///     How many cores are using this cache.
			/// </summary>
			public uint CoresPerCache { get; set; }

			/// <summary>
			///     How many times this exact cache has been found in the processor.
			/// </summary>
			public uint TimesPresent { get; set; }

			/// <summary>
			///     Capacity in bytes
			/// </summary>
			public ulong Capacity { get; set; }

			public string CapacityHRF { get; set; }

			/// <summary>
			///     0xffffffff for fully associative
			/// </summary>
			public uint Associativity { get; set; }

			/// <summary>
			///     Line size in bytes
			/// </summary>
			public uint LineSize { get; set; }

			/// <summary>
			///     Number of physical line partitions
			/// </summary>
			public uint Partitions { get; set; }

			/// <summary>
			///     Number of sets
			/// </summary>
			public uint Sets { get; set; }

			public override bool Equals(object obj)
			{
				if (obj.GetType() != typeof(Cache))
				{
					return false;
				}

				var cache = (Cache) obj;

				return cache.WBINVD == WBINVD && cache.LineSize == LineSize && cache.Associativity == Associativity &&
				       cache.Capacity == Capacity && cache.CoresPerCache == CoresPerCache && cache.Level == Level &&
				       cache.Type == Type && cache.Sets == Sets && cache.Partitions == Partitions;
			}
		}
	}
}