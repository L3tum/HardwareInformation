#region using

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;

#endregion

namespace HardwareInformation.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31, 2, 0, 5, 1)]
    [MemoryDiagnoser]
    [RPlotExporter]
    [MarkdownExporter]
    [AsciiDocExporter]
    [HtmlExporter]
    [CsvExporter]
    [MinColumn]
    [MaxColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(NumeralSystem.Roman)]
    public class MachineInformationGatherer
    {
        [Benchmark]
        public void GatherInformation()
        {
            HardwareInformation.MachineInformationGatherer.GatherInformation(invalidateCache: true);
        }
    }
}