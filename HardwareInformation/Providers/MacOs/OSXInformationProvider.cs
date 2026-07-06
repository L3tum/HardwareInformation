#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HardwareInformation.Information;
using HardwareInformation.Providers.Unix;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers.MacOs;

/// <summary>
///     macOS CPU information provider using per-key sysctl queries (avoids sysctl -a).
/// </summary>
public class OSXInformationProvider : UnixHelperInformationProvider
{
    /// <summary>
    ///     Returns true if running on macOS.
    /// </summary>
    public override bool Available(MachineInformation information)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }

    /// <summary>
    ///     Queries individual sysctl keys to build CPU topology (vendor, name, cores).
    /// </summary>
    protected override void IdentifyCpus(MachineInformation information)
    {
        try
        {
            // macOS does not offer a way to retrieve multiple CPUs, and I don't think there are any Macs like that to begin with.
            if (information.Cpus.Count > 1)
            {
                return;
            }

            if (information.Cpus.Count == 0)
            {
                information.Cpus = new List<CPU> { new() }.ToList().AsReadOnly();
                information.Cpus[0].InitializeLists();
            }

            // Query each sysctl key individually to avoid privilege requirements and
            // AccessViolationException on Intel macOS (12.16.3). Individual queries are
            // faster and don't require root.
            string value;

            if (GetSysctlValue(@"machdep.cpu.vendor", out value))
            {
                information.Cpu.Vendor = value.Trim();
            }

            if (GetSysctlValue(@"machdep.cpu.brand_string", out value))
            {
                information.Cpu.Caption = value.Trim();
            }

            if (GetSysctlValue(@"machdep.cpu.family", out value))
            {
                information.Cpu.Family = uint.Parse(value.Trim());
            }

            if (GetSysctlValue(@"machdep.cpu.model", out value))
            {
                information.Cpu.Model = uint.Parse(value.Trim());
            }

            if (GetSysctlValue(@"machdep.cpu.stepping", out value))
            {
                information.Cpu.Stepping = uint.Parse(value.Trim());
            }

            if (GetSysctlValue(@"hw.physicalcpu", out value))
            {
                information.Cpu.PhysicalCores = uint.Parse(value.Trim());
            }

            if (GetSysctlValue(@"hw.logicalcpu", out value))
            {
                information.Cpu.LogicalCoresInCpu = Enumerable.Range(0, int.Parse(value.Trim())).Select(number => (uint)number).ToHashSet();
                information.Cpu.InitializeLists();
            }

            // ARM Macs use this instead of machdep.cpu.family
            if (GetSysctlValue(@"hw.cpufamily", out value))
            {
                information.Cpu.Family = uint.Parse(value.Trim());
            }
        }
        catch (Exception e)
        {
            MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing information from sysctl on OSX");
        }
    }

    /// <summary>
    ///     Queries a single sysctl value by key. Does not require elevated privileges and avoids
    ///     AccessViolationException on Intel macOS that can occur with sysctl -a.
    /// </summary>
    /// <param name="key">The sysctl key (e.g. machdep.cpu.vendor)</param>
    /// <param name="value">The resolved value</param>
    /// <returns>true if the key exists and was read successfully</returns>
    private static bool GetSysctlValue(string key, out string value)
    {
        value = null;
        try
        {
            using var p = Util.StartProcess("sysctl", key);
            using var sr = p.StandardOutput;
            p.WaitForExit();
            var output = sr.ReadToEnd().Trim();

            if (string.IsNullOrEmpty(output))
            {
                return false;
            }

            // Output format: "key: value"
            var parts = output.Split(':');
            if (parts.Length >= 2)
            {
                value = string.Join(":", parts.Skip(1)).Trim();
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            MachineInformationGatherer.Logger.LogDebug(e, "sysctl query failed for {Key}", key);
            return false;
        }
    }
}