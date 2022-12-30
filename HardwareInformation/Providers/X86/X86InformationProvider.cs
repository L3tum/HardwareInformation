#region using

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        ///     We assume that every processor supports at least Level 1.
        ///     If that is not the case, then tough luck. The CPU is probably so old
        ///     that it can't even run C# on it.
        ///     EAX = 1, ECX = 0
        ///     EBX=[23..16:Logical Processor Count]
        /// </summary>
        /// <returns></returns>
        private uint GetNumberOfLogicalCores()
        {
            Opcode.Cpuid(out var result, 1, 0);
            return Util.ExtractBits(result.ebx, 16, 23);
        }

        /// <summary>
        ///     EAX = 1, ECX = 0
        ///     EAX=[27..20:Extended Family],[19..16:Extended Model],[13..12:Type],[11..8:Family],[7..4:Model],[3..0: Stepping]
        ///     EBX=[31..24: Default APIC ID],[23..16:Logical Processor Count],[15..8:CFLUSH Chunk Count],[7..0:Brand ID]
        ///     ECX=Feature Flags
        ///     EDX=Feature Flags
        /// </summary>
        /// <param name="information"></param>
        protected override void IdentifyCpus(MachineInformation information)
        {
            var threads = GetNumberOfLogicalCores();
            var cpus = new List<CPU>();
            var tasks = Util.RunAffinityOnNumberOfThreads(threads, thread =>
            {
                Opcode.Cpuid(out var result, 1, 0);
                var cpu = new CPU
                {
                    Type = (CPU.ProcessorType)Util.ExtractBits(result.eax, 12, 13),
                    Family = Util.ExtractBits(result.eax, 8, 12),
                    Model = Util.ExtractBits(result.eax, 4, 7),
                    Stepping = Util.ExtractBits(result.eax, 0, 3)
                };

                if (cpu.Family is 15)
                {
                    // Family needs to be added together
                    cpu.Family = Util.ExtractBits(result.eax, 20, 27) + cpu.Family;
                    // Model needs to be concatenated
                    cpu.Model = (Util.ExtractBits(result.eax, 16, 19) << 4) + cpu.Model;
                }

                if (cpu.Family is 6)
                {
                    // Model needs to be concatenated
                    cpu.Model = (Util.ExtractBits(result.eax, 16, 19) << 4) + cpu.Model;
                }

                cpu.FeatureFlagsOne = (CPU.FeatureFlagEDX)result.edx;
                cpu.FeatureFlagsTwo = (CPU.FeatureFlagECX)result.ecx;
                cpu.LogicalCoresInCpu.Add(thread);
                lock (cpus)
                {
                    cpus.Add(cpu);
                }
            });

            Task.WaitAll(tasks);

            // Check if Family, Model, Type and Stepping are different (deduplicate CPUs)
            var finalCpus = new List<CPU>();
            foreach (var cpu in cpus)
            {
                CPU equalCpu = null;
                foreach (var finalCpu in finalCpus)
                {
                    if (
                        finalCpu.Family == cpu.Family
                        && finalCpu.Model == cpu.Model
                        && finalCpu.Type == cpu.Type
                        && finalCpu.Stepping == cpu.Stepping
                        && finalCpu.FeatureFlagsOne == cpu.FeatureFlagsOne
                        && finalCpu.FeatureFlagsTwo == cpu.FeatureFlagsTwo
                    )
                    {
                        equalCpu = finalCpu;
                        break;
                    }
                }

                if (equalCpu == null)
                {
                    finalCpus.Add(cpu);
                }
                else
                {
                    equalCpu.LogicalCoresInCpu.UnionWith(cpu.LogicalCoresInCpu);
                }
            }

            foreach (var finalCpu in finalCpus)
            {
                finalCpu.LogicalCoresInCpu = finalCpu.LogicalCoresInCpu.OrderBy(key => key).ToHashSet();
                finalCpu.InitializeLists();
            }

            information.Cpus = finalCpus.AsReadOnly();
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