#region using

using System;
using System.Runtime.InteropServices;

#endregion

namespace HardwareInformation.Providers
{
	internal class OSXInformationProvider : InformationProvider
	{
		public void GatherInformation(ref MachineInformation information)
		{
			if (information.Cpu.Name == default || information.Cpu.NormalClockSpeed == default ||
			    information.Cpu.Name == information.Cpu.Caption)
			{
				try
				{
					using (var p = Util.StartProcess("sysctl", "-n machdep.cpu.brand_string"))
					{
						using (var sr = p.StandardOutput)
						{
							p.WaitForExit();

							var info = sr.ReadToEnd().Trim().Split('@');

							info[1] = info[1].Trim();

							if (info[1].EndsWith("GHz"))
							{
								info[1] = ((uint) (double.Parse(info[1].Replace("GHz", "").Replace(" ", "")) * 1000))
									.ToString();
							}
							else if (info[1].EndsWith("KHz"))
							{
								info[1] = ((uint) (double.Parse(info[1].Replace("KHz", "")) / 1000)).ToString();
							}
							else
							{
								info[1] = info[1].Replace("MHz", "").Trim();
							}

							information.Cpu.Name = info[0];
							information.Cpu.NormalClockSpeed = uint.Parse(info[1]);
						}
					}
				}
				catch (Exception)
				{
					// Intentionally left blank
				}
			}

			// Safety check
			if (information.Cpu.PhysicalCores == default ||
			    information.Cpu.PhysicalCores == information.Cpu.LogicalCores)
			{
				try
				{
					using (var p = Util.StartProcess("sysctl", "-n hw.physicalcpu"))
					{
						using (var sr = p.StandardOutput)
						{
							p.WaitForExit();

							var info = sr.ReadToEnd().Trim();

							information.Cpu.PhysicalCores = uint.Parse(info);
						}
					}
				}
				catch (Exception)
				{
					// Intentionally left blank
				}
			}

			if (information.Cpu.LogicalCores == default)
			{
				try
				{
					using (var p = Util.StartProcess("sysctl", "-n hw.logicalcpu"))
					{
						using (var sr = p.StandardOutput)
						{
							p.WaitForExit();

							var info = sr.ReadToEnd().Trim();

							information.Cpu.LogicalCores = uint.Parse(info);
						}
					}
				}
				catch (Exception)
				{
					// Intentionally left blank
				}
			}
		}

		public bool Available(MachineInformation information)
		{
			return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		}

		public void PostProviderUpdateInformation(ref MachineInformation information)
		{
			// Intentionally left blank
		}
	}
}