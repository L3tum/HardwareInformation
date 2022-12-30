#region using

using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.Versioning;
using HardwareInformation.Information;

#endregion

namespace HardwareInformation.Providers.Windows;

public class WindowsRamInformationProvider : WindowsInformationProvider
{
    [SupportedOSPlatform("windows")]
    public override void GatherInformation(MachineInformation information)
    {
        string query;
        var ramSticks = new List<RAM>();

        if (information.Windows10)
        {
            query =
                "select ConfiguredClockSpeed,Manufacturer,Capacity,DeviceLocator,PartNumber,FormFactor,ConfiguredVoltage,BankLabel from Win32_PhysicalMemory";
        }
        else
        {
            query = "select Manufacturer,Capacity,DeviceLocator,PartNumber,FormFactor from Win32_PhysicalMemory";
        }

        using (var mos = new ManagementObjectSearcher(query))
        {
            // There is currently no other way to gather RAM information so we don't need to check if it's already set
            foreach (var managementBaseObject in mos.Get())
            {
                if (managementBaseObject?.Properties == null || managementBaseObject.Properties.Count == 0)
                {
                    continue;
                }

                var ram = new RAM
                {
                    Speed = information.Windows10 ? uint.Parse(managementBaseObject.Properties["ConfiguredClockSpeed"].Value.ToString() ?? "0") : 0,
                    Manufacturer = managementBaseObject.Properties["Manufacturer"].Value.ToString(),
                    Name = managementBaseObject.Properties["DeviceLocator"].Value.ToString(),
                    PartNumber = managementBaseObject.Properties["PartNumber"].Value.ToString(),
                    FormFactor = (RAM.FormFactors)Enum.Parse(typeof(RAM.FormFactors), managementBaseObject.Properties["FormFactor"].Value.ToString() ?? "0"),
                    Capacity = ulong.Parse(managementBaseObject.Properties["Capacity"].Value.ToString() ?? "0"),
                    NominalVoltage = uint.Parse(managementBaseObject.Properties["ConfiguredVoltage"].Value.ToString() ?? "0"),
                    BankLabel = managementBaseObject.Properties["BankLabel"].Value.ToString(),
                };

                ram.CapacityHRF = Util.FormatBytes(ram.Capacity);

                ramSticks.Add(ram);
            }
        }

        information.RAMSticks = ramSticks.AsReadOnly();
    }
}