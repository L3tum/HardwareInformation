namespace HardwareInformation.Information
{
	/// <summary>
	///     Construct to represent a hard drive (HDD/SSD)
	/// </summary>
	public class Disk
	{
		/// <summary>
		///     Manufacturer of the hard drive
		/// </summary>
		public string Vendor { get; internal set; }

		/// <summary>
		///     Model string
		/// </summary>
		public string Model { get; internal set; }

		/// <summary>
		///     Caption
		/// </summary>
		public string Caption { get; internal set; }

		/// <summary>
		///     Capacity of this disk
		/// </summary>
		public ulong Capacity { get; internal set; }

		/// <summary>
		///     Human readable format of the capacity this disk has
		/// </summary>
		public string CapacityHRF { get; internal set; }
	}
}