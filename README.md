# HardwareInformation
.NET Core Cross-Platform Hardware Information Gatherer


## What gets collected for each OS?

### Windows:
	Cores (Physical + Logical)
	Architecture
	Processor Caption
	Processor Name
	L2CacheSize
	L3CacheSize
	Socket
	MaxClockSpeed
	RAM Speed
	RAM Manufacturer
	RAM Size
	BIOS Version

### Linux:
	Cores (Physical + Logical)
	Architecture
	Processor Caption
	Processor Name
		NOT L2CacheSize
		NOT L3CacheSize
		NOT Socket
	MaxClockSpeed
	RAM Speed
	RAM Manufacturer
	RAM Size
	BIOS Version

### Mac:
	Cores (Physical + Logical)
	Architecture
	Processor Caption
	Processor Name
		NOT L2CacheSize
		NOT L3CacheSize
		NOT Socket
	MaxClockSpeed
		NOT RAM Speed
		NOT RAM Manufacturer
	RAM Size
		NOT BIOS Version

### FreeBSD (.NET Core 3):
	Cores (NOT Physical BUT Logical)
	Architecture
	Processor Caption
	Processor Name
		NOT L2CacheSize
		NOT L3CacheSize
	Socket
	MaxClockSpeed
	RAM Speed
	RAM Manufacturer
	RAM Size
		NOT BIOS Version
