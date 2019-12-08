namespace HardwareInformation.Information.Cpu
{
	/// <summary>
	///     Construct to represent a logical CPU core
	/// </summary>
	public class Core
	{
		/// <summary>
		///     Core "ID"
		/// </summary>
		public uint Number { get; set; }

		/// <summary>
		///     Maximum reached clock speed in MHz. This measurement can be disabled.
		/// </summary>
		public uint MaxClockSpeed { get; set; }

		/// <summary>
		///     Nominal clock speed in MHz (without downclocking, turbo, pbo etc.)
		/// </summary>
		public uint NormalClockSpeed { get; set; }

		/// <summary>
		///     Reference maximum frequency as reported by CPUID 0x16
		/// </summary>
		public uint ReferenceMaxClockSpeed { get; set; }

		/// <summary>
		///     Reference base frequency as reported by CPUID 0x16
		/// </summary>
		public uint ReferenceNormalClockSpeed { get; set; }


		/// <summary>
		///     Reference bus frequency as reported by CPUID 0x16
		/// </summary>
		public uint ReferenceBusSpeed { get; set; }


		/// <summary>
		///     NUMA Node this core resides in
		/// </summary>
		public uint Node { get; set; } = 0;

		/// <summary>
		///     The physical core this logical core resides in
		/// </summary>
		public uint CoreId { get; set; } = 0;
	}
}