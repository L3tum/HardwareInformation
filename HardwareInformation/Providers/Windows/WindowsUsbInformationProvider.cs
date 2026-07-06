#region using

using System;
using System.Runtime.Versioning;

#endregion

namespace HardwareInformation.Providers.Windows;

/// <summary>
///     Derives from PnP provider and filters for USB devices, extracting vendor/product IDs.
/// </summary>
public class WindowsUsbInformationProvider : WindowsPnpInformationProvider
{
    /// <summary>
    ///     Filters USB devices, parses VID/PID, and adds vendor/product names.
    /// </summary>
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