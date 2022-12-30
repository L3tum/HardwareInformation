#region using

using System.Runtime.Versioning;

#endregion

namespace HardwareInformation.Providers.Linux;

public class LinuxSystemInformationProvider : LinuxInformationProvider
{
    [SupportedOSPlatform("linux")]
    public override void GatherInformation(MachineInformation information)
    {
        string data;

        if (ReadFile("/sys/class/dmi/id/bios_version", out data))
        {
            information.SmBios.BIOSVersion = data.Trim();
        }

        if (ReadFile("/sys/class/dmi/id/bios_vendor", out data))
        {
            information.SmBios.BIOSVendor = data.Trim();
        }

        if (ReadFile("/sys/class/dmi/id/board_name", out data))
        {
            information.SmBios.BoardName = data.Trim();
        }

        if (ReadFile("/sys/class/dmi/id/board_vendor", out data))
        {
            information.SmBios.BoardVendor = data.Trim();
        }
    }
}