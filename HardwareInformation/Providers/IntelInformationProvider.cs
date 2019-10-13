#region using

using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace HardwareInformation.Providers
{
	internal class IntelInformationProvider : InformationProvider
	{
		public void GatherInformation(ref MachineInformation information)
		{
			if (information.Cpu.MaxCpuIdFeatureLevel >= 6)
			{
				Opcode.Cpuid(out var result, 6, 0);

				information.Cpu.IntelFeatureFlags.TPMFeatureFlags =
					(MachineInformation.IntelFeatureFlags.TPMFeatureFlagsEAX) result.eax;
			}

			if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 1)
			{
				Opcode.Cpuid(out var result, 0x80000001, 0);

				information.Cpu.IntelFeatureFlags.ExtendedFeatureFlagsF81One =
					(MachineInformation.IntelFeatureFlags.ExtendedFeatureFlagsF81ECX) result.ecx;
				information.Cpu.IntelFeatureFlags.ExtendedFeatureFlagsF81Two =
					(MachineInformation.IntelFeatureFlags.ExtendedFeatureFlagsF81EDX) result.edx;
			}

			if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 4)
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

			if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 7)
			{
				Opcode.Cpuid(out var result, 0x80000001, 0);

				information.Cpu.IntelFeatureFlags.FeatureFlagsApm =
					(MachineInformation.IntelFeatureFlags.FeatureFlagsAPM) result.edx;
			}
		}
		 
		public bool Available(MachineInformation information)
		{
			return information.Cpu.Vendor == MachineInformation.Vendors.Intel &&
			       (RuntimeInformation.ProcessArchitecture == Architecture.X86 ||
			        RuntimeInformation.ProcessArchitecture == Architecture.X64);
		}
	}
}