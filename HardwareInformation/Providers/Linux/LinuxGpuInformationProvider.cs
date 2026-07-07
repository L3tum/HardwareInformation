namespace HardwareInformation.Providers.Linux;

/// <summary>
///     TODO: Currently (i.e. porting the old code to this structure) there is no information provided by Linux that can't be fetched with Vulkan
/// </summary>
public class LinuxGpuInformationProvider : LinuxInformationProvider
{
    /// <summary>
    ///     TODO: Currently (i.e. porting the old code to this structure) there is no information provided by Linux that can't be fetched with Vulkan
    /// </summary>
    public override void GatherInformation(MachineInformation information)
    {
        // TODO: Currently (i.e. porting the old code to this structure) there is no information provided by Linux that can't be fetched with Vulkan
        base.GatherInformation(information);
    }
}
