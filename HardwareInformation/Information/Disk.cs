#region using

using Reinforced.Typings.Attributes;

#endregion

namespace HardwareInformation.Information
{
	/// <summary>
	///     Construct to represent a hard drive (HDD/SSD)
	/// </summary>
	[TsClass]
	public class Disk
	{
		/// <summary>
		///     Model string
		/// </summary>
		public string Model { get; set; }

		/// <summary>
		///     Caption
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		///     Capacity of this disk
		/// </summary>
		public ulong Capacity { get; set; }

		/// <summary>
		///     Human readable format of the capacity this disk has
		/// </summary>
		public string CapacityHRF { get; set; }
	}
}