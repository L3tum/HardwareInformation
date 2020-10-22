namespace HardwareInformation.Information
{
	/// <summary>
	///     One installed GPU in the system
	/// </summary>
	public class GPU
    {
	    /// <summary>
	    ///     Vendor, or as WMI calls it, AdapterCompatibility
	    /// </summary>
	    public string Vendor { get; internal set; }

	    /// <summary>
	    ///     Description
	    /// </summary>
	    public string Description { get; internal set; }

	    /// <summary>
	    ///     Caption
	    /// </summary>
	    public string Caption { get; internal set; }

	    /// <summary>
	    ///     Name
	    /// </summary>
	    public string Name { get; internal set; }

	    /// <summary>
	    ///     Date of the driver in use
	    /// </summary>
	    public string DriverDate { get; internal set; }

	    /// <summary>
	    ///     Version of the driver in use
	    /// </summary>
	    public string DriverVersion { get; internal set; }


	    /// <summary>
	    ///     Status of the GPU
	    /// </summary>
	    public string Status { get; internal set; }
    }
}