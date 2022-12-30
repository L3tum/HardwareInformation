namespace HardwareInformation.Providers.Linux;

public class LinuxRamInformationProvider : LinuxInformationProvider
{
    //         [SupportedOSPlatform("linux")]
//         public override void GatherRamInformation(ref MachineInformation information)
//         {
//             var ramSticks = new List<RAM>();
//
//             try
//             {
//                 using var p = Util.StartProcess("lshw", "-short -C memory");
//                 using var sr = p.StandardOutput;
//                 p.WaitForExit();
//                 var lines = sr.ReadToEnd().Split(new[] {Environment.NewLine},
//                     StringSplitOptions.RemoveEmptyEntries);
//
//                 foreach (var line in lines)
//                 {
//                     // Skip any header or other lines that may be present
//                     if (!line.Contains("memory"))
//                     {
//                         continue;
//                     }
//
//                     var relevant = line.Split(new[] {"memory"}, StringSplitOptions.RemoveEmptyEntries)[1]
//                         .Trim();
//
//                     // Skip anything but DDR or DIMM so we don't report any false positives
//                     if (!relevant.Contains("DDR") && !relevant.Contains("DIMM"))
//                     {
//                         continue;
//                     }
//
//                     var ram = new RAM();
//                     var parts = relevant.Split(' ');
//
//                     foreach (var part in parts)
//                     {
//                         var sizeRegex = new Regex("^([0-9]+)(K|M|G|T)iB");
//                         var speedRegex = new Regex("^[0-9]+$");
//                         var formFactor = Enum.GetNames(typeof(RAM.FormFactors))
//                             .FirstOrDefault(ff => ff == part);
//
//                         if (formFactor != null)
//                         {
//                             ram.FormFactor =
//                                 (RAM.FormFactors) Enum.Parse(typeof(RAM.FormFactors), formFactor);
//                         }
//                         else if (speedRegex.IsMatch(part))
//                         {
//                             ram.Speed = uint.Parse(part);
//                         }
//                         else if (sizeRegex.IsMatch(part))
//                         {
//                             var match = sizeRegex.Match(part);
//                             var number = int.Parse(match.Captures[0].Value);
//                             var rawNumber = 0uL;
//                             var exponent = match.Captures[1].Value;
//
//                             if (exponent == "T")
//                             {
//                                 rawNumber = (ulong) number * 1024uL * 1024uL * 1024uL * 1024uL;
//                             }
//                             else if (exponent == "G")
//                             {
//                                 rawNumber = (ulong) number * 1024uL * 1024uL * 1024uL;
//                             }
//                             else if (exponent == "M")
//                             {
//                                 rawNumber = (ulong) number * 1024uL * 1024uL;
//                             }
//                             else if (exponent == "K")
//                             {
//                                 rawNumber = (ulong) number * 1024uL;
//                             }
//                             else
//                             {
//                                 // Oof
//                                 rawNumber = (ulong) number;
//                             }
//
//                             ram.Capacity = rawNumber;
//                             ram.CapacityHRF = match.Value;
//                         }
//                     }
//
//                     ramSticks.Add(ram);
//                 }
//             }
//             catch (Exception e)
//             {
//                 MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing RAM");
//             }
//             finally
//             {
//                 information.RAMSticks = ramSticks.AsReadOnly();
//             }
//         }

}