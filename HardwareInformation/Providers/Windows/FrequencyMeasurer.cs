using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using System.Threading;

namespace HardwareInformation.Providers.Windows
{
    /// <summary>
    /// Provides abstractions around using a high-priority background thread for frequency measuring
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class FrequencyMeasurer : IDisposable
    {
        private readonly double baseFrequency;
        private readonly List<double> measurements;
        private readonly Thread measuringThread;
        private readonly CancellationTokenSource measuringThreadTokenSource;

        /// <summary>
        ///     Instantiates a new frequency measurer and starts the background measuring thread
        /// </summary>
        public FrequencyMeasurer()
        {
            measurements = new List<double>();
            measuringThreadTokenSource = new CancellationTokenSource();
            measuringThread = new Thread(() => MeasureCpuFrequency(measuringThreadTokenSource.Token))
            {
                Priority = ThreadPriority.Highest, IsBackground = true
            };
            measuringThread.Start();

            foreach (var obj in
                new ManagementObjectSearcher("SELECT *, Name FROM Win32_Processor").Get())
            {
                baseFrequency = Convert.ToDouble(obj["MaxClockSpeed"]) / 1000;
            }
        }

        /// <summary>
        ///     Lock whether the measurement thread is ready
        /// </summary>
        public bool MeasuringThreadReady { get; private set; }

        /// <summary>
        ///     Dispose of the measurement thread
        /// </summary>
        public void Dispose()
        {
            StopMeasurements();
        }

        /// <summary>
        ///     Stops the measurement thread
        /// </summary>
        public void StopMeasurements()
        {
            measuringThreadTokenSource.Cancel();
            measuringThread.Join();
        }

        /// <summary>
        ///     Clears all measurements
        /// </summary>
        public void ClearMeasurements()
        {
            lock (measurements)
            {
                measurements.Clear();
            }
        }

        /// <summary>
        ///     Return measurements calculated to base, average, highest and lowest frequency
        /// </summary>
        /// <returns></returns>
        public Tuple<int, int, int, int> GetMeasurements()
        {
            lock (measurements)
            {
                var frequencies = measurements.Select(frequency => baseFrequency * frequency / 100).ToList();
                var averageFrequency = frequencies.Average();
                var highestFrequency = frequencies.Max();
                var lowestFrequency = frequencies.Min();

                return Tuple.Create((int) (baseFrequency * 1000), (int) (averageFrequency * 1000),
                    (int) (highestFrequency * 1000), (int) (lowestFrequency * 1000));
            }
        }

        /// <summary>
        ///     Asynchronously measures CPU frequency and returns base, average, highest
        ///     and lowest frequency in MHz
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private void MeasureCpuFrequency(CancellationToken cancellationToken)
        {
            using var cpuCounter =
                new PerformanceCounter("Processor Information", "% Processor Performance", "_Total");
            cpuCounter.NextValue();
            MeasuringThreadReady = true;

            while (!cancellationToken.IsCancellationRequested)
            {
                lock (measurements)
                {
                    measurements.Add(cpuCounter.NextValue());

                    // Keep the list of measurements kinda short
                    if (measurements.Count > 1000)
                    {
                        measurements.RemoveRange(0, 100);
                    }
                }

                Thread.Sleep(500);
            }
        }
    }
}