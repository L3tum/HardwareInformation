#region using

using System;
using HardwareInformation.Information;
using HardwareInformation.Information.Cpu;
using HardwareInformation.Information.Gpu;
using Reinforced.Typings.Fluent;
using Vendors = HardwareInformation.Information.Gpu.Vendors;

#endregion

namespace HardwareInformation.Typings
{
    public static class ReinforcedTypingsConfiguration
    {
        public static void Configure(ConfigurationBuilder builder)
        {
            builder.Global(configurationBuilder =>
            {
                configurationBuilder.UseModules();
                configurationBuilder.GenerateDocumentation();
            });

            builder.ExportAsClasses(new[]
            {
                typeof(MachineInformation),
                typeof(CPU),
                typeof(Disk),
                typeof(Display),
                typeof(GPU),
                typeof(RAM),
                typeof(SMBios),
                typeof(Cache),
                typeof(Core),
                typeof(AMDFeatureFlags),
                typeof(IntelFeatureFlags),
                typeof(USBDevice)
            }, configBuilder =>
            {
                configBuilder.WithAllFields().WithAllMethods()
                    .WithAllProperties();
            });

            builder.ExportAsClass<Vendors>().WithAllFields().WithAllMethods().WithAllProperties()
                .OverrideName("CPUVendors");
            builder.ExportAsClass<Information.Gpu.Vendors>().WithAllFields().WithAllMethods().WithAllProperties()
                .OverrideName("GPUVendors");

            builder.ExportAsClass<OperatingSystem>().WithFields(field =>
                field.Name is "Platform" or "ServicePack" or "Version" or "VersionString");
            builder.ExportAsClass<Version>().WithFields(field =>
                field.Name is "Build" or "Major" or "MajorRevision" or "Minor" or "MinorRevision" or "Revision");
            builder.ExportAsClass<DateTime>().WithFields(field =>
                field.Name is "Year" or "Month" or "Day" or "Hour" or "Minute" or "Second" or "Millisecond");

            builder.ExportAsEnums(new[]
            {
                typeof(MachineInformation.Platforms),
                typeof(CPU.FeatureFlagECX),
                typeof(CPU.FeatureFlagEDX),
                typeof(CPU.ExtendedFeatureFlagsF7EBX),
                typeof(CPU.ExtendedFeatureFlagsF7ECX),
                typeof(CPU.ExtendedFeatureFlagsF7EDX),
                typeof(CPU.ProcessorType),
                typeof(RAM.FormFactors),
                typeof(Cache.CacheLevel),
                typeof(Cache.CacheType),
                typeof(AMDFeatureFlags.ExtendedFeatureFlagsF81ECX),
                typeof(AMDFeatureFlags.ExtendedFeatureFlagsF81EDX),
                typeof(AMDFeatureFlags.FeatureFlagsAPM),
                typeof(AMDFeatureFlags.FeatureFlagsSVM),
                typeof(IntelFeatureFlags.ExtendedFeatureFlagsF81ECX),
                typeof(IntelFeatureFlags.ExtendedFeatureFlagsF81EDX),
                typeof(IntelFeatureFlags.FeatureFlagsAPM),
                typeof(IntelFeatureFlags.TPMFeatureFlagsEAX),
                typeof(DeviceType)
            });
        }
    }
}