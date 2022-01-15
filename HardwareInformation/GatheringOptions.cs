namespace HardwareInformation
{
    /// <summary>
    ///     Gathering options to decide which gathering functions to call
    /// </summary>
    public class GatheringOptions
    {
        /// <summary>
        ///     USB Information
        /// </summary>
        public bool GatherUsbInformation { get; set; } = false;

        /// <summary>
        ///     CPU Information
        ///     NOTE: Some information needs to be gathered regardless of this option
        /// </summary>
        public bool GatherCpuInformation { get; set; } = true;

        /// <summary>
        ///     Feature flag information for the CPU
        /// </summary>
        public bool GatherCpuFeatureFlagInformation { get; set; } = true;

        /// <summary>
        ///     CPU speed information
        ///     NOTE: Does not replace the skipClockSpeedTest argument for the Gatherer
        /// </summary>
        public bool GatherCpuSpeedInformation { get; set; } = true;

        /// <summary>
        ///     CPU Cache Topology information
        /// </summary>
        public bool GatherCpuCacheTopologyInformation { get; set; } = true;

        /// <summary>
        ///     GPU Information
        /// </summary>
        public bool GatherGpuInformation { get; set; } = true;

        /// <summary>
        ///     Hard drive information
        /// </summary>
        public bool GatherDiskInformation { get; set; } = true;

        /// <summary>
        ///     RAM information
        /// </summary>
        public bool GatherRamInformation { get; set; } = true;

        /// <summary>
        ///     Mainboard and BIOS information
        /// </summary>
        public bool GatherMainboardInformation { get; set; } = true;

        /// <summary>
        ///     Monitor information
        /// </summary>
        public bool GatherMonitorInformation { get; set; } = true;

        /// <summary>
        ///     Gather per-core information
        /// </summary>
        public bool GatherPerCoreInformation { get; set; } = false;
    }
}