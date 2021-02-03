#region using

using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers
{
    internal class OSXInformationProvider : InformationProvider
    {
        public override void GatherCpuInformation(ref MachineInformation information)
        {
            try
            {
                using var p = Util.StartProcess("sysctl", "-n machdep.cpu.brand_string");
                using var sr = p.StandardOutput;
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
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing sysctl brand_string");
            }

            try
            {
                using var p = Util.StartProcess("sysctl", "-n hw.physicalcpu");
                using var sr = p.StandardOutput;
                p.WaitForExit();

                var info = sr.ReadToEnd().Trim();

                information.Cpu.PhysicalCores = uint.Parse(info);
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing sysctl physicalcpu");
            }

            try
            {
                using var p = Util.StartProcess("sysctl", "-n hw.logicalcpu");
                using var sr = p.StandardOutput;
                p.WaitForExit();

                var info = sr.ReadToEnd().Trim();

                information.Cpu.LogicalCores = uint.Parse(info);
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing sysctl logicalcpu");
            }
        }

        public override bool Available(MachineInformation information)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }
    }
}