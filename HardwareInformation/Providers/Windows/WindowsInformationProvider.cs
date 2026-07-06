#region using

using System.Runtime.InteropServices;

#endregion

namespace HardwareInformation.Providers.Windows;

/// <summary>
///     Base class for all Windows providers.
/// </summary>
public abstract class WindowsInformationProvider : InformationProvider
{
    /// <summary>
    ///     Returns true if running on Windows.
    /// </summary>
    public override bool Available(MachineInformation information)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}