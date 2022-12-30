#region using

using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using HardwareInformation.Information;

#endregion

namespace HardwareInformation.Providers.Windows;

public class WindowsCpuInformationProvider : WindowsInformationProvider
{
    [SupportedOSPlatform("windows")]
    public override void GatherInformation(MachineInformation information)
    {
        string query;

        if (information.Windows10)
        {
            query =
                "select Name,Caption,NumberOfEnabledCore,NumberOfLogicalProcessors,SocketDesignation,MaxClockSpeed from Win32_Processor";
        }
        else
        {
            query = "select Name,Caption,NumberOfLogicalProcessors,SocketDesignation,MaxClockSpeed from Win32_Processor";
        }

        using (var mos = new ManagementObjectSearcher(query))
        {
            foreach (var managementBaseObject in mos.Get())
            {
                if (managementBaseObject?.Properties == null || managementBaseObject.Properties.Count == 0)
                {
                    continue;
                }

                var name = managementBaseObject.Properties["Name"].Value.ToString();
                var caption = managementBaseObject.Properties["Caption"].Value.ToString();
                var socket = managementBaseObject.Properties["SocketDesignation"].Value.ToString();
                var normalClockSpeed = uint.Parse(managementBaseObject.Properties["MaxClockSpeed"].Value.ToString() ?? "0");
                var numberOfLogicalCores = int.Parse(managementBaseObject.Properties["NumberOfLogicalProcessors"].Value.ToString() ?? "0");

                // Check if we already figured out the CPUs, if we haven't we can add them here
                if (information.Cpus.Count > 0)
                {
                    foreach (var cpu in information.Cpus)
                    {
                        if (cpu.Name == name)
                        {
                            cpu.Caption = caption;
                            cpu.Socket = socket;

                            if (cpu.NormalClockSpeed == default)
                            {
                                cpu.NormalClockSpeed = normalClockSpeed;
                            }
                        }
                    }
                }
                else
                {
                    var cpu = new CPU
                    {
                        Name = name,
                        Caption = caption,
                        Socket = socket,
                        NormalClockSpeed = normalClockSpeed,
                        LogicalCoresInCpu = Enumerable.Range(0, numberOfLogicalCores).Select(number => (uint)number).ToHashSet()
                    };
                    cpu.InitializeLists();
                    information.Cpus = information.Cpus.ToList().Append(cpu).ToList().AsReadOnly();
                }
            }
        }
    }
}