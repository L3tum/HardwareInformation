#region using

using System;
using System.Runtime.ExceptionServices;
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
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;
            Console.WriteLine(
                JsonConvert.SerializeObject(
                    MachineInformationGatherer.GatherInformation(logger: Logger),
                    new StringEnumConverter()
                )
            );
        }

        private static void CurrentDomainOnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
	        Logger.LogError(e.Exception, "First Chance");
        }
    }
}