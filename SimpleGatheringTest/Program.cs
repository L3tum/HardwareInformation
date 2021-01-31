#region using

using System;
using System.Collections.Generic;
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

        private static readonly ILogger Logger = LoggerFactory.CreateLogger("MachineInformation");

        private static List<Exception> exceptions = new List<Exception>();

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;
            Console.WriteLine(
                JsonConvert.SerializeObject(
                    MachineInformationGatherer.GatherInformation(logger: Logger),
                    new StringEnumConverter()
                )
            );

            foreach (var exception in exceptions)
            {
                Logger.LogCritical(exception, "First Chance");
            }

            if (exceptions.Count > 0)
            {
                throw new Exception("Fuck");
            }
        }

        private static void CurrentDomainOnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            exceptions.Add(e.Exception);
        }
    }
}