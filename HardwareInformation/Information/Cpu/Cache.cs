namespace HardwareInformation.Information.Cpu
{
	/// <summary>
	///     Construct to represent a CPU cache level
	/// </summary>
	public class Cache
    {
	    /// <summary>
	    ///     Cache level (Level1, 2 or 3 Cache)
	    /// </summary>
	    public CacheLevel Level { get; set; }

	    /// <summary>
	    ///     Cache type (instruction, data, unified)
	    /// </summary>
	    public CacheType Type { get; set; }

	    /// <summary>
	    ///     Write-back invalidate.
	    ///     If 0, writing back to this cache automatically invalidates all lower-level caches of cores sharing this cache.
	    ///     If 1, this is not guaranteed to happen.
	    ///     Cast to bool this means 0 => true, 1 => false.
	    /// </summary>
	    public bool WBINVD { get; set; }


	    /// <summary>
	    ///     How many logical cores are using this cache.
	    /// </summary>
	    public uint LogicalCoresPerCache { get; set; }

	    /// <summary>
	    ///     How many physical cores are using this cache.
	    /// </summary>
	    public uint CoresPerCache { get; set; }

	    /// <summary>
	    ///     How many times this exact cache has been found in the processor.
	    /// </summary>
	    public uint TimesPresent { get; set; } = 1;

	    /// <summary>
	    ///     Capacity in bytes
	    /// </summary>
	    public ulong Capacity { get; set; }

	    /// <summary>
	    ///     Capacity in human readable format
	    /// </summary>
	    public string CapacityHRF { get; set; }

	    /// <summary>
	    ///     0xffffffff for fully associative
	    /// </summary>
	    public uint Associativity { get; set; }

	    /// <summary>
	    ///     Line size in bytes
	    /// </summary>
	    public uint LineSize { get; set; }

	    /// <summary>
	    ///     Number of physical line partitions
	    /// </summary>
	    public uint Partitions { get; set; }

	    /// <summary>
	    ///     Number of sets
	    /// </summary>
	    public uint Sets { get; set; }

	    /// <summary>
	    ///     Custom equals for TimesPresent counting
	    /// </summary>
	    /// <param name="obj"></param>
	    /// <returns></returns>
	    public bool CustomEquals(object obj)
        {
            if (obj.GetType() != typeof(Cache))
            {
                return false;
            }

            var cache = (Cache)obj;

            return cache.WBINVD == WBINVD && cache.LineSize == LineSize && cache.Associativity == Associativity &&
                   cache.Capacity == Capacity && cache.LogicalCoresPerCache == LogicalCoresPerCache && cache.Level == Level &&
                   cache.Type == Type && cache.Sets == Sets && cache.Partitions == Partitions;
        }
#pragma warning disable 1591
	    /// <summary>
	    ///     An enum of cache levels and their values
	    /// </summary>
	    public enum CacheLevel : uint
        {
            RESERVED = 0b0,
            LEVEL1 = 0b1,
            LEVEL2 = 0b10,
            LEVEL3 = 0b11,
            RESERVED2 = 0b100,
            RESERVED3 = 0b101,
            RESERVED4 = 0b110,
            RESERVED5 = 0b111
        }

	    /// <summary>
	    ///     The cache types and their values
	    /// </summary>
	    public enum CacheType : uint
        {
            NONE = 0x0,
            DATA = 0x1,
            INSTRUCTION = 0x2,
            UNIFIED = 0x3,
            RESERVED = 0x4,
            RESERVED2 = 0x5,
            RESERVED3 = 0x6,
            RESERVED4 = 0x7,
            RESERVED5 = 0x8,
            RESERVED6 = 0x9,
            RESERVED7 = 0xA,
            RESERVED8 = 0xB,
            RESERVED9 = 0xC,
            RESERVED10 = 0xD,
            RESERVED11 = 0xE,
            RESERVED12 = 0xF
        }
#pragma warning restore 1591
    }
}