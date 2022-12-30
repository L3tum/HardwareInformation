using System;

namespace HardwareInformation.Information
{
    /// <summary>
    ///     Represents a single connected PNP device
    /// </summary>
    public class PnpDevice
    {
        /// <summary>
        ///     Name of the device according to the device itself.
        ///     ATTENTION: May not be set. <see cref="BusReportedName" /> should always be set.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        ///     Name of the device according to the USB bus
        /// </summary>
        public string BusReportedName { get; internal set; }

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

        /// <summary>
        ///     Manufacturer of this device. May not be set.
        /// </summary>
        public string Manufacturer { get; internal set; }

        /// <summary>
        ///     Device ID to query this device in registry/wmi etc.
        /// </summary>
        public string DeviceID { get; internal set; }

        /// <summary>
        ///     Vendor ID as encoded in the <see cref="DeviceID" />
        /// </summary>
        public string VendorID { get; internal set; }

        /// <summary>
        ///     Product ID as encoded in the <see cref="DeviceID" />
        /// </summary>
        public string ProductID { get; internal set; }

        /// <summary>
        ///     Vendor name as retrieved via <see cref="VendorID" /> from the database
        /// </summary>
        public string VendorName { get; internal set; }

        /// <summary>
        ///     Product name as retrieved via <see cref="ProductID" /> from the database
        /// </summary>
        public string ProductName { get; internal set; }
    }
}