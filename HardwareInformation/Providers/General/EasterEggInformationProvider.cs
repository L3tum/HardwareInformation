#region using

using System;
using System.Linq;

#endregion

namespace HardwareInformation.Providers.General;

public class EasterEggInformationProvider : InformationProvider
{
    public override bool Available(MachineInformation information)
    {
        return true;
    }

    protected override void GatherPerCpuInformation(int cpuIndex, MachineInformation information)
    {
        if (Opcode.IsOpen)
        {
            if (information.Cpus[cpuIndex].MaxCpuIdExtendedFeatureLevel >= 0xFFFFFFF)
            {
                try
                {
                    Opcode.Cpuid(out var hammerTime, 0x8FFFFFFF, 0);

                    var hammerString = string.Format("{0}{1}{2}{3}",
                        string.Join("", $"{hammerTime.eax:X}".HexStringToString().Reverse()),
                        string.Join("", $"{hammerTime.ebx:X}".HexStringToString().Reverse()),
                        string.Join("", $"{hammerTime.ecx:X}".HexStringToString().Reverse()),
                        string.Join("", $"{hammerTime.edx:X}".HexStringToString().Reverse()));

                    if (!string.IsNullOrWhiteSpace(hammerString))
                    {
                        information.Cpus[cpuIndex].EasterEgg = hammerString;
                    }
                }
                catch (Exception)
                {
                    // No K7 or K8 :(
                }
            }
        }
    }
}