#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;
using HardwareInformation.Information;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers.Windows;

public abstract class WindowsPnpInformationProvider : WindowsInformationProvider
{
    [SupportedOSPlatform("windows")]
    protected List<PnpDevice> GetPnpDevices(string type, bool windows10)
    {
        using var mos = new ManagementObjectSearcher($"select DeviceID from Win32_PnPEntity where DeviceId LIKE '{type}%'");
        var alreadySeenDeviceIds = new List<string>();
        var pnpDevices = new Dictionary<string, PnpDevice>();
        var mbos = new ArrayList(mos.Get());

        for (var i = 0; i < mbos.Count; i++)
        {
            if (mbos[i] is not ManagementBaseObject managementBaseObject)
            {
                continue;
            }

            if (managementBaseObject.Properties["DeviceID"].Value is not string deviceId)
            {
                continue;
            }

            if (alreadySeenDeviceIds.Contains(deviceId))
            {
                continue;
            }

            alreadySeenDeviceIds.Add(deviceId);

            // Win32_PnpDeviceProperty is only available with Windows 10
            if (!windows10)
            {
                continue;
            }

            var mo = (ManagementObject)managementBaseObject;
            var inParams = mo.GetMethodParameters("GetDeviceProperties");
            var result = mo.InvokeMethod(
                "GetDeviceProperties",
                inParams,
                new InvokeMethodOptions()
            );

            if (result?.Properties["deviceProperties"].Value is null)
            {
                continue;
            }

            string deviceDescription, driverDescription, driverVersion, driverDate, deviceClass, driverProvider, name, manufacturer;
            deviceDescription = driverDescription = driverVersion = driverDate = deviceClass = driverProvider = name = manufacturer = null;

            foreach (var deviceProperties in (ManagementBaseObject[])result.Properties["deviceProperties"].Value)
            {
                var keyName = deviceProperties.Properties["KeyName"].Value as string;
                var value = deviceProperties.Properties["Data"].Value as string;

                if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(keyName))
                {
                    MachineInformationGatherer.Logger.LogTrace(
                        "KeyName {KeyName} or Value {Value} was null or whitespace for device ID {DeviceId}",
                        keyName, value, deviceId);
                    continue;
                }

                switch (keyName)
                {
                    case "DEVPKEY_Device_BusReportedDeviceDesc":
                    {
                        deviceDescription = value;
                        break;
                    }
                    case "DEVPKEY_Device_DriverDesc":
                    {
                        driverDescription = value;
                        break;
                    }
                    case "DEVPKEY_Device_DriverVersion":
                    {
                        driverVersion = value;
                        break;
                    }
                    case "DEVPKEY_Device_DriverDate":
                    {
                        var year = int.Parse(value.Substring(0, 4));
                        var month = int.Parse(value.Substring(4, 2));
                        var day = int.Parse(value.Substring(6, 2));
                        var hour = int.Parse(value.Substring(8, 2));
                        var minute = int.Parse(value.Substring(10, 2));
                        var second = int.Parse(value.Substring(12, 2));

                        driverDate = new DateTime(year, month, day, hour, minute, second).ToString();
                        break;
                    }
                    case "DEVPKEY_Device_Class":
                    {
                        deviceClass = value;
                        break;
                    }
                    case "DEVPKEY_Device_DriverProvider":
                    {
                        driverProvider = value;
                        break;
                    }
                    case "DEVPKEY_NAME":
                    {
                        name = value;
                        break;
                    }
                    case "DEVPKEY_Device_Manufacturer":
                    {
                        manufacturer = value;
                        break;
                    }
                    case "DEVPKEY_Device_Children":
                    {
                        var children = deviceProperties.Properties["DEVPKEY_Device_Children"];
                        if (children.Value is not null)
                        {
                            if (children.IsArray)
                            {
                                var searcher = new ManagementObjectSearcher();
                                foreach (var child in children.Value as string[])
                                {
                                    searcher.Query = new ObjectQuery(
                                        $"select * from Win32_PnPEntity where DeviceID = {child}");
                                    var childs = searcher.Get();

                                    foreach (var child1 in childs)
                                    {
                                        mbos.Add(child1);
                                    }
                                }
                            }
                        }

                        break;
                    }
                }
            }

            if (deviceDescription is null || driverDescription is null)
            {
                continue;
            }

            if (!pnpDevices.ContainsKey(deviceId))
            {
                pnpDevices.Add(deviceId,
                    new PnpDevice
                    {
                        Name = name,
                        BusReportedName = deviceDescription,
                        DriverName = driverDescription,
                        DriverVersion = driverVersion,
                        DriverDate = DateTime.Parse(driverDate),
                        Class = deviceClass,
                        DriverProvider = driverProvider,
                        Manufacturer = manufacturer,
                        DeviceID = deviceId
                    }
                );
            }
            else
            {
                var device = pnpDevices[deviceId];

                // Prefer composite over solely input
                if (device.DriverName == "USB Input Device" && driverDescription == "USB Composite Device")
                {
                    device.DriverName = deviceDescription;
                }
                // Prefer composite over solely output
                else if (device.DriverName == "USB Output Device" &&
                         driverDescription == "USB Composite Device")
                {
                    device.DriverName = deviceDescription;
                }
                // Prefer composite over solely audio
                else if (device.DriverName == "USB Audio Device" &&
                         driverDescription == "USB Composite Device")
                {
                    device.DriverName = deviceDescription;
                }

                // Prefer different DriverProvider over Microsoft (standard)
                if (device.DriverProvider == "Microsoft" && driverProvider != "Microsoft")
                {
                    device.DriverProvider = driverProvider;
                }

                // Prefer different (or any) Manufacturer over Microsoft
                if (device.Manufacturer is null or "Microsoft" &&
                    manufacturer is not null and not "Microsoft")
                {
                    device.Manufacturer = manufacturer;
                }

                // Prefer custom description over windows-default
                if (device.DriverName != driverDescription && driverDescription is not "USB Input Device" and not "USB Composite Device" and not
                                                               "USB Output Device" and not "USB Audio Device" and not "USB Mass Storage Device"
                                                               and not "Disk Drive" and not "USB Attached SCSI (UAS) Mass Storage Device"
                                                           && !driverDescription.StartsWith("Generic"))
                {
                    device.DriverName = driverDescription;
                }
            }
        }

        return new List<PnpDevice>(pnpDevices.Values);
    }
}