namespace HardwareInformation.Information
{
	/// <summary>
	///     Monitor in the system
	/// </summary>
	public class Display
    {
	    /// <summary>
	    ///     Manufacturer of the monitor
	    /// </summary>
	    public string Manufacturer { get; internal set; }

	    /// <summary>
	    ///     Name of the monitor
	    /// </summary>
	    public string Name { get; internal set; }

	    /// <summary>
	    ///     Device ID of this monitor
	    /// </summary>
	    public string DeviceID { get; internal set; }

	    /// <summary>
	    ///     Vendor ID of the manufacturer of this monitor
	    /// </summary>
	    public string VendorID { get; internal set; }

	    /// <summary>
	    ///     Which year the monitor was built
	    /// </summary>
	    public string YearManufactured { get; internal set; }
    }
}