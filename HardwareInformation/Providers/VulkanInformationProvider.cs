using System.Collections.Generic;
using System.Linq;
using HardwareInformation.Information;
using HardwareInformation.Information.Gpu;
using Vulkan;

namespace HardwareInformation.Providers
{
    internal class VulkanInformationProvider : InformationProvider
    {
        public void GatherInformation(ref MachineInformation information)
        {
            using var instance = new Instance();
            var gpus = new List<GPU>();

            foreach (var device in instance.EnumeratePhysicalDevices())
            {
                var props = device.GetProperties();
                var gpu = new GPU
                {
                    Name = props.DeviceName,
                    Vendor = GetVendorNameFromVendorId(props.VendorId),
                    Caption = props.DeviceName,
                    DriverVersion = Version.ToString(props.DriverVersion),
                    Type = GetDeviceTypeFromVulkanDeviceType(props.DeviceType),
                    SupportedVulkanApiVersion = Version.ToString(props.ApiVersion)
                };

                var memoryProperties = device.GetMemoryProperties();

                var deviceLocalIds = (from memoryType in memoryProperties.MemoryTypes
                    where (memoryType.PropertyFlags & MemoryPropertyFlags.DeviceLocal) != 0
                    select memoryType.HeapIndex).ToList();
                var totalAvailableVram = 0uL;

                for (var index = 0u; index < memoryProperties.MemoryHeaps.Length; index++)
                {
                    if (deviceLocalIds.Contains(index))
                    {
                        totalAvailableVram += memoryProperties.MemoryHeaps[index].Size;
                    }
                }

                gpu.AvailableVideoMemory = totalAvailableVram;
                gpu.AvailableVideoMemoryHRF = Util.FormatBytes(totalAvailableVram);

                gpus.Add(gpu);
            }

            information.Gpus = gpus.AsReadOnly();
        }

        public bool Available(MachineInformation information)
        {
            using var instance = new Instance();

            return instance.EnumeratePhysicalDevices().Length > 0;
        }

        public void PostProviderUpdateInformation(ref MachineInformation information)
        {
            // Intentionally left blank
        }

        private DeviceType GetDeviceTypeFromVulkanDeviceType(PhysicalDeviceType type)
        {
            return type switch
            {
                PhysicalDeviceType.Cpu => DeviceType.CPU,
                PhysicalDeviceType.DiscreteGpu => DeviceType.DISCRETE,
                PhysicalDeviceType.IntegratedGpu => DeviceType.INTEGRATED,
                PhysicalDeviceType.VirtualGpu => DeviceType.VIRTUAL,
                _ => DeviceType.UNKNOWN
            };
        }

        private string GetVendorNameFromVendorId(uint vendorId)
        {
            return vendorId switch
            {
                0x1002 => "Advanced Micro Devices, Inc.", // Same as AdapterCompatibility on Windows
                0x1010 => "ImgTech", // ???
                0x8086 => "Intel Corporation", // Same as AMD
                0x10DE => "Nvidia Corporation", // I *think* same as AMD
                0x13B5 => "ARM", // ???
                0x5143 => "Qualcomm", // ???
                _ => "Unknown"
            };
        }
    }
}