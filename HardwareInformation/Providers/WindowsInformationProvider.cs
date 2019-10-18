#region using

using System;
using System.Management;
using System.Runtime.InteropServices;

#endregion

namespace HardwareInformation.Providers
{
	internal class WindowsInformationProvider : InformationProvider
	{
		public void GatherInformation(ref MachineInformation information)
		{
			var mos = new ManagementObjectSearcher(
				"select Name,NumberOfEnabledCore,NumberOfLogicalProcessors,SocketDesignation,MaxClockSpeed from Win32_Processor");

			foreach (var managementBaseObject in mos.Get())
			{
				foreach (var propertyData in managementBaseObject.Properties)
				{
					switch (propertyData.Name)
					{
						case "Name":
						{
							if (information.Cpu.Name == default)
							{
								information.Cpu.Name = propertyData.Value.ToString().Trim();
							}

							break;
						}

						// MIND THE SSSSSSSS
						case "NumberOfEnabledCore":
						{
                            var val = uint.Parse(propertyData.Value.ToString());

                            // Safety check
                            if (information.Cpu.PhysicalCores == default || information.Cpu.PhysicalCores == information.Cpu.LogicalCores || (val != 0 && val != information.Cpu.PhysicalCores))
							{
                                information.Cpu.PhysicalCores = val;
							}

							break;
						}

						case "NumberOfLogicalProcessors":
						{
                            var val = uint.Parse(propertyData.Value.ToString());

                            if (information.Cpu.LogicalCores == default || (val != 0 && val != information.Cpu.LogicalCores))
							{
								information.Cpu.LogicalCores = val;
							}

							break;
						}

						case "SocketDesignation":
						{
							if (information.Cpu.Socket == default)
							{
								information.Cpu.Socket = propertyData.Value.ToString().Trim();
							}

							break;
						}

						case "MaxClockSpeed":
						{
							if (information.Cpu.NormalClockSpeed == default)
							{
								information.Cpu.NormalClockSpeed = uint.Parse(propertyData.Value.ToString());
							}

							break;
						}
					}
				}
			}

			// There is currently no other way to gather RAM information so we don't need to check if it's already set
			mos = new ManagementObjectSearcher(
				"select ConfiguredClockSpeed,Manufacturer,Capacity,DeviceLocator,PartNumber,FormFactor from Win32_PhysicalMemory");

			foreach (var managementBaseObject in mos.Get())
			{
				var ram = new MachineInformation.RAM();

				foreach (var propertyData in managementBaseObject.Properties)
				{
					switch (propertyData.Name)
					{
						case "ConfiguredClockSpeed":
						{
							ram.Speed = uint.Parse(propertyData.Value.ToString());

							break;
						}

						case "Manufacturer":
						{
							ram.Manfucturer = propertyData.Value.ToString();

							break;
						}

						case "Capacity":
						{
							ram.Capacity += ulong.Parse(propertyData.Value.ToString());

							break;
						}

						case "DeviceLocator":
						{
							ram.Name = propertyData.Value.ToString();

							break;
						}

						case "PartNumber":
						{
							ram.PartNumber = propertyData.Value.ToString();

							break;
						}

						case "FormFactor":
						{
							ram.FormFactor = (MachineInformation.RAM.FormFactors) Enum.Parse(
								typeof(MachineInformation.RAM.FormFactors), propertyData.Value.ToString());

							break;
						}
					}
				}

				ram.CapacityHRF = Util.FormatBytes(ram.Capacity);

				information.RAMSticks.Add(ram);
			}

			mos = new ManagementObjectSearcher("select Name,Manufacturer,Version from Win32_BIOS");

			foreach (var managementBaseObject in mos.Get())
			{
				foreach (var propertyData in managementBaseObject.Properties)
				{
					switch (propertyData.Name)
					{
						case "Name":
						{
							information.SmBios.BIOSVersion = propertyData.Value.ToString();

							break;
						}

						case "Manufacturer":
						{
							information.SmBios.BIOSVendor = propertyData.Value.ToString();

							break;
						}

						case "Version":
						{
							information.SmBios.BIOSCodename = propertyData.Value.ToString();

							break;
						}
					}
				}
			}

			mos = new ManagementObjectSearcher("select Product,Manufacturer,Version from Win32_BaseBoard");

			foreach (var managementBaseObject in mos.Get())
			{
				foreach (var propertyData in managementBaseObject.Properties)
				{
					switch (propertyData.Name)
					{
						case "Product":
						{
							information.SmBios.BoardName = propertyData.Value.ToString();

							break;
						}

						case "Manufacturer":
						{
							information.SmBios.BoardVendor = propertyData.Value.ToString();

							break;
						}

						case "Version":
						{
							information.SmBios.BoardVersion = propertyData.Value.ToString();

							break;
						}
					}
				}
			}
		}

		public bool Available(MachineInformation information)
		{
			return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		}
	}
}