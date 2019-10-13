#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

#endregion

namespace HardwareInformation.Providers
{
	internal class LinuxInformationProvider : InformationProvider
	{
		public void GatherInformation(ref MachineInformation information)
		{
			var info = File.ReadAllLines("/proc/cpuinfo");
			var modelNameRegex = new Regex(@"^model name\s+:\s+(.+)");
			var cpuSpeedRegex = new Regex(@"^cpu MHz\s+:\s+(.+)");
			var physicalCoresRegex = new Regex(@"^cpu cores\s+:\s+(.+)");
			var logicalCoresRegex = new Regex(@"^siblings\s+:\s+(.+)");

			foreach (var s in info)
			{
				var match = modelNameRegex.Match(s);

				if (match.Success && information.Cpu.Name == default)
				{
					information.Cpu.Name = match.Groups[1].Value.Trim();

					continue;
				}

				match = cpuSpeedRegex.Match(s);

				if (match.Success && information.Cpu.NormalClockSpeed == default)
				{
					information.Cpu.NormalClockSpeed = uint.Parse(match.Groups[1].Value);

					continue;
				}

				match = physicalCoresRegex.Match(s);

				if (match.Success && information.Cpu.PhysicalCores == default)
				{
					information.Cpu.PhysicalCores = uint.Parse(match.Groups[1].Value);

					continue;
				}

				match = logicalCoresRegex.Match(s);

				if (match.Success && information.Cpu.LogicalCores == default)
				{
					information.Cpu.LogicalCores = uint.Parse(match.Groups[1].Value);
				}
			}

			if (information.SmBios.BIOSVersion == default)
			{
				try
				{
					information.SmBios.BIOSVersion = File.ReadAllText("/sys/class/dmi/id/bios_version").Trim();
				}
				catch (Exception)
				{
					// Intentionally left blank
				}
			}

			if (information.SmBios.BIOSVendor == default)
			{
				try
				{
					information.SmBios.BIOSVendor = File.ReadAllText("/sys/class/dmi/id/bios_vendor").Trim();
				}
				catch (Exception)
				{
					// Intentionally left blank
				}
			}

			if (information.SmBios.BoardName == default)
			{
				try
				{
					information.SmBios.BoardName = File.ReadAllText("/sys/class/dmi/id/board_name").Trim();
				}
				catch (Exception)
				{
					// Intentionally left blank
				}
			}

			if (information.SmBios.BoardVendor == default)
			{
				try
				{
					information.SmBios.BoardVendor = File.ReadAllText("/sys/class/dmi/id/board_vendor").Trim();
				}
				catch (Exception)
				{
					// Intentionally left blank
				}
			}
		}

		public bool Available(MachineInformation information)
		{
			return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		}
	}
}