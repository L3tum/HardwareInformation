namespace HardwareInformation.Providers
{
    internal abstract class InformationProvider
    {
	    /// <summary>
	    ///     Check if this provider or the underlying method is available on this platform
	    /// </summary>
	    /// <param name="information"></param>
	    /// <returns></returns>
	    public abstract bool Available(MachineInformation information);

	    /// <summary>
	    ///     Update information that may depend on other providers (OS providers for example)
	    /// </summary>
	    /// <param name="information"></param>
	    public virtual void PostProviderUpdateInformation(ref MachineInformation information)
        {
        }

	    /// <summary>
	    ///     Gathers general system information necessary for executing other information gathering methods
	    /// </summary>
	    /// <param name="information"></param>
	    public virtual void GatherGeneralSystemInformation(ref MachineInformation information)
        {
        }

	    /// <summary>
	    ///     Gathers CPU information
	    /// </summary>
	    /// <param name="information"></param>
	    public virtual void GatherCpuInformation(ref MachineInformation information)
        {
        }

	    /// <summary>
	    ///     Gather CPU cache topology information
	    /// </summary>
	    /// <param name="information"></param>
	    public virtual void GatherCpuCacheTopologyInformation(ref MachineInformation information)
        {
        }

	    /// <summary>
	    ///     Gather CPU feature flag information
	    /// </summary>
	    /// <param name="information"></param>
	    public virtual void GatherCpuFeatureFlagInformation(ref MachineInformation information)
        {
        }

	    /// <summary>
	    ///     Gathers CPU speed (frequency) information
	    /// </summary>
	    /// <param name="information"></param>
	    public virtual void GatherCpuSpeedInformation(ref MachineInformation information)
        {
        }

	    /// <summary>
	    ///     Gather GPU information
	    /// </summary>
	    /// <param name="information"></param>
	    public virtual void GatherGpuInformation(ref MachineInformation information)
        {
        }

	    /// <summary>
	    ///     Gathers mainboard and BIOS information
	    /// </summary>
	    /// <param name="information"></param>
	    public virtual void GatherMainboardInformation(ref MachineInformation information)
        {
        }

	    /// <summary>
	    ///     Gather USB information
	    /// </summary>
	    /// <param name="information"></param>
	    public virtual void GatherUsbInformation(ref MachineInformation information)
        {
        }

	    /// <summary>
	    ///     Gathers information on installed hard drives
	    /// </summary>
	    /// <param name="information"></param>
	    public virtual void GatherDiskInformation(ref MachineInformation information)
        {
        }

	    /// <summary>
	    ///     Gathers information on installed RAM sticks
	    /// </summary>
	    /// <param name="information"></param>
	    public virtual void GatherRamInformation(ref MachineInformation information)
        {
        }

	    /// <summary>
	    ///     Gathers information on connected monitors
	    /// </summary>
	    /// <param name="information"></param>
	    public virtual void GatherMonitorInformation(ref MachineInformation information)
        {
        }
    }
}