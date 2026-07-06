#region using

using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

#endregion

namespace HardwareInformation.Providers.General;

/// <summary>
///     Provides .NET runtime and platform information (OS, Windows/Linux/macOS).
/// </summary>
public class DotNetInformationProvider : InformationProvider
{
    /// <summary>
    ///     Always available.
    /// </summary>
    public override bool Available(MachineInformation information)
    {
        return true;
    }

    /// <summary>
    ///     Sets OperatingSystem and Platform fields.
    /// </summary>
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