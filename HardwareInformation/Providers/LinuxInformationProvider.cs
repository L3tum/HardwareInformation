#region using

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using HardwareInformation.Information;

#endregion

namespace HardwareInformation.Providers
{
	internal class LinuxInformationProvider : InformationProvider
	{
		public void GatherInformation(ref MachineInformation information)
		{
			if (!File.Exists("/proc/cpuinfo"))
			{
				return;
			}

			try
			{
				File.OpenRead("/proc/cpuinfo").Dispose();
			}
			catch (Exception)
			{
				return;
			}

			var info = File.ReadAllLines("/proc/cpuinfo");
			var modelNameRegex = new Regex(@"^model name\s+:\s+(.+)");
			var cpuSpeedRegex = new Regex(@"^cpu MHz\s+:\s+(.+)");
			var physicalCoresRegex = new Regex(@"^cpu cores\s+:\s+(.+)");
			var logicalCoresRegex = new Regex(@"^siblings\s+:\s+(.+)");

			foreach (var s in info)
			{
				try
				{
					var match = modelNameRegex.Match(s);

					if (match.Success && (information.Cpu.Name == default ||
					                      information.Cpu.Name == information.Cpu.Caption))
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

					if (match.Success)
					{
						var val = uint.Parse(match.Groups[1].Value);

						// Safety check
						if (information.Cpu.PhysicalCores == default ||
						    information.Cpu.PhysicalCores == information.Cpu.LogicalCores ||
						    val != 0 && val != information.Cpu.PhysicalCores)
						{
							information.Cpu.PhysicalCores = val;

							continue;
						}
					}

					match = logicalCoresRegex.Match(s);

					if (match.Success)
					{
						var val = uint.Parse(match.Groups[1].Value);

						if (match.Success && information.Cpu.LogicalCores == default ||
						    val != 0 && val != information.Cpu.LogicalCores)
						{
							information.Cpu.LogicalCores = val;
						}
					}
				}
				catch (Exception)
				{
					// Intentionally left blank
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

			GetGPUInformation(ref information);
			GetDiskInformation(ref information);
			GetRAMInformation(ref information, relevant => relevant.Contains("DDR") || relevant.Contains("DIMM"));
			GetRAMInformation(ref information, relevant => relevant.EndsWith("System", StringComparison.Ordinal));
		}

		public bool Available(MachineInformation information)
		{
			return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		}

		public void PostProviderUpdateInformation(ref MachineInformation information)
		{
			// Intentionally left blank
		}

		private void GetGPUInformation(ref MachineInformation machineInformation)
		{
			if (machineInformation.Gpus.Count == 0)
			{
				try
				{
					var p = Util.StartProcess("lspci", "");
					using var sr = p.StandardOutput;
					p.WaitForExit();

					var lines = sr.ReadToEnd().Trim().Split(new[] {Environment.NewLine},
						StringSplitOptions.RemoveEmptyEntries);

					foreach (var line in lines)
					{
						if (line.Contains("VGA compatible controller"))
						{
							try
							{
								var relevant = line.Split(':')[2];

								if (!string.IsNullOrWhiteSpace(relevant))
								{
									var vendor = "";

									if (relevant.Contains("Intel"))
									{
										vendor = "Intel Corporation";
									}
									else if (relevant.Contains("AMD") ||
									         relevant.Contains("Advanced Micro Devices") || relevant.Contains("ATI"))
									{
										vendor = "Advanced Micro Devices, Inc.";
									}
									else if (relevant.ToUpperInvariant().Contains("NVIDIA"))
									{
										vendor = "NVIDIA Corporation";
									}

									var name = relevant.Replace(vendor, "").Replace("[AMD/ATI]", "");

									var gpu = new GPU {Description = relevant, Vendor = vendor, Name = name};

									machineInformation.Gpus.Add(gpu);
								}
							}
							catch
							{
								// Intentionally left blank
							}
						}
					}
				}
				catch
				{
					// Intentionally left blank
				}
			}
		}

		private void GetDiskInformation(ref MachineInformation machineInformation)
		{
			if (machineInformation.Disks.Count == 0)
			{
				try
				{
					var p = Util.StartProcess("lshw", "-class disk");
					var sr = p.StandardOutput;
					p.WaitForExit();
					var lines = sr.ReadToEnd()
						.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

					Disk disk = null;

					foreach (var line in lines)
					{
						if (line.StartsWith("*-"))
						{
							if (disk != null)
							{
								machineInformation.Disks.Add(disk);
							}

							disk = null;
						}

						if (line.StartsWith("*-disk:"))
						{
							disk = new Disk();
							continue;
						}

						if (disk != null)
						{
							if (line.StartsWith("product:"))
							{
								disk.Model = disk.Caption = line.Replace("product:", "").Trim();
							}
							else if (line.StartsWith("vendor:"))
							{
								disk.Vendor = line.Replace("vendor:", "").Trim();
							}
						}
					}

					if (disk != null)
					{
						machineInformation.Disks.Add(disk);
					}
				}
				catch
				{
					// Intentionally left blank
				}
			}
		}

		private void GetRAMInformation(ref MachineInformation machineInformation, Func<string, bool> relevancy)
		{
			if (machineInformation.RAMSticks.Count == 0)
			{
				try
				{
					var p = Util.StartProcess("lshw", "-short -C memory");
					var sr = p.StandardOutput;
					p.WaitForExit();
					var lines = sr.ReadToEnd().Split(new[] {Environment.NewLine},
						StringSplitOptions.RemoveEmptyEntries);

					foreach (var line in lines)
					{
						try
						{
							var relevant = line.Split(new[] {"memory"}, StringSplitOptions.RemoveEmptyEntries)[1]
								.Trim();

							if (relevancy(relevant))
							{
								var ram = new RAM();
								var parts = relevant.Split(' ');

								foreach (var part in parts)
								{
									var sizeRegex = new Regex("^([0-9]+)(K|M|G|T)iB");
									var formFactor = Enum.GetNames(typeof(RAM.FormFactors))
										.FirstOrDefault(ff => ff == part);

									if (formFactor != null)
									{
										ram.FormFactor =
											(RAM.FormFactors) Enum.Parse(typeof(RAM.FormFactors), formFactor);
									}
									else if (new Regex("^[0-9]+$").IsMatch(part))
									{
										ram.Speed = uint.Parse(part);
									}
									else if (sizeRegex.IsMatch(part))
									{
										var match = sizeRegex.Match(part);
										var number = int.Parse(match.Groups[1].Value);
										var rawNumber = 0uL;
										var exponent = match.Groups[2].Value;

										if (exponent == "T")
										{
											rawNumber = (ulong) number * 1024uL * 1024uL * 1024uL * 1024uL;
										}
										else if (exponent == "G")
										{
											rawNumber = (ulong) number * 1024uL * 1024uL * 1024uL;
										}
										else if (exponent == "M")
										{
											rawNumber = (ulong) number * 1024uL * 1024uL;
										}
										else if (exponent == "K")
										{
											rawNumber = (ulong) number * 1024uL;
										}
										else
										{
											// Oof
											rawNumber = (ulong) number;
										}

										ram.Capacity = rawNumber;
										ram.CapacityHRF = match.Value;
									}
								}

								machineInformation.RAMSticks.Add(ram);
							}
						}
						catch
						{
							// Intentionally left blank
						}
					}
				}
				catch
				{
					// Intentionally left blank
				}
			}
		}
	}
}