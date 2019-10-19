#region using

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HardwareInformation.Providers;

#endregion

namespace HardwareInformation
{
	/// <summary>
	///     Main entry class to gather information on the system hardware.
	/// </summary>
	public static class MachineInformationGatherer
	{
		private static readonly InformationProvider[] InformationProviders =
		{
			new AMDInformationProvider(),
			new IntelInformationProvider(),
			new WindowsInformationProvider(),
			new LinuxInformationProvider(),
			new OSXInformationProvider()
		};

		private static MachineInformation information;

		private static bool lastSkipClockspeedTest = true;

		/// <summary>
		///     Gathers lots of information about the running processor.
		///     Currently does NOT support multi-processor setups (e.g. two Intel Xeon CPUs).
		///     For detailed information about the information provided please see the readme.
		/// </summary>
		/// <param name="skipClockspeedTest">
		///     Default true. If false it will run a quick speed test of all cores to determine
		///     maximum frequency.
		/// </param>
		/// <returns></returns>
		public static MachineInformation GatherInformation(bool skipClockspeedTest = true)
		{
			if (information != null && lastSkipClockspeedTest == skipClockspeedTest)
			{
				return information;
			}

			if (RuntimeInformation.ProcessArchitecture == Architecture.X86 ||
			    RuntimeInformation.ProcessArchitecture == Architecture.X64)
			{
				Opcode.Open();
			}

			lastSkipClockspeedTest = skipClockspeedTest;
			information = new MachineInformation();

			GetCommonCpuInformation();

			GatherCommonPerCoreInformation();

			foreach (var informationProvider in InformationProviders)
			{
				try
				{
					if (informationProvider.Available(information))
					{
						informationProvider.GatherInformation(ref information);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}

			foreach (var cpuCore in information.Cpu.Cores)
			{
				cpuCore.NormalClockSpeed = information.Cpu.NormalClockSpeed;
				cpuCore.MaxClockSpeed = information.Cpu.MaxClockSpeed;
			}

			foreach (var informationProvider in InformationProviders)
			{
				try
				{
					if (informationProvider.Available(information))
					{
						informationProvider.PostProviderUpdateInformation(ref information);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}

			if (!skipClockspeedTest && (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
			                            RuntimeInformation.IsOSPlatform(OSPlatform.Linux)))
			{
				GetCoreSpeeds();
			}

			if (RuntimeInformation.ProcessArchitecture == Architecture.X86 ||
			    RuntimeInformation.ProcessArchitecture == Architecture.X64)
			{
				Opcode.Close();
			}

			return information;
		}

		// TODO: Tests

		private static void GetCommonCpuInformation()
		{
			information.OperatingSystem = Environment.OSVersion;
			information.Platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
				? MachineInformation.Platforms.Windows
				: RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
					? MachineInformation.Platforms.Linux
					: RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
						? MachineInformation.Platforms.OSX
						: MachineInformation.Platforms.Unknown;
			information.Cpu.LogicalCores = (uint) Environment.ProcessorCount;
			information.Cpu.LogicalCoresPerNode = information.Cpu.LogicalCores;
			information.Cpu.Nodes = 1;
			information.Cpu.Architecture = RuntimeInformation.ProcessArchitecture.ToString();
			information.Cpu.Caption = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? default;
			information.Cpu.Name = information.Cpu.Caption;

			for (var i = 0; i < information.Cpu.LogicalCores; i++)
			{
				information.Cpu.Cores.Add(new MachineInformation.Core
				{
					Number = (uint) i
				});
			}

			if (RuntimeInformation.ProcessArchitecture != Architecture.X86 &&
			    RuntimeInformation.ProcessArchitecture != Architecture.X64)
			{
				return;
			}

			Opcode.Cpuid(out var result, 0, 0);

			var vendorString = string.Format("{0}{1}{2}",
				string.Join("", $"{result.ebx:X}".HexStringToString().Reverse()),
				string.Join("", $"{result.edx:X}".HexStringToString().Reverse()),
				string.Join("", $"{result.ecx:X}".HexStringToString().Reverse()));

			information.Cpu.Vendor = vendorString;
			information.Cpu.MaxCpuIdFeatureLevel = result.eax;

			if (information.Cpu.MaxCpuIdFeatureLevel >= 1)
			{
				Opcode.Cpuid(out result, 1, 0);

				information.Cpu.Stepping = result.eax & 0xF;

				var familyId = (result.eax & 0xF00) >> 8;

				if (familyId == 6 || familyId == 15)
				{
					information.Cpu.Model = (((result.eax & 0xF0000) >> 16) << 4) + ((result.eax & 0xF0) >> 4);
				}
				else
				{
					information.Cpu.Model = (result.eax & 0xF0) >> 4;
				}

				if (familyId == 15)
				{
					information.Cpu.Family = ((result.eax & 0xFF00000) >> 20) + familyId;
				}
				else
				{
					information.Cpu.Family = familyId;
				}

				information.Cpu.Type =
					(MachineInformation.CPU.ProcessorType) ((result.eax & 0b11000000000000) >> 12);
				information.Cpu.FeatureFlagsOne = (MachineInformation.CPU.FeatureFlagEDX) result.edx;
				information.Cpu.FeatureFlagsTwo = (MachineInformation.CPU.FeatureFlagECX) result.ecx;
			}

			if (information.Cpu.MaxCpuIdFeatureLevel >= 7)
			{
				Opcode.Cpuid(out result, 7, 0);

				information.Cpu.ExtendedFeatureFlagsF7One =
					(MachineInformation.CPU.ExtendedFeatureFlagsF7EBX) result.ebx;
				information.Cpu.ExtendedFeatureFlagsF7Two =
					(MachineInformation.CPU.ExtendedFeatureFlagsF7ECX) result.ecx;
				information.Cpu.ExtendedFeatureFlagsF7Three =
					(MachineInformation.CPU.ExtendedFeatureFlagsF7EDX) result.edx;
			}

			Opcode.Cpuid(out result, 0x80000000, 0);

			information.Cpu.MaxCpuIdExtendedFeatureLevel = result.eax;
		}

		private static void GatherCommonPerCoreInformation()
		{
			if (RuntimeInformation.ProcessArchitecture != Architecture.X86 &&
			    RuntimeInformation.ProcessArchitecture != Architecture.X64)
			{
				return;
			}

			for (var i = 0; i < information.Cpu.LogicalCores; i++)
			{
				var core = information.Cpu.Cores.First(c => c.Number == i);
				var thread = Util.RunAffinity(1uL << i, () =>
				{
					if (information.Cpu.MaxCpuIdFeatureLevel >= 16)
					{
						Opcode.Cpuid(out var result, 0x16, 0);

						core.ReferenceNormalClockSpeed = result.eax;
						core.ReferenceMaxClockSpeed = result.ebx;
						core.ReferenceBusSpeed = result.ecx;
					}
				});

				thread.Wait();
			}
		}

		private static void GetCoreSpeeds()
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

			for (var i = 0; i < information.Cpu.LogicalCores; i++)
			{
				if (i > 64)
				{
					// Too long for long
					break;
				}

				var core = information.Cpu.Cores.First(c => c.Number == i);

				core.NormalClockSpeed = information.Cpu.NormalClockSpeed;

				using var ct = new CancellationTokenSource();
				PerformanceCounter counter = null;

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					counter =
						new PerformanceCounter("Processor Information", "% Processor Performance", "0," + i);

					counter.NextValue();
				}

				var thread = Util.RunAffinity(1uL << i, () =>
				{
					var g = 0;

					while (!ct.IsCancellationRequested)
					{
						g++;
					}
				});

				Thread.Sleep(1000);

				var value = core.NormalClockSpeed;

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					value = (uint) (counter.NextValue() / 100.0f * value);
					counter.Dispose();
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					try
					{
						// KHz
						var freq = ulong.Parse(
							File.ReadAllText($"/sys/devices/system/cpu/cpu{i}/cpufreq/scaling_cur_freq"));

						value = (uint) (freq / 1000);
					}
					catch (Exception)
					{
						// Abort early since failing once means we'll most likely fail always.
						ct.Cancel();
						break;
					}
				}

				core.MaxClockSpeed = value;
				ct.Cancel();
				thread.Wait();
			}

			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
			information.Cpu.MaxClockSpeed =
				information.Cpu.Cores.Count > 0 ? information.Cpu.Cores.Max(c => c.MaxClockSpeed) : 0;
		}

		/// <summary>
		///     Monitors the frequency of the specified core until the token is cancelled. Reports maximum frequency in
		///     Core.MaxClockSpeed and minimum frequency in Core.NormalClockSpeed.
		/// </summary>
		/// <param name="coreNumber">
		///     0-indexed number of the core to monitor. Includes hyperthreaded cores (e.g. 0 and 1 are the
		///     same physical core)
		/// </param>
		/// <param name="token">Token to stop the monitoring.</param>
		/// <param name="measurementDelay">
		///     Delay in ms for taking the measurement (e.g. wait $measurementDelay, take measurement,
		///     repeat). Values under 1000ms may produce wrong results.
		/// </param>
		/// <returns></returns>
		public static Task<MachineInformation.Core> MonitorCoreFrequencies(int coreNumber,
			CancellationToken token, int measurementDelay = 1000)
		{
			if (coreNumber > 64 || coreNumber < 0)
			{
				throw new ArgumentException("Core Number must be below 65 and above 0!");
			}

			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
			    !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				throw new PlatformNotSupportedException("This method only works on Windows or Linux!");
			}

			return Task.Run(() =>
			{
				var core = new MachineInformation.Core();
				var highestFrequency = 0u;
				var lowestFrequency = 0u;
				PerformanceCounter counter = null;
				var normalFrequency = 0u;

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					counter = new PerformanceCounter("Processor Information", "% Processor Performance",
						"0," + coreNumber);
					counter.NextValue();

					ManagementObject Mo = new ManagementObject($"Win32_Processor.DeviceID='CPU{coreNumber}'");

					normalFrequency = (uint) Mo["MaxClockSpeed"];
				}

				while (!token.IsCancellationRequested)
				{
					Thread.Sleep(measurementDelay);
					var measuredValue = 0u;

					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					{
						measuredValue = (uint) (counter.NextValue() / 100.0f * normalFrequency);
					}
					else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					{
						// KHz
						var freq = ulong.Parse(
							File.ReadAllText($"/sys/devices/system/cpu/cpu{coreNumber}/cpufreq/scaling_cur_freq"));

						measuredValue = (uint) (freq / 1000);
					}

					if (measuredValue > highestFrequency)
					{
						highestFrequency = measuredValue;
					}

					if (measuredValue < lowestFrequency)
					{
						lowestFrequency = measuredValue;
					}
				}

				core.Number = (uint) coreNumber;
				core.MaxClockSpeed = highestFrequency;
				core.NormalClockSpeed = lowestFrequency;

				return core;
			});
		}
	}
}