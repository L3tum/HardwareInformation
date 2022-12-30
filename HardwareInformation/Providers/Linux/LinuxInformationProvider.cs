#region using

using System.Runtime.InteropServices;
using HardwareInformation.Providers.Unix;

#endregion

namespace HardwareInformation.Providers.Linux;

public class LinuxInformationProvider : UnixHelperInformationProvider
{
    public override bool Available(MachineInformation information)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}