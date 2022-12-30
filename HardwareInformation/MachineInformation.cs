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
            SmBios = new SMBios();
            Platform = Platforms.Unknown;
        }

	    /// <summary>
	    ///     The operating system installed. Version construct supplied by .NET
	    /// </summary>
	    public OperatingSystem OperatingSystem { get; internal set; }

	    /// <summary>
	    ///     Whether this is at least Windows 10 (this is a specific Windows system call that is quite bad to get right)
	    /// </summary>
	    public bool Windows10 { get; internal set; }

	    /// <summary>
	    ///     The OS platform .NET is running on. More reliable that OperatingSystem
	    /// </summary>
	    public Platforms Platform { get; internal set; }

	    /// <summary>
	    ///     The CPUs that are installed.
	    /// </summary>
	    public IReadOnlyList<CPU> Cpus { get; internal set; } = new List<CPU>();

	    /// <summary>
	    ///     Quick helper for situations when only one CPU is installed. Returns null if multiple are installed.
	    /// </summary>
	    public CPU Cpu
        {
            get
            {
                if (Cpus.Count == 1)
                {
                    return Cpus[0];
                }

                return null;
            }
        }

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
	    public IReadOnlyList<PnpDevice> UsbDevices { get; internal set; } = new List<PnpDevice>().AsReadOnly();

	    /// <summary>
	    ///     PCI Devices connected
	    ///     May include some USB devices and GPUs
	    /// </summary>
	    public IReadOnlyList<PnpDevice> PciDevices { get; internal set; } = new List<PnpDevice>().AsReadOnly();

	    /// <summary>
	    ///     Gets the index of the CPU in the Cpus List by a logical core "number"/"ID"
	    /// </summary>
	    /// <param name="logicalCore"></param>
	    /// <returns></returns>
	    public int? GetIndexOfCpuByLogicalCore(uint logicalCore)
        {
            for (var i = 0; i < Cpus.Count; i++)
            {
                if (Cpus[i].LogicalCoresInCpu.Contains(logicalCore))
                {
                    return i;
                }
            }

            return null;
        }

	    /// <summary>
	    ///     Gets the actual CPU by logical core number <see cref="GetIndexOfCpuByLogicalCore" />
	    /// </summary>
	    /// <param name="logicalCore"></param>
	    /// <returns></returns>
	    public CPU GetCpuByLogicalCore(uint logicalCore)
        {
            var index = GetIndexOfCpuByLogicalCore(logicalCore);

            if (index is not null)
            {
                return Cpus[index.Value];
            }

            return null;
        }
    }
}