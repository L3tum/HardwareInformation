#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers.Unix;

/// <summary>
///     Helper class for Linux/macOS providers that read system files (/proc, /sys) and use regex.
/// </summary>
public abstract class UnixHelperInformationProvider : InformationProvider
{
    /// <summary>
    ///     Reads an entire file as a string. Returns false if file doesn't exist or is unreadable.
    /// </summary>
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

    /// <summary>
    ///     Reads an entire file as an array of lines. Returns false if file doesn't exist or is unreadable.
    /// </summary>
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

    /// <summary>
    ///     Matches a regex against a string. Returns the match if successful.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    protected bool GetFromStringWithRegex(string data, string regex, out Match match)
    {
        match = new Regex(regex).Match(data);

        return match.Success;
    }

    /// <summary>
    ///     Matches a regex against a collection of strings. Returns the first successful match.
    /// </summary>
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

    /// <summary>
    ///     Finds a line that starts with a given text and extracts the value after the colon.
    /// </summary>
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