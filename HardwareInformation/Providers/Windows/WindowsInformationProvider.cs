#region using

using System.Runtime.InteropServices;

#endregion

namespace HardwareInformation.Providers.Windows;

public abstract class WindowsInformationProvider : InformationProvider
{
    public override bool Available(MachineInformation information)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}