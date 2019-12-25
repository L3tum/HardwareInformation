#region using

using System;
using System.Collections.Generic;
using HardwareInformation.Information;

#endregion

namespace HardwareInformation
{
	/// <summary>
	///     Holds all the information that the MachineInformationGatherer can gather
	/// </summary>
	public class MachineInformation
	{
		/// <summary>
		///     Operating system enum
		/// </summary>
		public enum Platforms
		{
			/// <summary>
			///     Unknwon
			/// </summary>
			Unknown,

			/// <summary>
			///     Linux
			/// </summary>
			Linux,

			/// <summary>
			///     Windows
			/// </summary>
			Windows,

			/// <summary>
			///     OSX
			/// </summary>
			OSX
		}

		/// <summary>
		///     Creates and initializes a new MachineInformation object
		/// </summary>
		public MachineInformation()
		{
			Cpu = new CPU();
			RAMSticks = new List<RAM>();
			SmBios = new SMBios();
			Platform = Platforms.Unknown;
			Disks = new List<Disk>();
			Gpus = new List<GPU>();
			Displays = new List<Display>();
		}

		/// <summary>
		///     The operating system installed. Version construct supplied by .NET
		/// </summary>
		public OperatingSystem OperatingSystem { get; set; }

		/// <summary>
		///     The OS platform .NET is running on. More reliable that OperatingSystem
		/// </summary>
		public Platforms Platform { get; set; }

		/// <summary>
		///     The CPU that's installed. Can't handle multi-processor environments for now
		/// </summary>
		public CPU Cpu { get; set; }

		/// <summary>
		///     The SMBios information (mainly BIOS and Mainboard)
		/// </summary>
		public SMBios SmBios { get; set; }

		/// <summary>
		///     All the individual RAM sticks installed
		/// </summary>
		public List<RAM> RAMSticks { get; set; }

		/// <summary>
		///     Disks installed
		/// </summary>
		public List<Disk> Disks { get; set; }

		/// <summary>
		///     GPUs installed
		/// </summary>
		public List<GPU> Gpus { get; set; }


		/// <summary>
		///     Displays connected
		/// </summary>
		public List<Display> Displays { get; set; }
	}
}