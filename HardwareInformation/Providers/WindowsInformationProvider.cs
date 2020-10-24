#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using HardwareInformation.Information;

#endregion

namespace HardwareInformation.Providers
{
    #region Win32API

    // Taken from https://github.com/pruggitorg/detect-windows-version
    internal enum ProductType : byte
    {
        /// <summary>
        ///     The operating system is Windows 10, Windows 8, Windows 7,...
        /// </summary>
        /// <remarks>VER_NT_WORKSTATION</remarks>
        Workstation = 0x0000001,

        /// <summary>
        ///     The system is a domain controller and the operating system is Windows Server.
        /// </summary>
        /// <remarks>VER_NT_DOMAIN_CONTROLLER</remarks>
        DomainController = 0x0000002,

        /// <summary>
        ///     The operating system is Windows Server. Note that a server that is also a domain controller
        ///     is reported as VER_NT_DOMAIN_CONTROLLER, not VER_NT_SERVER.
        /// </summary>
        /// <remarks>VER_NT_SERVER</remarks>
        Server = 0x0000003
    }


    // Taken from https://github.com/pruggitorg/detect-windows-version
    [Flags]
    internal enum SuiteMask : ushort
    {
        /// <summary>
        ///     Microsoft BackOffice components are installed.
        /// </summary>
        VER_SUITE_BACKOFFICE = 0x00000004,

        /// <summary>
        ///     Windows Server 2003, Web Edition is installed
        /// </summary>
        VER_SUITE_BLADE = 0x00000400,

        /// <summary>
        ///     Windows Server 2003, Compute Cluster Edition is installed.
        /// </summary>
        VER_SUITE_COMPUTE_SERVER = 0x00004000,

        /// <summary>
        ///     Windows Server 2008 Datacenter, Windows Server 2003, Datacenter Edition, or Windows 2000 Datacenter Server is
        ///     installed.
        /// </summary>
        VER_SUITE_DATACENTER = 0x00000080,

        /// <summary>
        ///     Windows Server 2008 Enterprise, Windows Server 2003, Enterprise Edition, or Windows 2000 Advanced Server is
        ///     installed.
        ///     Refer to the Remarks section for more information about this bit flag.
        /// </summary>
        VER_SUITE_ENTERPRISE = 0x00000002,

        /// <summary>
        ///     Windows XP Embedded is installed.
        /// </summary>
        VER_SUITE_EMBEDDEDNT = 0x00000040,

        /// <summary>
        ///     Windows Vista Home Premium, Windows Vista Home Basic, or Windows XP Home Edition is installed.
        /// </summary>
        VER_SUITE_PERSONAL = 0x00000200,

        /// <summary>
        ///     Remote Desktop is supported, but only one interactive session is supported. This value is set unless the system is
        ///     running in application server mode.
        /// </summary>
        VER_SUITE_SINGLEUSERTS = 0x00000100,

        /// <summary>
        ///     Microsoft Small Business Server was once installed on the system, but may have been upgraded to another version of
        ///     Windows.
        ///     Refer to the Remarks section for more information about this bit flag.
        /// </summary>
        VER_SUITE_SMALLBUSINESS = 0x00000001,

        /// <summary>
        ///     Microsoft Small Business Server is installed with the restrictive client license in force. Refer to the Remarks
        ///     section for more information about this bit flag.
        /// </summary>
        VER_SUITE_SMALLBUSINESS_RESTRICTED = 0x00000020,

        /// <summary>
        ///     Windows Storage Server 2003 R2 or Windows Storage Server 2003is installed.
        /// </summary>
        VER_SUITE_STORAGE_SERVER = 0x00002000,

        /// <summary>
        ///     Terminal Services is installed. This value is always set.
        ///     If VER_SUITE_TERMINAL is set but VER_SUITE_SINGLEUSERTS is not set, the system is running in application server
        ///     mode.
        /// </summary>
        VER_SUITE_TERMINAL = 0x00000010,

        /// <summary>
        ///     Windows Home Server is installed.
        /// </summary>
        VER_SUITE_WH_SERVER = 0x00008000

        //VER_SUITE_MULTIUSERTS = 0x00020000
    }

    // Taken from https://github.com/pruggitorg/detect-windows-version
    internal enum NTSTATUS : uint
    {
        /// <summary>
        ///     The operation completed successfully.
        /// </summary>
        STATUS_SUCCESS = 0x00000000
    }

