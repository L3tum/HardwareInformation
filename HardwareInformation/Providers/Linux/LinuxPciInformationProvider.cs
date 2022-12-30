#region using

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HardwareInformation.Information;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers.Linux;

public class LinuxPciInformationProvider : LinuxInformationProvider
{
    public override void GatherInformation(MachineInformation information)
    {
        #region Example

        /*
# lspci -n
01:00.1 0200: 14e4:1639 (rev 20)
02:00.0 0200: 14e4:1639 (rev 20)
02:00.1 0200: 14e4:1639 (rev 20)
03:00.0 0104: 1000:0079 (rev 05)
06:03.0 0300: 102b:0532 (rev 0a)
         */

        #endregion

        var pnpDevices = new List<PnpDevice>();

        try
        {
            using var p = Util.StartProcess("lspci", "-n");
            using var sr = p.StandardOutput;
            p.WaitForExit();

            var lines = sr.ReadToEnd().Trim().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var regex = new Regex(@"([a-f0-9]{4}):([a-f0-9]{4})");

            foreach (var line in lines)
            {
                var match = regex.Match(line);

                if (match.Success)
                {
                    var device = new PnpDevice
                    {
                        VendorID = match.Groups[1].Value,
                        DeviceID = match.Groups[2].Value
                    };
                    (device.VendorName, device.ProductName) = PCIVendorList.GetVendorAndProductName(device.VendorID, device.ProductID);
                    pnpDevices.Add(device);
                }
            }
        }
        catch (Exception e)
        {
            MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing PCI info on Linux");
        }
        finally
        {
            information.PciDevices = pnpDevices.AsReadOnly();
        }
    }
}