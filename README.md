# HardwareInformation
.NET Core Cross-Platform Hardware Information Gatherer

![GitHub release (latest by date)](https://img.shields.io/github/v/release/L3tum/HardwareInformation?style=flat-square)
![Nuget](https://img.shields.io/nuget/v/HardwareInformation?style=flat-square)

## Usage

Download from [Nuget](https://www.nuget.org/packages/HardwareInformation/) via your favorite Nuget client like dotnet

`dotnet add package HardwareInformation`

Get hardware information from the gatherer

`MachinInformation info = MachineInformationGatherer.GatherInformation()`

Result is cached internally so don't worry about calling it multiple times


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
