#region using

using System.Linq;
using System.Text;
using HardwareInformation.Information;

#endregion

namespace HardwareInformation.Providers.X86
{
    /// <summary>
    ///     All values that are available in all (i.e. Intel and AMD) x86 CPUs
    /// </summary>
    internal class X86InformationProvider : InformationProvider
    {
        public override bool Available(MachineInformation information)
        {
            Opcode.Open();
            return Opcode.IsOpen;
        }

        protected override void GatherPerCpuInformation(int cpuIndex, MachineInformation information)
        {
            GatherCpuModelInformation(cpuIndex, information);
            IdentifyVendorAndLevelInformation(cpuIndex, information);
            IdentifyExtendedFeatureFlags(cpuIndex, information);
            IdentifyExtendedName(cpuIndex, information);
        }

        protected override void GatherPerCoreInformation(int cpuIndex, int coreIndex, MachineInformation information)
        {
            GatherCoreSpeedInformation(cpuIndex, coreIndex, information);
        }

        public override void PostProviderUpdateInformation(MachineInformation information)
        {
            foreach (var cpu in information.Cpus)
            {
                cpu.MaxClockSpeed = cpu.Cores.Max(core => core.ReferenceMaxClockSpeed);
                cpu.NormalClockSpeed = (uint)cpu.Cores.Average(core => core.ReferenceNormalClockSpeed);
            }
        }

        /// <summary>
        ///     EAX = 0, ECX = 0
        ///     EAX=maximum supported standard level
        ///     EBX/ECX/EDX=Vendor String
        /// </summary>
        /// <param name="cpuIndex"></param>
        /// <param name="information"></param>
        private void IdentifyVendorAndLevelInformation(int cpuIndex, MachineInformation information)
        {
            Opcode.Cpuid(out var result, 0, 0);
            var vendorString = string.Format("{0}{1}{2}",
                string.Join("", $"{result.ebx:X}".HexStringToString().Reverse()),
                string.Join("", $"{result.edx:X}".HexStringToString().Reverse()),
                string.Join("", $"{result.ecx:X}".HexStringToString().Reverse()));

            information.Cpus[cpuIndex].Vendor = vendorString;
            information.Cpus[cpuIndex].MaxCpuIdFeatureLevel = result.eax;

            Opcode.Cpuid(out result, 0x80000000, 0);
            information.Cpus[cpuIndex].MaxCpuIdExtendedFeatureLevel = result.eax;
        }

        /// <summary>
        ///     Requires MaxCpuIdFeatureLevel from <see cref="IdentifyVendorAndLevelInformation" />
        ///     EAX = 7, EXC = 0
        ///     EBX, ECX, EDX = Feature Flags
        /// </summary>
        /// <param name="cpuIndex"></param>
        /// <param name="information"></param>
        private void IdentifyExtendedFeatureFlags(int cpuIndex, MachineInformation information)
        {
            if (information.Cpus[cpuIndex].MaxCpuIdFeatureLevel >= 7)
            {
                Opcode.Cpuid(out var result, 7, 0);

                information.Cpus[cpuIndex].ExtendedFeatureFlagsF7One = (CPU.ExtendedFeatureFlagsF7EBX)result.ebx;
                information.Cpus[cpuIndex].ExtendedFeatureFlagsF7Two = (CPU.ExtendedFeatureFlagsF7ECX)result.ecx;
                information.Cpus[cpuIndex].ExtendedFeatureFlagsF7Three = (CPU.ExtendedFeatureFlagsF7EDX)result.edx;
            }
        }

        /// <summary>
        ///     Requires MaxCpuIdExtendedFeatureLevel from <see cref="IdentifyVendorAndLevelInformation" />
        ///     This is a non-standard feature but implemented by both Intel and AMD so ¯\_(ツ)_/¯
        /// </summary>
        /// <param name="cpuIndex"></param>
        /// <param name="information"></param>
        private void IdentifyExtendedName(int cpuIndex, MachineInformation information)
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

                information.Cpus[cpuIndex].Name = sb.ToString();
            }
        }

        /// <summary>
        ///     EAX = 1, ECX = 0
        ///     EAX=[27..20:Extended Family],[19..16:Extended Model],[13..12:Type],[11..8:Family],[7..4:Model],[3..0: Stepping]
        ///     EBX=[31..24: Default APIC ID],[23..16:Logical Processor Count],[15..8:CFLUSH Chunk Count],[7..0:Brand ID]
        ///     ECX=Feature Flags
        ///     EDX=Feature Flags
        /// </summary>
        /// <param name="information"></param>
        private void GatherCpuModelInformation(int cpuIndex, MachineInformation information)
        {
            Opcode.Cpuid(out var result, 1, 0);
            information.Cpus[cpuIndex].Type = (CPU.ProcessorType)Util.ExtractBits(result.eax, 12, 13);
            information.Cpus[cpuIndex].Family = Util.ExtractBits(result.eax, 8, 12);
            information.Cpus[cpuIndex].Model = Util.ExtractBits(result.eax, 4, 7);
            information.Cpus[cpuIndex].Stepping = Util.ExtractBits(result.eax, 0, 3);

            if (information.Cpus[cpuIndex].Family is 15)
            {
                // Family needs to be added together
                information.Cpus[cpuIndex].Family = Util.ExtractBits(result.eax, 20, 27) + information.Cpus[cpuIndex].Family;
                // Model needs to be concatenated
                information.Cpus[cpuIndex].Model = (Util.ExtractBits(result.eax, 16, 19) << 4) + information.Cpus[cpuIndex].Model;
            }

            if (information.Cpus[cpuIndex].Family is 6)
            {
                // Model needs to be concatenated
                information.Cpus[cpuIndex].Model = (Util.ExtractBits(result.eax, 16, 19) << 4) + information.Cpus[cpuIndex].Model;
            }

            information.Cpus[cpuIndex].FeatureFlagsOne = (CPU.FeatureFlagEDX)result.edx;
            information.Cpus[cpuIndex].FeatureFlagsTwo = (CPU.FeatureFlagECX)result.ecx;
        }

        /// <summary>
        ///     This is a standard feature, but only implemented by Intel right now
        /// </summary>
        /// <param name="cpuIndex"></param>
        /// <param name="coreIndex"></param>
        /// <param name="information"></param>
        private void GatherCoreSpeedInformation(int cpuIndex, int coreIndex, MachineInformation information)
        {
            if (information.Cpus[cpuIndex].MaxCpuIdFeatureLevel >= 0x16)
            {
                Opcode.Cpuid(out var result, 0x16, 0);

                information.Cpus[cpuIndex].Cores[coreIndex].ReferenceNormalClockSpeed = result.eax;
                information.Cpus[cpuIndex].Cores[coreIndex].ReferenceMaxClockSpeed = result.ebx;
                information.Cpus[cpuIndex].Cores[coreIndex].ReferenceBusSpeed = result.ecx;
            }
        }
    }
}