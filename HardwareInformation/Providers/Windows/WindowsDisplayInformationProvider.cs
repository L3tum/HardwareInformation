#region using

using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using HardwareInformation.Information;

#endregion

namespace HardwareInformation.Providers.Windows;

public class WindowsDisplayInformationProvider : WindowsInformationProvider
{
    [SupportedOSPlatform("windows")]
    public override void GatherInformation(MachineInformation information)
    {
        using var mos = new ManagementObjectSearcher("root\\wmi",
            "select ManufacturerName,UserFriendlyName,InstanceName,YearOfManufacture from WmiMonitorID");
        var displays = new List<Display>();

        foreach (var managementBaseObject in mos.Get())
        {
            try
            {
                var display = new Display
                {
                    Manufacturer = string.Join("",
                        ((IEnumerable<ushort>)managementBaseObject.Properties["ManufacturerName"].Value)
                        .Select(u => char.ConvertFromUtf32(u)).Where(s => s != "\u0000").ToList()),
                    Name = string.Join("",
                        ((IEnumerable<ushort>)managementBaseObject.Properties["UserFriendlyName"].Value)
                        .Select(u => char.ConvertFromUtf32(u)).Where(s => s != "\u0000").ToList()),
                    VendorID = managementBaseObject.Properties["InstanceName"].Value.ToString()?.Split("\\")[1].Substring(0, 3),
                    DeviceID = managementBaseObject.Properties["InstanceName"].Value.ToString()?.Split("\\")[1].Substring(3, 4),
                    YearManufactured = managementBaseObject.Properties["YearOfManufacture"].Value.ToString()
                };

                displays.Add(display);
            }
            catch
            {
                // Intentionally left blank
            }
        }

        information.Displays = displays.AsReadOnly();
    }
}