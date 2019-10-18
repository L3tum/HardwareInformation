namespace HardwareInformation.Providers
{
	internal interface InformationProvider
	{
		/// <summary>
		///     Gather the information
		/// </summary>
		/// <param name="information"></param>
		void GatherInformation(ref MachineInformation information);

		/// <summary>
		///     Check if this provider or the underlying method is available on this platform
		/// </summary>
		/// <param name="information"></param>
		/// <returns></returns>
		bool Available(MachineInformation information);

		/// <summary>
		///     Update information that may depend on other providers (OS providers for example)
		/// </summary>
		/// <param name="information"></param>
		void PostProviderUpdateInformation(ref MachineInformation information);
	}
}