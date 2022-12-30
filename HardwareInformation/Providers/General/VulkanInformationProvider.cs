#region using

using System.Linq;
using HardwareInformation.Information;
using HardwareInformation.Information.Gpu;
using Microsoft.Extensions.Logging;
using Vulkan;

#endregion

namespace HardwareInformation.Providers.General;

public class VulkanInformationProvider : InformationProvider
{
    private Instance instance;

    public override void GatherInformation(MachineInformation information)
    {
        foreach (var device in instance.EnumeratePhysicalDevices())
        {
            var props = device.GetProperties();

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

            if (information.Gpus.Count > 0)
            {
                foreach (var informationGpu in information.Gpus)
                {
                    if (informationGpu.Name == props.DeviceName)
                    {
                        informationGpu.Type = GetDeviceTypeFromVulkanDeviceType(props.DeviceType);
                        informationGpu.SupportedVulkanApiVersion = Version.ToString(props.ApiVersion);
                        informationGpu.AvailableVideoMemory = totalAvailableVram;
                        informationGpu.AvailableVideoMemoryHRF = Util.FormatBytes(totalAvailableVram);
                    }
                }
            }
            else
            {
                var gpu = new GPU
                {
                    Name = props.DeviceName,
                    Vendor = GetVendorNameFromVendorId(props.VendorId),
                    VendorID = props.VendorId.ToString(),
                    Caption = props.DeviceName,
                    DriverVersion = Version.ToString(props.DriverVersion),
                    Type = GetDeviceTypeFromVulkanDeviceType(props.DeviceType),
                    SupportedVulkanApiVersion = Version.ToString(props.ApiVersion),
                    AvailableVideoMemory = totalAvailableVram,
                    AvailableVideoMemoryHRF = Util.FormatBytes(totalAvailableVram)
                };

                information.Gpus = information.Gpus.Append(gpu).ToList().AsReadOnly();
            }
        }
    }

    public override bool Available(MachineInformation information)
    {
        try
        {
            if (!DynamicLibraryChecker.CheckLibrary("vulkan-1"))
            {
                MachineInformationGatherer.Logger.LogWarning("Vulkan shared library is not available");

                return false;
            }

            instance = new Instance();

            return instance.EnumeratePhysicalDevices().Length > 0;
        }
        catch
        {
            return false;
        }
    }

    public override void PostProviderUpdateInformation(MachineInformation information)
    {
        instance?.Dispose();
    }

    private static DeviceType GetDeviceTypeFromVulkanDeviceType(PhysicalDeviceType type)
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

    internal static string GetVendorNameFromVendorId(uint vendorId)
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