using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HardwareInformation.Information;
using HardwareInformation.Information.Cpu;

namespace HardwareInformation.Providers
{
    internal class CommonInformationProvider : InformationProvider
    {
        public override bool Available(MachineInformation information)
        {
            return Opcode.IsOpen;
        }

        public override void GatherGeneralSystemInformation(ref MachineInformation information)
        {
            information.OperatingSystem = Environment.OSVersion;
            information.Platform = Expression.Empty() switch
            {
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => MachineInformation.Platforms.Windows,
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => MachineInformation.Platforms.Linux,
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => MachineInformation.Platforms.OSX,
                _ => MachineInformation.Platforms.Unknown
            };
            information.Cpu.LogicalCores = (uint) Environment.ProcessorCount;
            information.Cpu.LogicalCoresPerNode = information.Cpu.LogicalCores;
            information.Cpu.Nodes = 1;
            information.Cpu.Architecture = RuntimeInformation.ProcessArchitecture.ToString();
            information.Cpu.Caption = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? default;
            information.Cpu.Name = information.Cpu.Caption;

            var cores = new List<Core>();
            for (var i = 0; i < information.Cpu.LogicalCores; i++)
            {
                cores.Add(new Core
                {
                    Number = (uint) i
                });
            }

            information.Cpu.Cores = cores.AsReadOnly();

            Opcode.Cpuid(out var result, 0, 0);

            var vendorString = string.Format("{0}{1}{2}",
                string.Join("", $"{result.ebx:X}".HexStringToString().Reverse()),
                string.Join("", $"{result.edx:X}".HexStringToString().Reverse()),
                string.Join("", $"{result.ecx:X}".HexStringToString().Reverse()));

            information.Cpu.Vendor = vendorString;
            information.Cpu.MaxCpuIdFeatureLevel = result.eax;

            Opcode.Cpuid(out result, 0x80000000, 0);
            information.Cpu.MaxCpuIdExtendedFeatureLevel = result.eax;
        }

        public override void GatherCpuInformation(ref MachineInformation information)
        {
            if (information.Cpu.MaxCpuIdFeatureLevel >= 1)
            {
                Opcode.Cpuid(out var result, 1, 0);

                information.Cpu.Stepping = result.eax & 0xF;

                var familyId = (result.eax & 0xF00) >> 8;

                if (familyId == 6 || familyId == 15)
                {
                    information.Cpu.Model = (((result.eax & 0xF0000) >> 16) << 4) + ((result.eax & 0xF0) >> 4);
                }
                else
                {
                    information.Cpu.Model = (result.eax & 0xF0) >> 4;
                }

                if (familyId == 15)
                {
                    information.Cpu.Family = ((result.eax & 0xFF00000) >> 20) + familyId;
                }
                else
                {
                    information.Cpu.Family = familyId;
                }

                information.Cpu.Type =
                    (CPU.ProcessorType) ((result.eax & 0b11000000000000) >> 12);
            }
        }

        public override void GatherCpuFeatureFlagInformation(ref MachineInformation information)
        {
            Opcode.Result result;

            if (information.Cpu.MaxCpuIdFeatureLevel >= 1)
            {
                Opcode.Cpuid(out result, 1, 0);
                information.Cpu.FeatureFlagsOne = (CPU.FeatureFlagEDX) result.edx;
                information.Cpu.FeatureFlagsTwo = (CPU.FeatureFlagECX) result.ecx;
            }

            if (information.Cpu.MaxCpuIdFeatureLevel >= 7)
            {
                Opcode.Cpuid(out result, 7, 0);

                information.Cpu.ExtendedFeatureFlagsF7One =
                    (CPU.ExtendedFeatureFlagsF7EBX) result.ebx;
                information.Cpu.ExtendedFeatureFlagsF7Two =
                    (CPU.ExtendedFeatureFlagsF7ECX) result.ecx;
                information.Cpu.ExtendedFeatureFlagsF7Three =
                    (CPU.ExtendedFeatureFlagsF7EDX) result.edx;
            }
        }

        public override void GatherCpuSpeedInformation(ref MachineInformation information)
        {
            var threads = new List<Task>();

            for (var i = 0; i < information.Cpu.LogicalCores; i++)
            {
                var core = information.Cpu.Cores.First(c => c.Number == i);
                var maxCpuIdFeatureLevel = information.Cpu.MaxCpuIdFeatureLevel;

                var thread = Util.RunAffinity(1uL << i, () =>
                {
                    if (maxCpuIdFeatureLevel >= 16)
                    {
                        Opcode.Cpuid(out var result, 0x16, 0);

                        core.ReferenceNormalClockSpeed = result.eax;
                        core.ReferenceMaxClockSpeed = result.ebx;
                        core.ReferenceBusSpeed = result.ecx;
                    }
                });

                threads.Add(thread);
            }

            Task.WaitAll(threads.ToArray());
        }
    }
}