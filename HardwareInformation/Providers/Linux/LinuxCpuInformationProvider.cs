#region using

using System;
using System.Linq;
using System.Runtime.Versioning;
using HardwareInformation.Information;

#endregion

namespace HardwareInformation.Providers.Linux;

public class LinuxCpuInformationProvider : LinuxInformationProvider
{
    [SupportedOSPlatform("linux")]
    protected override void IdentifyCpus(MachineInformation information)
    {
        #region Example

        /*
processor   : 0
vendor_id   : GenuineIntel
cpu family  : 6
model       : 69
model name  : Intel(R) Core(TM) i7-4500U CPU @ 1.80GHz
stepping    : 1
microcode   : 0x17
cpu MHz     : 774.000
cache size  : 4096 KB
physical id : 0
siblings    : 4
core id     : 0
cpu cores   : 2
apicid      : 0
initial apicid  : 0
fpu     : yes
fpu_exception   : yes
cpuid level : 13
wp      : yes
flags       : fpu vme de pse tsc msr pae mce cx8 apic sep mtrr pge mca cmov pat pse36 clflush dts acpi mmx fxsr sse sse2 ss ht tm pbe syscall nx pdpe1gb rdtscp lm constant_tsc arch_perfmon pebs bts rep_good nopl xtopology nonstop_tsc aperfmperf eagerfpu pni pclmulqdq dtes64 monitor ds_cpl vmx est tm2 ssse3 fma cx16 xtpr pdcm pcid sse4_1 sse4_2 movbe popcnt tsc_deadline_timer aes xsave avx f16c rdrand lahf_lm abm ida arat epb xsaveopt pln pts dtherm tpr_shadow vnmi flexpriority ept vpid fsgsbase tsc_adjust bmi1 avx2 smep bmi2 erms invpcid
bogomips    : 3591.40
clflush size    : 64
cache_alignment : 64
address sizes   : 39 bits physical, 48 bits virtual
power management:

processor   : 1
vendor_id   : GenuineIntel
cpu family  : 6
model       : 69
model name  : Intel(R) Core(TM) i7-4500U CPU @ 1.80GHz
stepping    : 1
microcode   : 0x17
cpu MHz     : 1600.000
cache size  : 4096 KB
physical id : 0
siblings    : 4
core id     : 0
cpu cores   : 2
apicid      : 1
initial apicid  : 1
fpu     : yes
fpu_exception   : yes
cpuid level : 13
wp      : yes
flags       : fpu vme de pse tsc msr pae mce cx8 apic sep mtrr pge mca cmov pat pse36 clflush dts acpi mmx fxsr sse sse2 ss ht tm pbe syscall nx pdpe1gb rdtscp lm constant_tsc arch_perfmon pebs bts rep_good nopl xtopology nonstop_tsc aperfmperf eagerfpu pni pclmulqdq dtes64 monitor ds_cpl vmx est tm2 ssse3 fma cx16 xtpr pdcm pcid sse4_1 sse4_2 movbe popcnt tsc_deadline_timer aes xsave avx f16c rdrand lahf_lm abm ida arat epb xsaveopt pln pts dtherm tpr_shadow vnmi flexpriority ept vpid fsgsbase tsc_adjust bmi1 avx2 smep bmi2 erms invpcid
bogomips    : 3591.40
clflush size    : 64
cache_alignment : 64
address sizes   : 39 bits physical, 48 bits virtual
power management:

processor   : 2
vendor_id   : GenuineIntel
cpu family  : 6
model       : 69
model name  : Intel(R) Core(TM) i7-4500U CPU @ 1.80GHz
stepping    : 1
microcode   : 0x17
cpu MHz     : 800.000
cache size  : 4096 KB
physical id : 0
siblings    : 4
core id     : 1
cpu cores   : 2
apicid      : 2
initial apicid  : 2
fpu     : yes
fpu_exception   : yes
cpuid level : 13
wp      : yes
flags       : fpu vme de pse tsc msr pae mce cx8 apic sep mtrr pge mca cmov pat pse36 clflush dts acpi mmx fxsr sse sse2 ss ht tm pbe syscall nx pdpe1gb rdtscp lm constant_tsc arch_perfmon pebs bts rep_good nopl xtopology nonstop_tsc aperfmperf eagerfpu pni pclmulqdq dtes64 monitor ds_cpl vmx est tm2 ssse3 fma cx16 xtpr pdcm pcid sse4_1 sse4_2 movbe popcnt tsc_deadline_timer aes xsave avx f16c rdrand lahf_lm abm ida arat epb xsaveopt pln pts dtherm tpr_shadow vnmi flexpriority ept vpid fsgsbase tsc_adjust bmi1 avx2 smep bmi2 erms invpcid
bogomips    : 3591.40
clflush size    : 64
cache_alignment : 64
address sizes   : 39 bits physical, 48 bits virtual
power management:

processor   : 3
vendor_id   : GenuineIntel
cpu family  : 6
model       : 69
model name  : Intel(R) Core(TM) i7-4500U CPU @ 1.80GHz
stepping    : 1
microcode   : 0x17
cpu MHz     : 774.000
cache size  : 4096 KB
physical id : 0
siblings    : 4
core id     : 1
cpu cores   : 2
apicid      : 3
initial apicid  : 3
fpu     : yes
fpu_exception   : yes
cpuid level : 13
wp      : yes
flags       : fpu vme de pse tsc msr pae mce cx8 apic sep mtrr pge mca cmov pat pse36 clflush dts acpi mmx fxsr sse sse2 ss ht tm pbe syscall nx pdpe1gb rdtscp lm constant_tsc arch_perfmon pebs bts rep_good nopl xtopology nonstop_tsc aperfmperf eagerfpu pni pclmulqdq dtes64 monitor ds_cpl vmx est tm2 ssse3 fma cx16 xtpr pdcm pcid sse4_1 sse4_2 movbe popcnt tsc_deadline_timer aes xsave avx f16c rdrand lahf_lm abm ida arat epb xsaveopt pln pts dtherm tpr_shadow vnmi flexpriority ept vpid fsgsbase tsc_adjust bmi1 avx2 smep bmi2 erms invpcid
bogomips    : 3591.40
clflush size    : 64
cache_alignment : 64
address sizes   : 39 bits physical, 48 bits virtual
power management:
         */

        #endregion

        if (ReadFile("/proc/cpuinfo", out var data))
        {
            // Split on empty lines
            var logicalCpuCores = data.Split(new[] { Environment.NewLine + Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var logicalCpuCore in logicalCpuCores)
            {
                var lines = logicalCpuCore.Split(Environment.NewLine);
                string value;
                var currentCpuIdx = -1;
                var numberOfLogicalCores = 0;

                if (GetValueFromStartingText(lines, "siblings", out value))
                {
                    numberOfLogicalCores = int.Parse(value);
                }

                if (GetValueFromStartingText(lines, "physical id", out value))
                {
                    currentCpuIdx = int.Parse(value);
                }

                if (currentCpuIdx == -1)
                {
                    // Something was wrong
                    continue;
                }

                if (GetValueFromStartingText(lines, "model name", out value))
                {
                    var name = value.Trim();
                    if (information.Cpus.Count > currentCpuIdx)
                    {
                        if (information.Cpus[currentCpuIdx].Name != name)
                        {
                            // Something doesn't match with the counting of the processors, better break instead of doing a complicated match
                            continue;
                        }
                    }
                    else
                    {
                        if (information.Cpus.Count <= currentCpuIdx - 1)
                        {
                            // We are missing a CPU somehow, break
                            continue;
                        }

                        var startOfLogicalCoreNumbering = (int) information.Cpus.Sum(cpu => cpu.LogicalCores);

                        var cpu = new CPU
                            { Name = name, LogicalCoresInCpu = Enumerable.Range(startOfLogicalCoreNumbering, numberOfLogicalCores).Select(number => (uint)number).ToHashSet() };
                        cpu.InitializeLists();
                        information.Cpus = information.Cpus.ToList().Append(cpu).ToList().AsReadOnly();
                    }
                }

                var logicalCoreIdx = -1;

                if (GetValueFromStartingText(lines, "processor", out value))
                {
                    logicalCoreIdx = int.Parse(value);
                }

                if (logicalCoreIdx == -1)
                {
                    // Something was wrong
                    continue;
                }

                if (GetValueFromStartingText(lines, "cpu MHz", out value))
                {
                    information.Cpus[currentCpuIdx].Cores[logicalCoreIdx].NormalClockSpeed = uint.Parse(value.Split('.', ',')[0]);
                }

                if (GetValueFromStartingText(lines, "vendor_id", out value))
                {
                    information.Cpus[currentCpuIdx].Vendor = value;
                }

                if (GetValueFromStartingText(lines, "cpu family", out value))
                {
                    information.Cpus[currentCpuIdx].Family = uint.Parse(value);
                }

                if (GetValueFromStartingText(lines, "model", out value))
                {
                    information.Cpus[currentCpuIdx].Model = uint.Parse(value);
                }

                if (GetValueFromStartingText(lines, "stepping", out value))
                {
                    information.Cpus[currentCpuIdx].Stepping = uint.Parse(value);
                }

                if (GetValueFromStartingText(lines, "core id", out value))
                {
                    information.Cpus[currentCpuIdx].Cores[logicalCoreIdx].CoreId = uint.Parse(value);
                }
            }
        }
    }

    public override void PostProviderUpdateInformation(MachineInformation information)
    {
        foreach (var cpu in information.Cpus)
        {
            if (cpu.PhysicalCores == default)
            {
                cpu.PhysicalCores = (uint)cpu.Cores.Select(core => core.CoreId).Distinct().Count();
            }
        }
    }
}