#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers.Unix;

public abstract class UnixHelperInformationProvider : InformationProvider
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    protected bool ReadFile(string file, out string data)
    {
        if (File.Exists(file))
        {
            try
            {
                File.OpenRead(file).Dispose();
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered error while trying to open file {File}", file);
                data = null;
                return false;
            }

            data = File.ReadAllText(file);
            return true;
        }

        data = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    protected bool ReadFileAsLines(string file, out string[] lines)
    {
        if (File.Exists(file))
        {
            try
            {
                File.OpenRead(file).Dispose();
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered error while trying to open file {File}", file);
                lines = null;
                return false;
            }

            lines = File.ReadAllLines(file);
            return true;
        }

        lines = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    protected bool GetFromStringWithRegex(string data, string regex, out Match match)
    {
        match = new Regex(regex).Match(data);

        return match.Success;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    protected bool GetFromStringsWithRegex(IEnumerable<string> data, string regex, out Match match)
    {
        match = null;
        foreach (var line in data)
        {
            if (GetFromStringWithRegex(line, regex, out match))
            {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    protected bool GetValueFromStartingText(IEnumerable<string> data, string startingText, out string value)
    {
        var regex = $@"^{startingText}\s+:\s+(.+)";
        if (GetFromStringsWithRegex(data, regex, out var match))
        {
            value = match.Groups[1].Value;
            return true;
        }

        value = null;
        return false;
    }
}