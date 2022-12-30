#region using

using BenchmarkDotNet.Running;

#endregion

namespace HardwareInformation.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}