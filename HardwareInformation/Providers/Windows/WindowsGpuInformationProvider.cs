#region using

using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using HardwareInformation.Information;

#endregion

namespace HardwareInformation.Providers.Windows;

public class WindowsGpuInformationProvider : WindowsInformationProvider
{
    [SupportedOSPlatform("windows")]
    public override void GatherInformation(MachineInformation information)
    {
        var gpus = new List<GPU>();

        using (var mos = new ManagementObjectSearcher(
                   "select AdapterCompatibility,Caption,Description,DriverDate,DriverVersion,Name,Status,PNPDeviceID from Win32_VideoController"))
        {
            foreach (var managementBaseObject in mos.Get())
            {
                var gpu = new GPU
                {
                    Vendor = managementBaseObject.Properties["AdapterCompatibility"].Value.ToString(),
                    Caption = managementBaseObject.Properties["Caption"].Value.ToString(),
                    DriverDate = managementBaseObject.Properties["DriverDate"].Value.ToString(),
                    DriverVersion = managementBaseObject.Properties["DriverVersion"].Value.ToString(),
                    Description = managementBaseObject.Properties["Description"].Value.ToString(),
                    Name = managementBaseObject.Properties["Name"].Value.ToString(),
                    Status = managementBaseObject.Properties["Status"].Value.ToString(),
                    VendorID = managementBaseObject.Properties["PNPDeviceID"].Value.ToString()?.Split("VEN_")[1].Split("&")[0],
                    DeviceID = managementBaseObject.Properties["PNPDeviceID"].Value.ToString()?.Split("DEV_")[1].Split("&")[0],
                };

                gpus.Add(gpu);
            }
        }

        if (information.Gpus.Count == 0)
        {
            information.Gpus = gpus.AsReadOnly();
        }
        else
        {
            foreach (var gpu in information.Gpus)
            {
                var wmiGpu = gpus.FirstOrDefault(g => g.Name == gpu.Name);

                if (wmiGpu is not null)
                {
                    // Currently only DriverDate, Status and Description cannot be queried via Vulkan
                    gpu.DriverDate = wmiGpu.DriverDate;
                    gpu.Status = wmiGpu.Status;
                    gpu.Description = wmiGpu.Description;
                }
            }
        }
    }
}