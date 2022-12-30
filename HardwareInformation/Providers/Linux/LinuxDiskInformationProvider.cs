#region using

using System;
using System.Collections.Generic;
using HardwareInformation.Information;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers.Linux;

public class LinuxDiskInformationProvider : LinuxInformationProvider
{
    public override void GatherInformation(MachineInformation information)
    {
        #region Example

        /*
$ lsblk -io KNAME,TYPE,SIZE,MODEL
KNAME TYPE   SIZE MODEL
sda   disk 149.1G TOSHIBA MK1637GS
sr0   rom   1024M CD/DVDW TS-L632M

# smartctl -i /dev/sda
smartctl 6.1 2013-03-16 r3800 [i686-linux-3.9.9-301.fc19.i686.PAE] (local build)
Copyright (C) 2002-13, Bruce Allen, Christian Franke, www.smartmontools.org

=== START OF INFORMATION SECTION ===
Model Family:     Toshiba 2.5" HDD MK..56GSY
Device Model:     TOSHIBA MK1656GSY
Serial Number:    60PKT43CT
LU WWN Device Id: 5 000039 2919874b6
Firmware Version: LH013D
User Capacity:    160 041 885 696 bytes [160 GB]
Sector Size:      512 bytes logical/physical
Device is:        In smartctl database [for details use: -P show]
ATA Version is:   ATA8-ACS (minor revision not indicated)
SATA Version is:  SATA 2.6, 3.0 Gb/s
Local Time is:    Mon Jul 22 11:13:37 2013 CEST
SMART support is: Available - device has SMART capability.
SMART support is: Enabled

         */

        #endregion

        var disks = new List<Disk>();

        try
        {
            using var p = Util.StartProcess("lsblk", "-ido KNAME,TYPE,SIZE,MODEL");
            using var sr = p.StandardOutput;
            p.WaitForExit();
            var lines = sr.ReadToEnd().Trim().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            // Skip first line
            for (var index = 1; index < lines.Length; index++)
            {
                var line = lines[index];
                var parts = line.Split(new[] { " ", "\t" }, 4, StringSplitOptions.None);
                var type = parts[1];

                if (type != "disk")
                {
                    // Only check disks
                    continue;
                }

                var caption = parts[3];
                var disk = new Disk { Caption = caption };

                try
                {
                    var size = parts[2];
                    if (GetFromStringWithRegex(size, @"([0-9.]+)(K|M|G|T|P)", out var match))
                    {
                        var sizeNumerical = double.Parse(match.Groups[1].Value);
                        var multiplier = match.Groups[2].Value switch
                        {
                            "K" => 1024uL,
                            "M" => 1024uL * 1024uL,
                            "G" => 1024uL * 1024uL * 1024uL,
                            "T" => 1024uL * 1024uL * 1024uL * 1024uL,
                            "P" => 1024uL * 1024uL * 1024uL * 1024uL * 1024uL,
                            _ => 0uL
                        };
                        disk.Capacity = (ulong)(sizeNumerical * multiplier);
                        disk.CapacityHRF = Util.FormatBytes(disk.Capacity);
                    }
                }
                catch (Exception e)
                {
                    MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing disk size info on Linux");
                }

                // Try to get actual model name
                try
                {
                    var deviceNameInLinux = parts[0];
                    using var p1 = Util.StartProcess("smartctl", $"-i {deviceNameInLinux}");
                    using var sr1 = p.StandardOutput;
                    p1.WaitForExit();
                    var smartLines = sr1.ReadToEnd().Trim().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                    if (GetValueFromStartingText(smartLines, "Model Family", out var value))
                    {
                        disk.Model = value;
                    }
                }
                catch (Exception e)
                {
                    MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing Smartctl info on Linux");
                }

                disks.Add(disk);
            }
        }
        catch (Exception e)
        {
            MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing disk info on Linux");
        }
        finally
        {
            information.Disks = disks.AsReadOnly();
        }
    }
}