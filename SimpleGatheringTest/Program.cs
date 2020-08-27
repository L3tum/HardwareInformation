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
			var information = MachineInformationGatherer.GatherInformation();
			string json = JsonConvert.SerializeObject(information, Formatting.Indented, new StringEnumConverter());
			Console.WriteLine(json);
		}
	}
}