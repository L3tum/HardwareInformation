#region using

using System;
using HardwareInformation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace SimpleGatheringTest
{
    internal class Program
    {
        private static readonly ILoggerFactory LoggerFactory =
            Microsoft.Extensions.Logging.LoggerFactory.Create(builder => { builder.AddConsole(); });

        private static readonly ILogger<MachineInformation> Logger = LoggerFactory.CreateLogger<MachineInformation>();

        private static void Main(string[] args)
        {
            Console.WriteLine(
                JsonConvert.SerializeObject(
                    MachineInformationGatherer.GatherInformation(logger: Logger),
                    new StringEnumConverter()
                )
            );
        }
    }
}