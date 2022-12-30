#region using

using System.Collections.Generic;
using System.Management;
using System.Runtime.Versioning;
using HardwareInformation.Information;

#endregion

namespace HardwareInformation.Providers.Windows;

public class WindowsDiskInformationProvider : WindowsInformationProvider
{
    [SupportedOSPlatform("windows")]
    public override void GatherInformation(MachineInformation information)
    {
        var disks = new List<Disk>();

        using (var mos =
               new ManagementObjectSearcher("select Model,Size,Caption,Partitions,DeviceID from Win32_DiskDrive"))
        {
            foreach (var managementBaseObject in mos.Get())
            {
                var disk = new Disk
                {
                    Model = managementBaseObject.Properties["Model"].Value.ToString(),
                    Capacity = ulong.Parse(managementBaseObject.Properties["Size"].Value.ToString() ?? "0"),
                    Caption = managementBaseObject.Properties["Caption"].Value.ToString(),
                    Partitions = uint.Parse(managementBaseObject.Properties["Partitions"].Value.ToString() ?? "0"),
                    DeviceID = managementBaseObject.Properties["DeviceID"].Value.ToString()
                };
                disk.CapacityHRF = Util.FormatBytes(disk.Capacity);

                disks.Add(disk);
            }
        }

        information.Disks = disks.AsReadOnly();
    }
}