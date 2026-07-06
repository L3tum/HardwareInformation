#region using

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HardwareInformation.Information;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers.MacOs;

/// <summary>
///     Gathers total RAM capacity on macOS via sysctl hw.memsize.
///     Note: macOS does not expose individual RAM stick details,
///     so this returns a single RAM entry with total capacity.
/// </summary>
public class OSXRamInformationProvider : InformationProvider
{
    /// <summary>
    ///     Returns true if running on macOS.
    /// </summary>
    public override bool Available(MachineInformation information)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }

    /// <summary>
    ///     Queries sysctl hw.memsize and creates a single RAM entry with total capacity.
    /// </summary>
    public override void GatherInformation(MachineInformation information)
    {
        try
        {
            var memSize = GetMemorySizeBytes();
            if (memSize > 0)
            {
                // Create a single RAM entry with total capacity
                var ram = new RAM
                {
                    Capacity = memSize,
                    CapacityHRF = Util.FormatBytes(memSize),
                    Name = "Total System Memory",
                    FormFactor = RAM.FormFactors.UNKNOWN,
                    Manufacturer = "Apple" // Apple does not expose actual memory vendors
                };
                information.RAMSticks = new List<RAM> { ram }.AsReadOnly();
            }
        }
        catch (Exception e)
        {
            MachineInformationGatherer.Logger.LogError(e, "Failed to gather RAM information on macOS");
        }
    }

    private static ulong GetMemorySizeBytes()
    {
        using var p = Util.StartProcess("sysctl", "hw.memsize");
        using var sr = p.StandardOutput;
        p.WaitForExit();
        var output = sr.ReadToEnd().Trim();

        // Output: "hw.memsize: 34359738368"
        var parts = output.Split(':');
        if (parts.Length >= 2)
        {
            if (ulong.TryParse(parts[1].Trim(), out var size))
                return size;
        }
        return 0;
    }
}
