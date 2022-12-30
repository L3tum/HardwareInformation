#region using

using System;
using System.Management;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers.Windows;

public class WindowsSystemInformationProvider : WindowsInformationProvider
{
    [SupportedOSPlatform("windows")]
    public override void GatherInformation(MachineInformation information)
    {
        try
        {
            var osVersionInfoEx = new OSVERSIONINFOEX();
            var success = Win32APIProvider.ntdll_RtlGetVersion(ref osVersionInfoEx);

            if (success == NTSTATUS.STATUS_SUCCESS)
            {
                if (osVersionInfoEx.MajorVersion >= 10)
                {
                    information.Windows10 = true;
                }
            }
        }
        catch (Exception e)
        {
            MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing OS Version info");
        }
        
        using (var mos = new ManagementObjectSearcher("select Product,Manufacturer,Version from Win32_BaseBoard"))
        {
            foreach (var managementBaseObject in mos.Get())
            {
                if (managementBaseObject?.Properties == null || managementBaseObject.Properties.Count == 0)
                {
                    continue;
                }

                information.SmBios.BoardName = managementBaseObject.Properties["Product"].Value.ToString();
                information.SmBios.BoardVendor = managementBaseObject.Properties["Manufacturer"].Value.ToString();
                information.SmBios.BoardVersion = managementBaseObject.Properties["Version"].Value.ToString();
            }
        }

        using (var mos = new ManagementObjectSearcher("select Name,Manufacturer,Version from Win32_BIOS"))
        {
            foreach (var managementBaseObject in mos.Get())
            {
                if (managementBaseObject?.Properties == null || managementBaseObject.Properties.Count == 0)
                {
                    continue;
                }

                information.SmBios.BIOSCodename = managementBaseObject.Properties["Name"].Value.ToString();
                information.SmBios.BIOSVendor = managementBaseObject.Properties["Manufacturer"].Value.ToString();
                information.SmBios.BIOSVersion = managementBaseObject.Properties["Version"].Value.ToString();
            }
        }
    }
}