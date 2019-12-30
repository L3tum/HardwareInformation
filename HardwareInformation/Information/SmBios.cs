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
		public string BIOSVersion { get; set; }

		/// <summary>
		///     Vendor of the BIOS. Should be American Megatrends in most cases.
		/// </summary>
		public string BIOSVendor { get; set; }

		/// <summary>
		///     Codename of your BIOS, mostly something internal but may be useful if the BIOSVersion isn't set.
		/// </summary>
		public string BIOSCodename { get; set; }

		/// <summary>
		///     Manufacturer/Vendor of the Mainboard (e.g. ASUSTek)
		/// </summary>
		public string BoardVendor { get; set; }

		/// <summary>
		///     Name of the Mainboard (e.g. Crosshair VIII Hero)
		/// </summary>
		public string BoardName { get; set; }

		/// <summary>
		///     Version of the Mainboard. Most often a revision number
		/// </summary>
		public string BoardVersion { get; set; }
	}
}