namespace HardwareInformation.Providers
{
	internal interface InformationProvider
	{
		void GatherInformation(ref MachineInformation information);

		bool Available(MachineInformation information);
	}
}