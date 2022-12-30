#region using

using System;
using System.Runtime.Versioning;

#endregion

namespace HardwareInformation.Providers.Windows;

public class WindowsUsbInformationProvider : WindowsPnpInformationProvider
{
    [SupportedOSPlatform("windows")]
    public override void GatherInformation(MachineInformation information)
    {
        var devices = GetPnpDevices("USB", information.Windows10);

        foreach (var device in devices)
        {
            (device.VendorID, device.ProductID) = GetVidAndPid(device.DeviceID);
            (device.VendorName, device.ProductName) = USBVendorList.GetVendorAndProductName(device.VendorID, device.ProductID);
        }

        information.UsbDevices = devices.AsReadOnly();
    }

    private static Tuple<string, string> GetVidAndPid(string deviceId)
    {
        var vidPid = deviceId.Split('\\')[1];
        var vid = vidPid.StartsWith("VID_") ? vidPid.Split('&')[0].Replace("VID_", "") : null;
        var pid = vidPid.StartsWith("VID_") ? vidPid.Split('&')[1].Replace("PID_", "") : null;

        return Tuple.Create(vid, pid);
    }
}