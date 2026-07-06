#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using HardwareInformation.Information;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers.MacOs;

/// <summary>
///     Gathers disk information on macOS using diskutil.
/// </summary>
public class OSXDiskInformationProvider : InformationProvider
{
    public override bool Available(MachineInformation information)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }

    public override void GatherInformation(MachineInformation information)
    {
        try
        {
            var disks = ParseDiskutil();
            information.Disks = disks.AsReadOnly();
        }
        catch (Exception e)
        {
            MachineInformationGatherer.Logger.LogError(e, "Failed to gather disk information on macOS");
        }
    }

    private List<Disk> ParseDiskutil()
    {
        var disks = new List<Disk>();

        // diskutil list outputs something like:
        //   /dev/disk0 (internal, physical):
        //    #:                       TYPE NAME                    SIZE       IDENTIFIER
        //    0:      GUID_partition_scheme                        *121.3 GB   disk0
        //    1:             Apple_APFS_ISC                         524.3 MB   disk0s1
        //    ...
        using var p = Util.StartProcess("diskutil", "list");
        using var sr = p.StandardOutput;
        p.WaitForExit();
        var lines = sr.ReadToEnd().Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

        // Match lines that look like:
        //   /dev/disk0 (internal, physical):
        //   /dev/disk1 (external, physical):
        string currentDiskDevice = null;
        string currentDiskSize = null;
        string currentDiskType = null;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Detect disk header line: /dev/disk0 (internal, physical):
            if (trimmed.StartsWith("/dev/disk") && trimmed.Contains("physical"))
            {
                // Save the previous disk if any
                if (currentDiskDevice != null)
                {
                    disks.Add(new Disk
                    {
                        DeviceID = currentDiskDevice,
                        Capacity = ParseSize(currentDiskSize),
                        CapacityHRF = Util.FormatBytes(ParseSize(currentDiskSize)),
                        Model = $"Apple {currentDiskType}",
                        Caption = currentDiskType,
                        Vendor = "Apple",
                        Partitions = 0 // We don't count partitions in the simple model
                    });
                }

                // Parse device name (e.g., "disk0")
                var device = trimmed.Split('(')[0].Trim();
                currentDiskDevice = device;

                // Parse disk type (internal/external/etc.)
                var typeMatch = trimmed.Split('(')[1]?.Split(')')[0]?.Trim();
                currentDiskType = typeMatch ?? "unknown";
                currentDiskSize = null;
            }
            else if (trimmed.Contains("GUID_partition_scheme") && trimmed.EndsWith(currentDiskDevice))
            {
                // The GUID line shows total size: "*121.3 GB   disk0"
                // Extract size by splitting on whitespace and finding the token starting with '*'
                var tokens = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < tokens.Length - 1; i++)
                {
                    if (tokens[i].StartsWith("*"))
                    {
                        currentDiskSize = tokens[i].TrimStart('*') + " " + tokens[i + 1];
                        break;
                    }
                }
            }
        }

        // Don't forget the last disk
        if (currentDiskDevice != null)
        {
            disks.Add(new Disk
            {
                DeviceID = currentDiskDevice,
                Capacity = ParseSize(currentDiskSize),
                CapacityHRF = Util.FormatBytes(ParseSize(currentDiskSize)),
                Model = $"Apple {currentDiskType}",
                Caption = currentDiskType,
                Vendor = "Apple",
                Partitions = 0
            });
        }

        return disks;
    }

    private ulong ParseSize(string sizeStr)
    {
        if (string.IsNullOrEmpty(sizeStr))
            return 0;

        // Parse "121.3 GB" or "121.3 GiB" or "524.3 MB"
        var parts = sizeStr.Trim().Split(' ');
        if (parts.Length != 2)
            return 0;

        if (!double.TryParse(parts[0], out var value))
            return 0;

        var unit = parts[1].ToUpperInvariant();
        switch (unit)
        {
            case "B":
            case "BYTES":
                return (ulong)(value * 1);
            case "KB":
            case "KIB":
                return (ulong)(value * 1024);
            case "MB":
            case "MIB":
                return (ulong)(value * 1024 * 1024);
            case "GB":
            case "GIB":
                return (ulong)(value * 1024 * 1024 * 1024);
            case "TB":
            case "TIB":
                return (ulong)(value * 1024 * 1024 * 1024 * 1024);
            default:
                return 0;
        }
    }
}
