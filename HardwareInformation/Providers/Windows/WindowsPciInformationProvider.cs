#region using

using System;
using System.Runtime.Versioning;

#endregion

namespace HardwareInformation.Providers.Windows;

public class WindowsPciInformationProvider : WindowsPnpInformationProvider
{
    [SupportedOSPlatform("windows")]
    public override void GatherInformation(MachineInformation information)
    {
        var devices = GetPnpDevices("PCI", information.Windows10);

        foreach (var device in devices)
        {
            (device.VendorID, device.ProductID) = GetVidAndPid(device.DeviceID);
            (device.VendorName, device.ProductName) = PCIVendorList.GetVendorAndProductName(device.VendorID, device.ProductID);
        }

        information.PciDevices = devices.AsReadOnly();
    }

    private static Tuple<string, string> GetVidAndPid(string deviceId)
    {
        var vidPid = deviceId.Split('\\')[1];
        var vid = vidPid.StartsWith("VEN_") ? vidPid.Split('&')[0].Replace("VEN_", "") : null;
        var pid = vidPid.StartsWith("VEN_") ? vidPid.Split('&')[1].Replace("DEV_", "") : null;

        return Tuple.Create(vid, pid);
    }
}