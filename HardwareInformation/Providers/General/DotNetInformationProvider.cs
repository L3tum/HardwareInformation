using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace HardwareInformation.Providers.General;

public class DotNetInformationProvider : InformationProvider
{
    public override bool Available(MachineInformation information)
    {
        return true;
    }

    public override void GatherInformation(MachineInformation information)
    {
        information.OperatingSystem = Environment.OSVersion;
        information.Platform = Expression.Empty() switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => MachineInformation.Platforms.Windows,
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => MachineInformation.Platforms.Linux,
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => MachineInformation.Platforms.OSX,
            _ => MachineInformation.Platforms.Unknown
        };
    }
}