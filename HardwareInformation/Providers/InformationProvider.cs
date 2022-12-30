#region using

using System.Threading.Tasks;

#endregion

namespace HardwareInformation.Providers
{
    public abstract class InformationProvider
    {
        /// <summary>
        ///     Check if this provider or the underlying method is available on this platform
        /// </summary>
        /// <param name="information"></param>
        /// <returns></returns>
        public abstract bool Available(MachineInformation information);

        public virtual void GatherInformation(MachineInformation information)
        {
            IdentifyCpus(information);
            var cpuTasks = Util.RunAffinityPerCpu(information.Cpus, cpuIndex => GatherPerCpuInformation(cpuIndex, information));
            Task.WaitAll(cpuTasks);

            var coreTasks = Util.RunAffinityPerCpuCore(information.Cpus, (cpuIndex, coreIndex) => GatherPerCoreInformation(cpuIndex, coreIndex, information));

            foreach (var tasks in coreTasks)
            {
                Task.WaitAll(tasks);
            }
        }

        /// <summary>
        ///     This method should populate the Cpus array in MachineInformation so that any further information gathering can be
        ///     performed per-cpu.
        /// </summary>
        /// <param name="information"></param>
        protected virtual void IdentifyCpus(MachineInformation information)
        {
        }

        protected virtual void GatherPerCpuInformation(int cpuIndex, MachineInformation information)
        {
        }

        protected virtual void GatherPerCoreInformation(int cpuIndex, int coreIndex, MachineInformation information)
        {
        }

        /// <summary>
        ///     Update information that may depend on other providers (OS providers for example)
        /// </summary>
        /// <param name="information"></param>
        public virtual void PostProviderUpdateInformation(MachineInformation information)
        {
        }
    }
}