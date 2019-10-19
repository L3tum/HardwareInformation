#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace HardwareInformation.Providers
{
	internal class AMDInformationProvider : InformationProvider
	{
		public void GatherInformation(ref MachineInformation information)
		{
			GatherFeatureFlags(ref information);

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

			try
			{
				Opcode.Cpuid(out var hammerTime, 0x8FFFFFFF, 0);

				var hammerString = string.Format("{0}{1}{2}{3}", string.Join("", $"{hammerTime.eax:X}".HexStringToString().Reverse()),
					string.Join("", $"{hammerTime.ebx:X}".HexStringToString().Reverse()),
					string.Join("", $"{hammerTime.ecx:X}".HexStringToString().Reverse()),
					string.Join("", $"{hammerTime.edx:X}".HexStringToString().Reverse()));

				if (!string.IsNullOrWhiteSpace(hammerString))
				{
					Console.WriteLine(hammerString);
				}
			}
			catch (Exception)
			{
				// No K7 or K8 :(
			}

			if (information.Cpu.Family >= 0x15 && information.Cpu.MaxCpuIdExtendedFeatureLevel >= 0x1E)
			{
				GatherPhysicalCores(ref information);
			}
			else if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 8)
			{
				Opcode.Cpuid(out var result, 0x80000008, 0);

				information.Cpu.PhysicalCores = (result.ecx & 0xFF) + 1;

				if (information.Cpu.FeatureFlagsOne.HasFlag(MachineInformation.CPU.FeatureFlagEDX.HTT) &&
				    information.Cpu.PhysicalCores == information.Cpu.LogicalCores)
				{
					information.Cpu.PhysicalCores /= 2;
				}
			}

			GatherCacheTopology(ref information);
		}

		public bool Available(MachineInformation information)
		{
			return (information.Cpu.Vendor == MachineInformation.Vendors.AMD ||
			        information.Cpu.Vendor == MachineInformation.Vendors.AMD_LEGACY) &&
			       (RuntimeInformation.ProcessArchitecture == Architecture.X86 ||
			        RuntimeInformation.ProcessArchitecture == Architecture.X64);
		}

		public void PostProviderUpdateInformation(ref MachineInformation information)
		{
			if (information.Cpu.AMDFeatureFlags.ExtendedFeatureFlagsF81One.HasFlag(MachineInformation.AMDFeatureFlags
				.ExtendedFeatureFlagsF81ECX.TOPOEXT) && information.Cpu.PhysicalCores != 0)
			{
				foreach (var cache in information.Cpu.Caches)
				{
					var threadsPerCore = information.Cpu.LogicalCores / information.Cpu.PhysicalCores;

					cache.TimesPresent++;
					cache.TimesPresent /= cache.CoresPerCache;
					cache.CoresPerCache /= threadsPerCore;
				}
			}
		}

		private void GatherCacheTopology(ref MachineInformation information)
		{
			var threads = new List<Task>();
			var caches = information.Cpu.Caches;
			var maxExtendedFeatureLevel = information.Cpu.MaxCpuIdExtendedFeatureLevel;
			var supportsCacheTopologyExtensions =
				information.Cpu.AMDFeatureFlags.ExtendedFeatureFlagsF81One.HasFlag(MachineInformation.AMDFeatureFlags
					.ExtendedFeatureFlagsF81ECX.TOPOEXT);

			foreach (var core in information.Cpu.Cores)
			{
				threads.Add(Util.RunAffinity(1uL << (int) core.Number, () =>
				{
					if (supportsCacheTopologyExtensions)
					{
						var ecx = 0u;

						while (true)
						{
							Opcode.Cpuid(out var result, 0x8000001D, ecx);

							var type = (MachineInformation.Cache.CacheType) (result.eax & 0xF);

							// Null, no more caches
							if (type == MachineInformation.Cache.CacheType.NONE)
							{
								break;
							}

							var cache = new MachineInformation.Cache
							{
								CoresPerCache = ((result.eax & 0x3FFC000) >> 14) + 1u,
								Level = (MachineInformation.Cache.CacheLevel) ((result.eax & 0xF0) >> 5),
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
					}

					if (!supportsCacheTopologyExtensions)
					{
						if (maxExtendedFeatureLevel >= 5)
						{
							Opcode.Cpuid(out var result, 0x80000005, 0);

							var l1DataCacheSize = ((result.ecx & 0xff000000) >> 24) * 1000uL; // KB to bytes
							var l1DataCacheAssociativity = (result.ecx & 0xff0000) >> 16;
							var l1DataCacheLineSize = result.ecx & 0xff;

							var cache = new MachineInformation.Cache
							{
								Type = MachineInformation.Cache.CacheType.DATA,
								LineSize = l1DataCacheLineSize,
								Capacity = l1DataCacheSize,
								Associativity = l1DataCacheAssociativity,
								CapacityHRF = Util.FormatBytes(l1DataCacheSize)
							};

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

							var l1InstCacheSize = ((result.edx & 0xff000000) >> 24) * 1000uL; // KB to bytes
							var l1InstCacheAssociativity = (result.edx & 0xff0000) >> 16;
							var l1InstCacheLineSize = result.edx & 0xff;

							var instCache = new MachineInformation.Cache
							{
								Type = MachineInformation.Cache.CacheType.INSTRUCTION,
								LineSize = l1InstCacheLineSize,
								Capacity = l1InstCacheSize,
								Associativity = l1InstCacheAssociativity,
								CapacityHRF = Util.FormatBytes(l1InstCacheSize)
							};

							lock (caches)
							{
								var instOrig = caches.FirstOrDefault(c => c.Equals(instCache));

								if (instOrig == null)
								{
									caches.Add(instCache);
								}
								else
								{
									instOrig.TimesPresent++;
								}
							}
						}


						if (maxExtendedFeatureLevel >= 6)
						{
							Opcode.Cpuid(out var result, 0x80000006, 0);

							var l2CacheSize = ((result.ecx & 0xffff0000) >> 16) * 1000uL; //KB to bytes
							var l2CacheAssociativity = (result.ecx & 0xF000) >> 12;
							var l2CacheLineSize = result.ecx & 0xFF;

							var l2Cache = new MachineInformation.Cache
							{
								Type = MachineInformation.Cache.CacheType.UNIFIED,
								LineSize = l2CacheLineSize,
								Capacity = l2CacheSize,
								Associativity = l2CacheAssociativity,
								CapacityHRF = Util.FormatBytes(l2CacheSize)
							};

							lock (caches)
							{
								var l2CacheOrig = caches.FirstOrDefault(c => c.Equals(l2Cache));

								if (l2CacheOrig == null)
								{
									caches.Add(l2Cache);
								}
								else
								{
									l2CacheOrig.TimesPresent++;
								}
							}

							var l3CacheSize = (ulong) ((result.edx & 0xFFFC0000) >> 18);

							// times 512 KB
							l3CacheSize *= 512000uL;

							var l3CacheAssociativity = (result.edx & 0xF000) >> 12;
							var l3CacheLineSize = result.ecx & 0xFF;

							var l3Cache = new MachineInformation.Cache
							{
								Type = MachineInformation.Cache.CacheType.UNIFIED,
								LineSize = l3CacheLineSize,
								Capacity = l3CacheSize,
								Associativity = l3CacheAssociativity,
								CapacityHRF = Util.FormatBytes(l3CacheSize)
							};

							lock (caches)
							{
								var l3CacheOrig = caches.FirstOrDefault(c => c.Equals(l3Cache));

								if (l3CacheOrig == null)
								{
									caches.Add(l3Cache);
								}
								else
								{
									l3CacheOrig.TimesPresent++;
								}
							}
						}
					}
				}));
			}

			Task.WaitAll(threads.ToArray());

			information.Cpu.Caches = caches;
		}

		private void GatherFeatureFlags(ref MachineInformation information)
		{
			if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 1)
			{
				Opcode.Cpuid(out var result, 0x80000001, 0);

				information.Cpu.AMDFeatureFlags.ExtendedFeatureFlagsF81One =
					(MachineInformation.AMDFeatureFlags.ExtendedFeatureFlagsF81ECX) result.ecx;
				information.Cpu.AMDFeatureFlags.ExtendedFeatureFlagsF81Two =
					(MachineInformation.AMDFeatureFlags.ExtendedFeatureFlagsF81EDX) result.edx;
			}

			if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 7)
			{
				Opcode.Cpuid(out var result, 0x80000007, 0);

				information.Cpu.AMDFeatureFlags.FeatureFlagsApm =
					(MachineInformation.AMDFeatureFlags.FeatureFlagsAPM) result.edx;
			}

			if (information.Cpu.MaxCpuIdExtendedFeatureLevel >= 0xA)
			{
				Opcode.Cpuid(out var result, 0x8000000A, 0);

				information.Cpu.AMDFeatureFlags.FeatureFlagsSvm =
					(MachineInformation.AMDFeatureFlags.FeatureFlagsSVM) result.edx;
			}
		}

		private void GatherPhysicalCores(ref MachineInformation information)
		{
			var threads = new Task[information.Cpu.LogicalCores];
			var coreIds = new Dictionary<uint, uint>();
			var nodeIds = new Dictionary<uint, uint>();
			var cores = information.Cpu.Cores;

			for (var i = 0; i < information.Cpu.LogicalCores; i++)
			{
				var i1 = i;
				threads[i] = Util.RunAffinity(1uL << i, () =>
				{
					Opcode.Cpuid(out var result, 0x8000001E, 0);

					var unitId = Util.ExtractBits(result.ebx, 0, 7);
					var nodeId = Util.ExtractBits(result.ecx, 0, 7);

					lock (coreIds)
					{
						if (coreIds.ContainsKey(unitId))
						{
							coreIds[unitId]++;
						}
						else
						{
							coreIds.Add(unitId, 1);
						}
					}

					lock (nodeIds)
					{
						if (nodeIds.ContainsKey(nodeId))
						{
							nodeIds[nodeId]++;
						}
						else
						{
							nodeIds.Add(nodeId, 1);
						}
					}

					var core = cores.First(c => c.Number == i1);

					core.Node = nodeId;
					core.CoreId = unitId;
				});
			}

			Task.WaitAll(threads);

			information.Cpu.PhysicalCores = (uint) coreIds.Count;
			information.Cpu.Nodes = (uint) nodeIds.Count;
			information.Cpu.LogicalCoresPerNode = nodeIds.First().Value;

			information.Cpu.Cores = cores;
		}
	}
}