#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using HardwareInformation.Information;
using HardwareInformation.Information.Gpu;
using Microsoft.Extensions.Logging;

#endregion

namespace HardwareInformation.Providers
{
    internal class LinuxInformationProvider : InformationProvider
    {
        public override bool Available(MachineInformation information)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }

        public override void GatherCpuInformation(ref MachineInformation information)
        {
            try
            {
                File.OpenRead("/proc/cpuinfo").Dispose();
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing CPU info");
                return;
            }

            var procCpuInfo = File.ReadAllLines("/proc/cpuinfo");
            var modelNameRegex = new Regex(@"^model name\s+:\s+(.+)");
            var cpuSpeedRegex = new Regex(@"^cpu MHz\s+:\s+(.+)");
            var physicalCoresRegex = new Regex(@"^cpu cores\s+:\s+(.+)");
            var logicalCoresRegex = new Regex(@"^siblings\s+:\s+(.+)");

            foreach (var s in procCpuInfo)
            {
                try
                {
                    var match = modelNameRegex.Match(s);

                    if (match.Success)
                    {
                        information.Cpu.Name = match.Groups[1].Value.Trim();

                        continue;
                    }

                    match = cpuSpeedRegex.Match(s);

                    if (match.Success)
                    {
                        information.Cpu.NormalClockSpeed = uint.Parse(match.Groups[1].Value.Split('.', ',')[0]);

                        continue;
                    }

                    match = physicalCoresRegex.Match(s);

                    if (match.Success)
                    {
                        information.Cpu.PhysicalCores = uint.Parse(match.Groups[1].Value);

                        continue;
                    }

                    match = logicalCoresRegex.Match(s);

                    if (match.Success)
                    {
                        information.Cpu.LogicalCores = uint.Parse(match.Groups[1].Value);
                    }
                }
                catch (Exception e)
                {
                    MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing CPU info");
                }
            }

            try
            {
                using var p = Util.StartProcess("lscpu", "");
                using var sr = p.StandardOutput;
                p.WaitForExit();

                var lines = sr.ReadToEnd()
                    .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    if (line.StartsWith("CPU max MHz:") && information.Cpu.MaxClockSpeed == default)
                    {
                        var value = line.Split(':')[1].Trim();
                        value = value.Split('.', ',')[0];

                        information.Cpu.MaxClockSpeed = uint.Parse(value);
                    }
                    else if (line.StartsWith("CPU min MHz:") && information.Cpu.NormalClockSpeed == default)
                    {
                        var value = line.Split(':')[1].Trim();
                        value = value.Split('.', ',')[0];

                        information.Cpu.NormalClockSpeed = uint.Parse(value);
                    }
                    else if (line.StartsWith("CPU(s):") && information.Cpu.LogicalCores == default)
                    {
                        var value = line.Split(':')[1].Trim();

                        information.Cpu.LogicalCores = uint.Parse(value);
                    }
                    else if (line.StartsWith("Core(s) per socket:") && information.Cpu.PhysicalCores == default)
                    {
                        var value = line.Split(':')[1].Trim();

                        information.Cpu.PhysicalCores = uint.Parse(value);
                    }
                }
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing lscpu");
            }
        }

        public override void GatherMainboardInformation(ref MachineInformation information)
        {
            try
            {
                information.SmBios.BIOSVersion = File.ReadAllText("/sys/class/dmi/id/bios_version").Trim();
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing BIOSVersion");
            }

            try
            {
                information.SmBios.BIOSVendor = File.ReadAllText("/sys/class/dmi/id/bios_vendor").Trim();
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing BIOSVendor");
            }

            try
            {
                information.SmBios.BoardName = File.ReadAllText("/sys/class/dmi/id/board_name").Trim();
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing BoardName");
            }

            try
            {
                information.SmBios.BoardVendor = File.ReadAllText("/sys/class/dmi/id/board_vendor").Trim();
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing BoardVendor");
            }
        }

        public override void GatherUsbInformation(ref MachineInformation information)
        {
            var usbs = new List<USBDevice>();

            try
            {
                using var p = Util.StartProcess("lsusb", "");
                using var sr = p.StandardOutput;
                p.WaitForExit();

                var lines = sr.ReadToEnd().Trim()
                    .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                var data = new Dictionary<string, string>();

                foreach (var line in lines)
                {
                    try
                    {
                        var parts = line.Split(' ');
                        var busNumber = parts[1].Trim('0');
                        var deviceNumber = parts[3].Replace(":", "").Trim('0');
                        var deviceId = parts[5];

                        data.Add($"{busNumber}-{deviceNumber}", deviceId);
                    }
                    catch
                    {
                        // Intentionally left blank
                    }
                }

                using var pr = Util.StartProcess("lsusb", "-t");
                using var so = pr.StandardOutput;
                pr.WaitForExit();

                lines = sr.ReadToEnd().Trim()
                    .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                var lastBusNumber = "";

                foreach (var line in lines)
                {
                    var parts = line.Split(' ').ToList();
                    string busNumber;

                    // Sub-device
                    if (parts[0].StartsWith("|_"))
                    {
                        busNumber = lastBusNumber;
                    }
                    // Top-level HUB
                    else
                    {
                        busNumber = parts[2].Split('.')[0].Trim('0');
                        lastBusNumber = busNumber;
                        parts.RemoveAt(1);
                    }

                    var deviceNumber = parts[4].Trim(',');
                    var classSpecifier = parts[5].Split('=')[1].Trim(',');
                    var driverName = parts[6].Split('=')[1].Trim(',');

                    if (data.TryGetValue($"{busNumber}-{deviceNumber}", out var deviceId))
                    {
                        var vendorId = deviceId.Split(':')[0];
                        var productId = deviceId.Split(':')[1];
                        var (vendorName, productName) = USBVendorList.GetVendorAndProductName(vendorId, productId);

                        var usb = new USBDevice
                        {
                            Class = classSpecifier, DriverName = driverName, VendorID = vendorId,
                            ProductID = productId, VendorName = vendorName, ProductName = productName
                        };

                        usbs.Add(usb);
                    }
                }
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing USB info");
            }
            finally
            {
                information.UsbDevices = usbs.AsReadOnly();
            }
        }

        public override void GatherGpuInformation(ref MachineInformation information)
        {
            var gpus = new List<GPU>();

            try
            {
                using var p = Util.StartProcess("lspci", "");
                using var sr = p.StandardOutput;
                p.WaitForExit();

                var lines = sr.ReadToEnd().Trim().Split(new[] {Environment.NewLine},
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    if (line.Contains("VGA compatible controller"))
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
                            else
                            {
                                foreach (var field in typeof(Vendors).GetFields())
                                {
                                    var vendorString = field.GetValue(null) as string;
                                    if (relevant.Contains(vendorString))
                                    {
                                        vendor = vendorString;
                                    }
                                }
                            }

                            var name = string.IsNullOrWhiteSpace(vendor) ? relevant : relevant.Replace(vendor, "");

                            if (!string.IsNullOrEmpty(name))
                            {
                                name = name.Replace("[AMD/ATI]", "");
                            }

                            var gpu = new GPU {Description = relevant, Vendor = vendor, Name = name};

                            gpus.Add(gpu);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing GPU info");
            }
            finally
            {
                information.Gpus = gpus.AsReadOnly();
            }
        }

        public override void GatherDiskInformation(ref MachineInformation information)
        {
            var disks = new List<Disk>();

            try
            {
                using var p = Util.StartProcess("lshw", "-class disk");
                using var sr = p.StandardOutput;
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
                            disks.Add(disk);
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
                    disks.Add(disk);
                }
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing disk info");
            }
            finally
            {
                information.Disks = disks.AsReadOnly();
            }
        }

        public override void GatherRamInformation(ref MachineInformation information)
        {
            var ramSticks = new List<RAM>();

            try
            {
                using var p = Util.StartProcess("lshw", "-short -C memory");
                using var sr = p.StandardOutput;
                p.WaitForExit();
                var lines = sr.ReadToEnd().Split(new[] {Environment.NewLine},
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    // Skip any header or other lines that may be present
                    if (!line.Contains("memory"))
                    {
                        continue;
                    }

                    var relevant = line.Split(new[] {"memory"}, StringSplitOptions.RemoveEmptyEntries)[1]
                        .Trim();

                    // Skip anything but DDR or DIMM so we don't report any false positives
                    if (!relevant.Contains("DDR") && !relevant.Contains("DIMM"))
                    {
                        continue;
                    }

                    var ram = new RAM();
                    var parts = relevant.Split(' ');

                    foreach (var part in parts)
                    {
                        var sizeRegex = new Regex("^([0-9]+)(K|M|G|T)iB");
                        var speedRegex = new Regex("^[0-9]+$");
                        var formFactor = Enum.GetNames(typeof(RAM.FormFactors))
                            .FirstOrDefault(ff => ff == part);

                        if (formFactor != null)
                        {
                            ram.FormFactor =
                                (RAM.FormFactors) Enum.Parse(typeof(RAM.FormFactors), formFactor);
                        }
                        else if (speedRegex.IsMatch(part))
                        {
                            ram.Speed = uint.Parse(part);
                        }
                        else if (sizeRegex.IsMatch(part))
                        {
                            var match = sizeRegex.Match(part);
                            var number = int.Parse(match.Captures[0].Value);
                            var rawNumber = 0uL;
                            var exponent = match.Captures[1].Value;

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

                    ramSticks.Add(ram);
                }
            }
            catch (Exception e)
            {
                MachineInformationGatherer.Logger.LogError(e, "Encountered while parsing RAM");
            }
            finally
            {
                information.RAMSticks = ramSticks.AsReadOnly();
            }
        }
    }
}