#region using

using HardwareInformation.Information;
using HardwareInformation.Information.Cpu;
using Reinforced.Typings.Fluent;

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
				typeof(Vendors),
				typeof(AMDFeatureFlags),
				typeof(IntelFeatureFlags)
			}, configBuilder =>
			{
				configBuilder.WithAllFields().WithAllMethods()
					.WithAllProperties();
			});

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
				typeof(IntelFeatureFlags.TPMFeatureFlagsEAX)
			});
		}
	}
}