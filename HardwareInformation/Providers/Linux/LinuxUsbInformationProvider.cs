#region using

using System;
using System.Collections.Generic;
using HardwareInformation.Information;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers.Linux;

public class LinuxUsbInformationProvider : LinuxInformationProvider
{
    public override void GatherInformation(MachineInformation information)
    {
        #region Example

        /*
~]$ lsusb
Bus 001 Device 001: ID 1d6b:0002 Linux Foundation 2.0 root hub
Bus 002 Device 001: ID 1d6b:0002 Linux Foundation 2.0 root hub
[output truncated]
Bus 001 Device 002: ID 0bda:0151 Realtek Semiconductor Corp. Mass Storage Device (Multicard Reader)
Bus 008 Device 002: ID 03f0:2c24 Hewlett-Packard Logitech M-UAL-96 Mouse
Bus 008 Device 003: ID 04b3:3025 IBM Corp.
         */

        #endregion

        var usbDevices = new List<PnpDevice>();

        try
        {
            using var p = Util.StartProcess("lsusb", "");
            using var sr = p.StandardOutput;
            p.WaitForExit();
            var lines = sr.ReadToEnd().Trim().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (GetFromStringWithRegex(line, @"ID\s+([a-f0-9]{4}):([a-f0-9]{4})", out var match))
                {
                    var device = new PnpDevice { VendorID = match.Groups[1].Value, ProductID = match.Groups[2].Value };
                    (device.VendorName, device.ProductName) = USBVendorList.GetVendorAndProductName(device.VendorID, device.ProductID);
                    usbDevices.Add(device);
                }
            }
        }
        catch (Exception e)
        {
            MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing USB info on Linux");
        }
        finally
        {
            information.UsbDevices = usbDevices.AsReadOnly();
        }
    }
}