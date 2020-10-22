#region using

#endregion

namespace HardwareInformation.Information
{
	/// <summary>
	///     BIOS and Mainboard information
	/// </summary>
	public class SMBios
    {
	    /// <summary>
	    ///     Version of the BIOS
	    /// </summary>
	    public string BIOSVersion { get; internal set; }

	    /// <summary>
	    ///     Vendor of the BIOS. Should be American Megatrends in most cases.
	    /// </summary>
	    public string BIOSVendor { get; internal set; }

	    /// <summary>
	    ///     Codename of your BIOS, mostly something internal but may be useful if the BIOSVersion isn't set.
	    /// </summary>
	    public string BIOSCodename { get; internal set; }

	    /// <summary>
	    ///     Manufacturer/Vendor of the Mainboard (e.g. ASUSTek)
	    /// </summary>
	    public string BoardVendor { get; internal set; }

	    /// <summary>
	    ///     Name of the Mainboard (e.g. Crosshair VIII Hero)
	    /// </summary>
	    public string BoardName { get; internal set; }

	    /// <summary>
	    ///     Version of the Mainboard. Most often a revision number
	    /// </summary>
	    public string BoardVersion { get; internal set; }
    }
}