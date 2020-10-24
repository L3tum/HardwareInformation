using System;

namespace HardwareInformation.Information
{
    /// <summary>
    ///     Represents a single connected USB device
    /// </summary>
    public class USBDevice
    {
        /// <summary>
        ///     Name of the device according to the USB bus
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        ///     Name of the driver. May contain more information than Name itself (such as Manufacturer)
        /// </summary>
        public string DriverName { get; internal set; }

        /// <summary>
        ///     Version of the driver
        /// </summary>
        public string DriverVersion { get; internal set; }

        /// <summary>
        ///     Date the driver was published
        /// </summary>
        public DateTime DriverDate { get; internal set; }

        /// <summary>
        ///     Class of the USB Device (e.g. SCSIAdapter)
        /// </summary>
        public string Class { get; internal set; }

        /// <summary>
        ///     Provider of the drivers. Mostly Microsoft, but may contain Manufacturer
        /// </summary>
        public string DriverProvider { get; internal set; }
    }
}