#region using

using System.Runtime.InteropServices;

#endregion

namespace HardwareInformation.Providers
{
	internal class OSXInformationProvider : InformationProvider
	{
		public void GatherInformation(ref MachineInformation information)
		{
			if (information.Cpu.Name == default || information.Cpu.NormalClockSpeed == default)
			{
				using (var p = Util.StartProcess("sysctl", "-n machdep.cpu.brand_string"))
				{
					using (var sr = p.StandardOutput)
					{
						p.WaitForExit();

						var info = sr.ReadToEnd().Trim().Split('@');

						if (info[1].EndsWith("GHz"))
						{
							info[1] = (int.Parse(info[1].Replace("GHz", "")) * 1000).ToString();
						}

						if (info[1].EndsWith("KHz"))
						{
							info[1] = (int.Parse(info[1].Replace("KHz", "")) / 1000).ToString();
						}

						info[1] = info[1].Replace("MHz", "").Trim();

						information.Cpu.Name = info[0];
						information.Cpu.NormalClockSpeed = uint.Parse(info[1]);
					}
				}
			}

			if (information.Cpu.PhysicalCores == default)
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

			if (information.Cpu.LogicalCores == default)
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
		}

		public bool Available(MachineInformation information)
		{
			return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		}
	}
}