#region using

using HardwareInformation.Information.Gpu;

#endregion

namespace HardwareInformation.Information
{
	/// <summary>
	///     One installed GPU in the system
	/// </summary>
	public class GPU
    {
	    /// <summary>
	    ///     Vendor, or as WMI calls it, AdapterCompatibility. One of Gpu.Vendors
	    /// </summary>
	    public string Vendor { get; internal set; }

	    /// <summary>
	    ///     Description
	    /// </summary>
	    public string Description { get; internal set; }

	    /// <summary>
	    ///     Caption
	    /// </summary>
	    public string Caption { get; internal set; }

	    /// <summary>
	    ///     Name
	    /// </summary>
	    public string Name { get; internal set; }

	    /// <summary>
	    ///     Date of the driver in use
	    /// </summary>
	    public string DriverDate { get; internal set; }

	    /// <summary>
	    ///     Version of the driver in use
	    /// </summary>
	    public string DriverVersion { get; internal set; }

	    /// <summary>
	    ///     Status of the GPU
	    /// </summary>
	    public string Status { get; internal set; }

	    /// <summary>
	    ///     Type of the GPU
	    /// </summary>
	    public DeviceType Type { get; internal set; }

	    /// <summary>
	    ///     The supported Vulkan API Version
	    /// </summary>
	    public string SupportedVulkanApiVersion { get; internal set; }

	    /// <summary>
	    ///     The available VRAM on this card
	    /// </summary>
	    public ulong AvailableVideoMemory { get; internal set; }

	    /// <summary>
	    ///     The available VRAM in Human-readable-form on this card
	    /// </summary>
	    public string AvailableVideoMemoryHRF { get; internal set; }

	    /// <summary>
	    ///     Vendor ID of this card
	    /// </summary>
	    public string VendorID { get; internal set; }

	    /// <summary>
	    ///     Device ID of this card
	    /// </summary>
	    public string DeviceID { get; internal set; }
    }
}