using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;

namespace HardwareInformation.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31, 1, 0, targetCount:10, invocationCount: 1)]
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