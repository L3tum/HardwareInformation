namespace HardwareInformation.Providers.Linux;

public class LinuxGpuInformationProvider : LinuxInformationProvider
{
    public override void GatherInformation(MachineInformation information)
    {
        // TODO: Currently (i.e. porting the old code to this structure) there is no information provided by Linux that can't be fetched with Vulkan
        base.GatherInformation(information);
    }
}