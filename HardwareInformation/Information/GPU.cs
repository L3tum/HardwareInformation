namespace HardwareInformation.Information
{
	public class GPU
	{
		/// <summary>
		///     Vendor, or as WMI calls it, AdapterCompatibility
		/// </summary>
		public string Vendor { get; set; }

		/// <summary>
		///     Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		///     Caption
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		///     Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///     Date of the driver in use
		/// </summary>
		public string DriverDate { get; set; }

		/// <summary>
		///     Version of the driver in use
		/// </summary>
		public string DriverVersion { get; set; }


		/// <summary>
		///     Status of the GPU
		/// </summary>
		public string Status { get; set; }
	}
}