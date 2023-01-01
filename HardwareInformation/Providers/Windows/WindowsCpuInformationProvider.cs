#region using

using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using HardwareInformation.Information;

#endregion

namespace HardwareInformation.Providers.Windows;

public class WindowsCpuInformationProvider : WindowsInformationProvider
{
    [SupportedOSPlatform("windows")]
    protected override void IdentifyCpus(MachineInformation information)
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
            var index = -1;
            foreach (var managementBaseObject in mos.Get())
            {
                index++;
                if (managementBaseObject?.Properties == null || managementBaseObject.Properties.Count == 0)
                {
                    continue;
                }

                var name = (managementBaseObject.Properties["Name"].Value?.ToString() ?? "").Trim();
                var caption = managementBaseObject.Properties["Caption"].Value?.ToString() ?? "";
                var socket = managementBaseObject.Properties["SocketDesignation"].Value?.ToString() ?? "";
                var normalClockSpeed = uint.Parse(managementBaseObject.Properties["MaxClockSpeed"].Value.ToString() ?? "0");
                var numberOfLogicalCores = int.Parse(managementBaseObject.Properties["NumberOfLogicalProcessors"].Value.ToString() ?? "0");

                if (information.Cpus.Count > index)
                {
                    if (information.Cpus[index].Name != name)
                    {
                        // Something doesn't match with the counting of the processors, better break instead of doing a complicated match
                        continue;
                    }
                }
                else
                {
                    if (information.Cpus.Count <= index - 1)
                    {
                        // We are missing a CPU somehow, break
                        continue;
                    }

                    var startOfLogicalCoreNumbering = (int)information.Cpus.Sum(cpu => cpu.LogicalCores);

                    var cpu = new CPU
                    {
                        Name = name,
                        LogicalCoresInCpu = Enumerable.Range(startOfLogicalCoreNumbering, numberOfLogicalCores).Select(number => (uint)number).ToHashSet()
                    };
                    cpu.InitializeLists();
                    information.Cpus = information.Cpus.ToList().Append(cpu).ToList().AsReadOnly();
                }

                if (!string.IsNullOrWhiteSpace(caption))
                {
                    information.Cpus[index].Caption = caption;
                }

                if (!string.IsNullOrWhiteSpace(socket))
                {
                    information.Cpus[index].Socket = socket;
                }

                if (normalClockSpeed != 0)
                {
                    information.Cpus[index].NormalClockSpeed = normalClockSpeed;
                }
            }
        }
    }
}