    // Taken from https://github.com/pruggitorg/detect-windows-version
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct OSVERSIONINFOEX
    {
        // The OSVersionInfoSize field must be set to Marshal.SizeOf(typeof(OSVERSIONINFOEX))
        public int OSVersionInfoSize;
        public int MajorVersion;
        public int MinorVersion;
        public int BuildNumber;
        public int PlatformId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string CSDVersion;

        public ushort ServicePackMajor;
        public ushort ServicePackMinor;
        public SuiteMask SuiteMask;
        public ProductType ProductType;
        public byte Reserved;
    }

    internal static class Win32APIProvider
    {
        private const string NTDLL = "ntdll.dll";

        [SecurityCritical]
        [DllImport(NTDLL, EntryPoint = "RtlGetVersion", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern NTSTATUS ntdll_RtlGetVersion(ref OSVERSIONINFOEX versionInfo);
    }

    #endregion

    internal class WindowsInformationProvider : InformationProvider
    {
        private string currentUsbDeviceId = "";
        private int counter = 0;
        
        public void GatherInformation(ref MachineInformation information)
        {
            var win10 = false;

            try
            {
                var osVersionInfoEx = new OSVERSIONINFOEX();
                var success = Win32APIProvider.ntdll_RtlGetVersion(ref osVersionInfoEx);

                if (success == NTSTATUS.STATUS_SUCCESS)
                {
                    if (osVersionInfoEx.MajorVersion >= 10)
                    {
                        win10 = true;
                    }
                }
            }
            catch
            {
                // Intentionally left blank
            }

            try
            {
                GatherWin32ProcessorInformation(ref information, win10);
            }
            catch
            {
                // Intentionally left blank
            }

            try
            {
                GatherWin32PhysicalMemory(ref information, win10);
            }
            catch
            {
                // Intentionally left blank
            }

            try
            {
                GatherWin32Bios(ref information, win10);
            }
            catch
            {
                // Intentionally left blank
            }

            try
            {
                GatherWin32BaseBoard(ref information, win10);
            }
            catch
            {
                // Intentionally left blank
            }

            try
            {
                GatherWin32DiskDrive(ref information, win10);
            }
            catch
            {
                // Intentionally left blank
            }

            try
            {
                GatherWin32VideoController(ref information, win10);
            }
            catch
            {
                // Intentionally left blank
            }

            try
            {
                GatherWmiMonitorId(ref information, win10);
            }
            catch
            {
                // Intentionally left blank
            }

            try
            {
                GatherPnpDevices(ref information, win10);
            }
            catch
            {
                // Intentionally left blank
            }
        }

        public bool Available(MachineInformation information)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        public void PostProviderUpdateInformation(ref MachineInformation information)
        {
            // Intentionally left blank
        }

        private void GatherPnpDevices(ref MachineInformation information, bool win10)
        {
            if (!win10)
            {
                return;
            }

            var usbDeviceIds = new List<string>();

            var mos = new ManagementObjectSearcher("select PNPDeviceID from Win32_PnPEntity");

            foreach (var managementBaseObject in mos.Get())
            {
                if (managementBaseObject?.Properties == null || managementBaseObject.Properties.Count == 0)
                {
                    continue;
                }

                foreach (var propertyData in managementBaseObject.Properties)
                {
                    if (propertyData?.Value == null)
                    {
                        continue;
                    }

                    if (propertyData.Name == "PNPDeviceID")
                    {
                        usbDeviceIds.Add(propertyData.Value.ToString());
                    }
                }
            }

            var workingSet = usbDeviceIds.Where(deviceId => deviceId.StartsWith("USB"))
                .Select(deviceId => deviceId.Split('\\')).Where(parts => !parts[1].Contains("HUB"))
                .GroupBy(parts => parts[2]).Select(grp => grp.First())
                .Select(grp => string.Join("\\", grp)).ToList();

            foreach (var usbDeviceId in workingSet)
            {
                Console.WriteLine(usbDeviceId);
            }

            using var p = Util.StartProcess("powershell.exe", "-noexit -nologo");
            var hasLines = false;
            var dataReceived = false;
            var data = new Dictionary<string, string[]>();
            p.OutputDataReceived += (sender, args) =>
            {
                if (args is null || args.Data is null)
                {
                    dataReceived = true;
                    return;
                }
                if (hasLines)
                {
                    Console.WriteLine(args.Data);
                    
                    if (!data.ContainsKey(currentUsbDeviceId))
                    {
                        data.Add(currentUsbDeviceId, new string[4]);
                    }

                    data[currentUsbDeviceId][counter] = args.Data;
                    counter++;
                    hasLines = false;
                    dataReceived = true;
                }
                else if (args.Data.StartsWith("-"))
                {
                    hasLines = true;
                }
                else if (string.IsNullOrWhiteSpace(args.Data))
                {
                    dataReceived = true;
                    hasLines = false;
                }
            };
            p.Start();
            p.BeginOutputReadLine();

            foreach (var usbDeviceId in workingSet)
            {
                currentUsbDeviceId = usbDeviceId;
                counter = 0;
                p.StandardInput.WriteLine(
                    $"Get-PnPDeviceProperty -InstanceId '{usbDeviceId}' -KeyName 'DEVPKEY_Device_BusReportedDeviceDesc' | select Data");
                while (!dataReceived)
                {
                    Thread.Sleep(10);
                }

                dataReceived = false;

                p.StandardInput.WriteLine(
                    $"Get-PnPDeviceProperty -InstanceId '{usbDeviceId}' -KeyName 'DEVPKEY_Device_DriverDesc' | select Data");
                while (!dataReceived)
                {
                    Thread.Sleep(10);
                }

                dataReceived = false;

                p.StandardInput.WriteLine(
                    $"Get-PnPDeviceProperty -InstanceId '{usbDeviceId}' -KeyName 'DEVPKEY_Device_DriverDate' | select Data");
                while (!dataReceived)
                {
                    Thread.Sleep(10);
                }

                dataReceived = false;
                
                p.StandardInput.WriteLine(
                    $"Get-PnPDeviceProperty -InstanceId '{usbDeviceId}' -KeyName 'DEVPKEY_Device_DriverVersion' | select Data");
                while (!dataReceived)
                {
                    Thread.Sleep(10);
                }

                dataReceived = false;
            }

            p.Kill();

            foreach (var stringse in data)
            {
                Console.WriteLine($"{stringse.Key}: {string.Join(", ", stringse.Value)}");
            }

            // using var p = Util.StartProcess("powershell.exe", "Get-PnPDevice | where InstanceId -like \"USB*\" | ForEach-Object { Get-PnPDeviceProperty -InstanceId $_.InstanceId -KeyName 'DEVPKEY_Device_BusReportedDeviceDesc' | select KeyName, Type, Data } | where Type -ne 'Empty'");
            // p.Start();
            // var i = 0;
            // p.OutputDataReceived += (sender, args) =>
            // {
            //     if (i < 20)
            //     {
            //         Console.WriteLine(args.Data);
            //         i++;
            //     }
            //     else
            //     {
            //         p.Kill();
            //     }
            // };
            // p.BeginOutputReadLine();
            // p.WaitForExit(10000);
        }

        private void GatherWin32BaseBoard(ref MachineInformation information, bool win10)
        {
            var mos = new ManagementObjectSearcher("select Product,Manufacturer,Version from Win32_BaseBoard");

            foreach (var managementBaseObject in mos.Get())
            {
                if (managementBaseObject?.Properties == null || managementBaseObject.Properties.Count == 0)
                {
                    continue;
                }

                foreach (var propertyData in managementBaseObject.Properties)
                {
                    if (propertyData?.Value == null)
                    {
                        continue;
                    }

                    switch (propertyData.Name)
                    {
                        case "Product":
                        {
                            information.SmBios.BoardName = propertyData.Value.ToString();

                            break;
                        }

                        case "Manufacturer":
                        {
                            information.SmBios.BoardVendor = propertyData.Value.ToString();

                            break;
                        }

                        case "Version":
                        {
                            information.SmBios.BoardVersion = propertyData.Value.ToString();

                            break;
                        }
                    }
                }
            }
        }

        private void GatherWin32DiskDrive(ref MachineInformation information, bool win10)
        {
            var mos = new ManagementObjectSearcher("select Model,Size,Caption from Win32_DiskDrive");
            var disks = new List<Disk>();

            foreach (var managementBaseObject in mos.Get())
            {
                var disk = new Disk();

                foreach (var propertyData in managementBaseObject.Properties)
                {
                    switch (propertyData.Name)
                    {
                        case "Model":
                        {
                            disk.Model = propertyData.Value.ToString();
                            break;
                        }
                        case "Size":
                        {
                            disk.Capacity = ulong.Parse(propertyData.Value.ToString());
                            disk.CapacityHRF = Util.FormatBytes(disk.Capacity);
                            break;
                        }
                        case "Caption":
                        {
                            disk.Caption = propertyData.Value.ToString();
                            break;
                        }
                    }
                }

                disks.Add(disk);
            }

            information.Disks = disks.AsReadOnly();
        }

        private void GatherWin32VideoController(ref MachineInformation information, bool win10)
        {
            var mos = new ManagementObjectSearcher(
                "select AdapterCompatibility,Caption,Description,DriverDate,DriverVersion,Name,Status from Win32_VideoController");
            var gpus = new List<GPU>();

            foreach (var managementBaseObject in mos.Get())
            {
                var gpu = new GPU();

                foreach (var propertyData in managementBaseObject.Properties)
                {
                    switch (propertyData.Name)
                    {
                        case "AdapterCompatibility":
                        {
                            gpu.Vendor = propertyData.Value.ToString();

                            break;
                        }
                        case "Caption":
                        {
                            gpu.Caption = propertyData.Value.ToString();

                            break;
                        }
                        case "DriverDate":
                        {
                            gpu.DriverDate = propertyData.Value.ToString();

                            break;
                        }
                        case "DriverVersion":
                        {
                            gpu.DriverVersion = propertyData.Value.ToString();

                            break;
                        }
                        case "Description":
                        {
                            gpu.Description = propertyData.Value.ToString();

                            break;
                        }
                        case "Name":
                        {
                            gpu.Name = propertyData.Value.ToString();

                            break;
                        }
                        case "Status":
                        {
                            gpu.Status = propertyData.Value.ToString();

                            break;
                        }
                    }
                }

                gpus.Add(gpu);
            }

            if (information.Gpus.Count == 0)
            {
                information.Gpus = gpus.AsReadOnly();
            }
            else
            {
                foreach (var gpu in information.Gpus)
                {
                    var wmiGpu = gpus.FirstOrDefault(g => g.Name == gpu.Name);

                    if (wmiGpu is not null)
                    {
                        // Currently only DriverDate, Status and Description cannot be queried via Vulkan
                        gpu.DriverDate = wmiGpu.DriverDate;
                        gpu.Status = wmiGpu.Status;
                        gpu.Description = wmiGpu.Description;
                    }
                }
            }
        }

        private void GatherWmiMonitorId(ref MachineInformation information, bool win10)
        {
            var mos = new ManagementObjectSearcher("root\\wmi",
                "select ManufacturerName,UserFriendlyName from WmiMonitorID");
            var displays = new List<Display>();

            foreach (var managementBaseObject in mos.Get())
            {
                try
                {
                    var display = new Display();

                    foreach (var propertyData in managementBaseObject.Properties)
                    {
                        switch (propertyData.Name)
                        {
                            case "ManufacturerName":
                            {
                                display.Manufacturer = string.Join("", ((IEnumerable<ushort>) propertyData.Value)
                                    .Select(u => char.ConvertFromUtf32(u)).Where(s => s != "\u0000").ToList());
                                break;
                            }
                            case "UserFriendlyName":
                            {
                                display.Name = string.Join("", ((IEnumerable<ushort>) propertyData.Value)
                                    .Select(u => char.ConvertFromUtf32(u)).Where(s => s != "\u0000").ToList());
                                break;
                            }
                        }
                    }

                    displays.Add(display);
                }
                catch
                {
                    // Intentionally left blank
                }
            }

            information.Displays = displays.AsReadOnly();
        }

        private void GatherWin32ProcessorInformation(ref MachineInformation information, bool win10)
        {
            ManagementObjectSearcher mos;

            if (win10)
            {
                mos = new ManagementObjectSearcher(
                    "select Name,NumberOfEnabledCore,NumberOfLogicalProcessors,SocketDesignation,MaxClockSpeed from Win32_Processor");
            }
            else
            {
                mos = new ManagementObjectSearcher(
                    "select Name,NumberOfLogicalProcessors,SocketDesignation,MaxClockSpeed from Win32_Processor");
            }

            foreach (var managementBaseObject in mos.Get())
            {
                if (managementBaseObject?.Properties == null || managementBaseObject.Properties.Count == 0)
                {
                    continue;
                }

                foreach (var propertyData in managementBaseObject.Properties)
                {
                    if (propertyData?.Value == null || propertyData.Name == null)
                    {
                        continue;
                    }

                    switch (propertyData.Name)
                    {
                        case "Name":
                        {
                            if (information.Cpu.Name == default || information.Cpu.Name == information.Cpu.Caption)
                            {
                                information.Cpu.Name = propertyData.Value.ToString().Trim();
                            }

                            break;
                        }

                        // MIND THE SSSSSSSS
                        case "NumberOfEnabledCore":
                        {
                            var val = uint.Parse(propertyData.Value.ToString());

                            // Safety check
                            if (information.Cpu.PhysicalCores == default ||
                                information.Cpu.PhysicalCores == information.Cpu.LogicalCores ||
                                val != 0 && val != information.Cpu.PhysicalCores)
                            {
                                information.Cpu.PhysicalCores = val;
                            }

                            break;
                        }

                        case "NumberOfLogicalProcessors":
                        {
                            var val = uint.Parse(propertyData.Value.ToString());

                            if (information.Cpu.LogicalCores == default ||
                                val != 0 && val != information.Cpu.LogicalCores)
                            {
                                information.Cpu.LogicalCores = val;
                            }

                            break;
                        }

                        case "SocketDesignation":
                        {
                            if (information.Cpu.Socket == default)
                            {
                                information.Cpu.Socket = propertyData.Value.ToString().Trim();
                            }

                            break;
                        }

                        case "MaxClockSpeed":
                        {
                            if (information.Cpu.NormalClockSpeed == default)
                            {
                                information.Cpu.NormalClockSpeed = uint.Parse(propertyData.Value.ToString());
                            }

                            break;
                        }
                    }
                }
            }
        }

        private void GatherWin32PhysicalMemory(ref MachineInformation information, bool win10)
        {
            ManagementObjectSearcher mos;
            var ramSticks = new List<RAM>();

            if (win10)
            {
                mos = new ManagementObjectSearcher(
                    "select ConfiguredClockSpeed,Manufacturer,Capacity,DeviceLocator,PartNumber,FormFactor from Win32_PhysicalMemory");
            }
            else
            {
                mos = new ManagementObjectSearcher(
                    "select Manufacturer,Capacity,DeviceLocator,PartNumber,FormFactor from Win32_PhysicalMemory");
            }

            // There is currently no other way to gather RAM information so we don't need to check if it's already set
            foreach (var managementBaseObject in mos.Get())
            {
                if (managementBaseObject?.Properties == null || managementBaseObject.Properties.Count == 0)
                {
                    continue;
                }

                var ram = new RAM();

                foreach (var propertyData in managementBaseObject.Properties)
                {
                    if (propertyData?.Value == null)
                    {
                        continue;
                    }

                    switch (propertyData.Name)
                    {
                        case "ConfiguredClockSpeed":
                        {
                            ram.Speed = uint.Parse(propertyData.Value.ToString());

                            break;
                        }

                        case "Manufacturer":
                        {
                            ram.Manufacturer = propertyData.Value.ToString();

                            break;
                        }

                        case "Capacity":
                        {
                            ram.Capacity += ulong.Parse(propertyData.Value.ToString());

                            break;
                        }

                        case "DeviceLocator":
                        {
                            ram.Name = propertyData.Value.ToString();

                            break;
                        }

                        case "PartNumber":
                        {
                            ram.PartNumber = propertyData.Value.ToString();

                            break;
                        }

                        case "FormFactor":
                        {
                            ram.FormFactor = (RAM.FormFactors) Enum.Parse(
                                typeof(RAM.FormFactors), propertyData.Value.ToString());

                            break;
                        }
                    }
                }

                ram.CapacityHRF = Util.FormatBytes(ram.Capacity);

                ramSticks.Add(ram);
            }

            information.RAMSticks = ramSticks.AsReadOnly();
        }

        private void GatherWin32Bios(ref MachineInformation information, bool win10)
        {
            var mos = new ManagementObjectSearcher("select Name,Manufacturer,Version from Win32_BIOS");

            foreach (var managementBaseObject in mos.Get())
            {
                if (managementBaseObject?.Properties == null || managementBaseObject.Properties.Count == 0)
                {
                    continue;
                }

                foreach (var propertyData in managementBaseObject.Properties)
                {
                    if (propertyData?.Value == null)
                    {
                        continue;
                    }

                    switch (propertyData.Name)
                    {
                        case "Name":
                        {
                            information.SmBios.BIOSVersion = propertyData.Value.ToString();

                            break;
                        }

                        case "Manufacturer":
                        {
                            information.SmBios.BIOSVendor = propertyData.Value.ToString();

                            break;
                        }

                        case "Version":
                        {
                            information.SmBios.BIOSCodename = propertyData.Value.ToString();

                            break;
                        }
                    }
                }
            }
        }
    }
}