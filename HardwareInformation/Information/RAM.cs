#region using

using System;

#endregion

namespace HardwareInformation.Information
{
	/// <summary>
	///     Construct to represent a RAM/memory module
	/// </summary>
	public class RAM
	{
#pragma warning disable 1591
		/// <summary>
		///     The various different FormFactors and their values.
		/// </summary>
		public enum FormFactors
		{
			UNKNOWN = 0,
			OTHER = 1,
			SIP = 2,
			DIP = 3,
			ZIP = 4,
			SOJ = 5,
			PROPRIETARY = 6,
			SIMM = 7,
			DIMM = 8,
			TSOP = 9,
			PGA = 10,
			RIMM = 11,
			SODIMM = 12,
			SRIMM = 13,
			SMD = 14,
			SSMP = 15,
			QFP = 16,
			TQFP = 17,
			SOIC = 18,
			LCC = 19,
			PLCC = 20,
			BGA = 21,
			FPBGA = 22,
			LGA = 23
		}
#pragma warning restore 1591
		/// <summary>
		///     Speed in MHz
		/// </summary>
		public uint Speed { get; set; }

		/// <summary>
		///     Manufacturer of the module, can be your vendor (Corsair for example) or the actual manufacturer (like Samsung)
		/// </summary>
		[Obsolete]
		public string Manfucturer
		{
			get => Manufacturer;
			internal set => Manufacturer = value;
		}

		/// <summary>
		///     Manufacturer of the module, can be your vendor (Corsair for example) or the actual manufacturer (like Samsung)
		/// </summary>
		public string Manufacturer { get; internal set; }

		/// <summary>
		///     Capacity in bytes
		/// </summary>
		public ulong Capacity { get; internal set; }

		/// <summary>
		///     Capacity in human readable format
		/// </summary>
		public string CapacityHRF { get; internal set; }

		/// <summary>
		///     The "name" of the memory module, like DIMM-A1 etc.
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		///     The partnumber of the memory module, mostly the specifier that can be used to search for it on Google
		/// </summary>
		public string PartNumber { get; internal set; }

		/// <summary>
		///     FormFactor of the module (DIMM vs SODIMM etc.)
		/// </summary>
		public FormFactors FormFactor { get; internal set; }
	}
}