#region using

using System;
using System.Linq;

#endregion

namespace HardwareInformation.Providers.General;

/// <summary>
///     Reads the CPUID 0x8FFFFFFF leaf (AMD K7/K8 "hammer time" easter egg). Only works on older AMD CPUs.
/// </summary>
public class EasterEggInformationProvider : InformationProvider
{
    /// <summary>
    ///     Always available, though the easter egg itself may not exist on newer CPUs.
    /// </summary>
    public override bool Available(MachineInformation information)
    {
        return true;
    }

    /// <summary>
    ///     Reads the AMD easter egg string from CPUID leaf 0x8FFFFFFF.
    /// </summary>
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