#region using

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace HardwareInformation.Providers
{
	internal class IntelInformationProvider : InformationProvider
	{
		public void GatherInformation(ref MachineInformation information)
		{
			if (information.Cpu.MaxCpuIdFeatureLevel >= 6)
			{
				Opcode.Cpuid(out var result, 6, 0);

				information.Cpu.IntelFeatureFlags.TPMFeatureFlags =
					(MachineInformation.IntelFeatureFlags.TPMFeatureFlagsEAX) result.eax;
			}

			if (information.Cpu.MaxCpuIdFeatureLevel >= 11)
			{
				var threads = new List<Task>();
				var cores = 0u;

				for (int i = 0; i < information.Cpu.LogicalCores; i++)
				{
					threads.Add(Util.RunAffinity(1uL << i, () =>
					{
						Opcode.Cpuid(out var result, 11, 0);

						if ((result.edx & 0b1) != 1)
						{
							cores++;
						}
					}));
				}

				Task.WaitAll(threads.ToArray());
				information.Cpu.PhysicalCores = cores;
			}

			if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 1)
			{
				Opcode.Cpuid(out var result, 0x80000001, 0);

				information.Cpu.IntelFeatureFlags.ExtendedFeatureFlagsF81One =
					(MachineInformation.IntelFeatureFlags.ExtendedFeatureFlagsF81ECX) result.ecx;
				information.Cpu.IntelFeatureFlags.ExtendedFeatureFlagsF81Two =
					(MachineInformation.IntelFeatureFlags.ExtendedFeatureFlagsF81EDX) result.edx;
			}

			if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 4)
			{
				Opcode.Cpuid(out var partOne, 0x80000002, 0);
				Opcode.Cpuid(out var partTwo, 0x80000003, 0);
				Opcode.Cpuid(out var partThree, 0x80000004, 0);

				var results = new[] {partOne, partTwo, partThree};
				var sb = new StringBuilder();

				foreach (var res in results)
				{
					sb.Append(string.Format("{0}{1}{2}{3}",
						string.Join("", $"{res.eax:X}".HexStringToString().Reverse()),
						string.Join("", $"{res.ebx:X}".HexStringToString().Reverse()),
						string.Join("", $"{res.ecx:X}".HexStringToString().Reverse()),
						string.Join("", $"{res.edx:X}".HexStringToString().Reverse())));
				}

				information.Cpu.Name = sb.ToString();
			}

			if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 7)
			{
				Opcode.Cpuid(out var result, 0x80000001, 0);

				information.Cpu.IntelFeatureFlags.FeatureFlagsApm =
					(MachineInformation.IntelFeatureFlags.FeatureFlagsAPM) result.edx;
			}

			GatherPerCoreInformation(ref information);
		}
		 
		public bool Available(MachineInformation information)
		{
			return information.Cpu.Vendor == MachineInformation.Vendors.Intel &&
			       (RuntimeInformation.ProcessArchitecture == Architecture.X86 ||
			        RuntimeInformation.ProcessArchitecture == Architecture.X64);
		}

		private void GatherPerCoreInformation(ref MachineInformation information)
		{
			var threads = new List<Task>();
			var caches = information.Cpu.Caches;
			var supportsCacheTopologyExtensions = information.Cpu.MaxCpuIdFeatureLevel >= 4;

			if (!supportsCacheTopologyExtensions)
			{
				return;
			}

			foreach (var core in information.Cpu.Cores)
			{
				threads.Add(Util.RunAffinity(1uL << (int)core.Number, () =>
				{
					var ecx = 0u;

						while (true)
						{
							Opcode.Cpuid(out var result, 4, ecx);

							var type = (MachineInformation.Cache.CacheType)(result.eax & 0xF);

							// Null, no more caches
							if (type == MachineInformation.Cache.CacheType.NONE)
							{
								break;
							}

							var cache = new MachineInformation.Cache
							{
								CoresPerCache = ((result.eax & 0x3FFC000) >> 14) + 1u,
								Level = (MachineInformation.Cache.CacheLevel)((result.eax & 0xF0) >> 5),
								Type = type,
								LineSize = (result.ebx & 0xFFF) + 1u,
								WBINVD = (result.edx & 0b1) == 0,
								Sets = (result.eax & 0b1000000000) == 1 ? 0x1 : result.ecx + 1,
								Partitions = ((result.ebx & 0x3FF000) >> 12) + 1,
								Associativity = (result.eax & 0b1000000000) == 1
									? 0xffffffff
									: ((result.ebx & 0x7FE00000) >> 22) + 1
							};

							cache.Capacity = cache.LineSize * cache.Associativity * cache.Partitions * cache.Sets;
							cache.CapacityHRF = Util.FormatBytes(cache.Capacity);

							lock (caches)
							{
								var orig = caches.FirstOrDefault(c => c.Equals(cache));

								if (orig == null)
								{
									caches.Add(cache);
								}
								else
								{
									orig.TimesPresent++;
								}
							}

							ecx++;
						}
				}));
			}

			Task.WaitAll(threads.ToArray());

			if (supportsCacheTopologyExtensions)
			{
				foreach (var cache in caches)
				{
					var threadsPerCore = information.Cpu.LogicalCores / information.Cpu.PhysicalCores;

					cache.TimesPresent++;
					cache.TimesPresent /= cache.CoresPerCache;
					cache.CoresPerCache /= threadsPerCore;
				}
			}

			information.Cpu.Caches = caches;
		}
	}
}