namespace HardwareInformation.Providers.Linux;

/// <summary>
///     Reads GPU information from /proc/driver/nvidia/gpus (if Nvidia driver is loaded).
/// </summary>
public class LinuxGpuInformationProvider : LinuxInformationProvider
{
    /// <summary>
    ///     Checks for /proc/driver/nvidia/gpus and reads GPU name, memory, and PCI bus info.
    /// </summary>
    public override void GatherInformation(MachineInformation information)
    {
        // TODO: Currently (i.e. porting the old code to this structure) there is no information provided by Linux that can't be fetched with Vulkan
        base.GatherInformation(information);
    }
}