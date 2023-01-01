#region using

using System;
using System.Linq;
using System.Runtime.InteropServices;
using HardwareInformation.Providers.Unix;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers.MacOs;

public class OSXInformationProvider : UnixHelperInformationProvider
{
    public override bool Available(MachineInformation information)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }

    public override void GatherInformation(MachineInformation information)
    {
        try
        {
            using var p = Util.StartProcess("sysctl", "-a");
            using var sr = p.StandardOutput;
            p.WaitForExit();
            var lines = sr.ReadToEnd().Trim().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            string value;

            if (GetValueFromStartingText(lines, @"machdep\.cpu\.vendor", out value))
            {
                information.Cpu.Vendor = value.Trim();
            }

            if (GetValueFromStartingText(lines, @"machdep\.cpu\.brand_string", out value))
            {
                information.Cpu.Caption = value.Trim();
            }

            if (GetValueFromStartingText(lines, @"machdep\.cpu\.family", out value))
            {
                information.Cpu.Family = uint.Parse(value.Trim());
            }

            if (GetValueFromStartingText(lines, @"machdep\.cpu\.model", out value))
            {
                information.Cpu.Model = uint.Parse(value.Trim());
            }

            if (GetValueFromStartingText(lines, @"machdep\.cpu\.stepping", out value))
            {
                information.Cpu.Stepping = uint.Parse(value.Trim());
            }

            if (GetValueFromStartingText(lines, @"hw\.physicalcpu", out value))
            {
                information.Cpu.PhysicalCores = uint.Parse(value.Trim());
            }

            if (GetValueFromStartingText(lines, @"hw\.logicalcpu", out value))
            {
                information.Cpu.LogicalCoresInCpu = Enumerable.Range(0, int.Parse(value.Trim())).Select(number => (uint)number).ToHashSet();
                information.Cpu.InitializeLists();
            }

            // ARM Macs use this instead of machdep.cpu.family :)
            if (GetValueFromStartingText(lines, @"hw\.cpufamily", out value))
            {
                information.Cpu.Family = uint.Parse(value.Trim());
            }
        }
        catch (Exception e)
        {
            MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing information from sysctl on OSX");
        }
    }
}