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
	    internal MachineInformation()
        {
            Cpu = new CPU();
            SmBios = new SMBios();
            Platform = Platforms.Unknown;
        }

	    /// <summary>
	    ///     The operating system installed. Version construct supplied by .NET
	    /// </summary>
	    public OperatingSystem OperatingSystem { get; internal set; }

	    /// <summary>
	    ///     The OS platform .NET is running on. More reliable that OperatingSystem
	    /// </summary>
	    public Platforms Platform { get; internal set; }

	    /// <summary>
	    ///     The CPU that's installed. Can't handle multi-processor environments for now
	    /// </summary>
	    public CPU Cpu { get; internal set; }

	    /// <summary>
	    ///     The SMBios information (mainly BIOS and Mainboard)
	    /// </summary>
	    public SMBios SmBios { get; internal set; }

	    /// <summary>
	    ///     All the individual RAM sticks installed
	    /// </summary>
	    public IReadOnlyList<RAM> RAMSticks { get; internal set; } = new List<RAM>().AsReadOnly();

	    /// <summary>
	    ///     Disks installed
	    /// </summary>
	    public IReadOnlyList<Disk> Disks { get; internal set; } = new List<Disk>().AsReadOnly();

	    /// <summary>
	    ///     GPUs installed
	    /// </summary>
	    public IReadOnlyList<GPU> Gpus { get; internal set; } = new List<GPU>().AsReadOnly();


	    /// <summary>
	    ///     Displays connected
	    /// </summary>
	    public IReadOnlyList<Display> Displays { get; internal set; } = new List<Display>().AsReadOnly();

	    /// <summary>
	    ///     USB Devices connected
	    /// </summary>
	    public IReadOnlyList<USBDevice> UsbDevices { get; internal set; } = new List<USBDevice>().AsReadOnly();
    }
}