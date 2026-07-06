#region using

using System.Runtime.InteropServices;
using HardwareInformation.Providers.Unix;

#endregion

namespace HardwareInformation.Providers.Linux;

/// <summary>
///     Base class for all Linux providers. Checks if running on Linux.
/// </summary>
public class LinuxInformationProvider : UnixHelperInformationProvider
{
    /// <summary>
    ///     Returns true if the current OS is Linux.
    /// </summary>
    public override bool Available(MachineInformation information)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}