#region using

using System;
using HardwareInformation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace SimpleGatheringTest
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine(JsonConvert.SerializeObject(MachineInformationGatherer.GatherInformation(),
				new StringEnumConverter()));
		}
	}
}