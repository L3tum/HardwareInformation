using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;

namespace HardwareInformation.Benchmarks.Providers
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
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
    public class USBVendorList
    {
        [Params("USB\\VID_046D&PID_0A66\\8&2A1B065E&0&6", "USB\\VID_046D&PID_C07D&MI_01\\8&39C8A24B&0&0001",
            "USB\\VID_0B05&PID_18F3\\9876543210")]
        public string deviceId;

        [Benchmark]
        public void FetchVendorIdString()
        {
            var vendorId = deviceId.Split('\\')[1].Split('&')[0].Replace("VID_", "");
            HardwareInformation.Providers.USBVendorList.GetVendorName(vendorId);
        }

        [Benchmark]
        public void FetchVendorIdInt()
        {
            var vendorId = deviceId.Split('\\')[1].Split('&')[0].Replace("VID_", "");
            HardwareInformation.Providers.USBVendorList.GetVendorName(int.Parse(vendorId, NumberStyles.HexNumber));
        }

        [Benchmark]
        public void FetchProductIdString()
        {
            var vendorId = deviceId.Split('\\')[1].Split('&')[0].Replace("VID_", "");
            var productId = deviceId.Split('\\')[1].Split('&')[1].Replace("PID_", "");
            HardwareInformation.Providers.USBVendorList.GetProductName(vendorId, productId);
        }

        [Benchmark(Baseline = true)]
        public void FetchProductIdInt()
        {
            var vendorId = deviceId.Split('\\')[1].Split('&')[0].Replace("VID_", "");
            var productId = deviceId.Split('\\')[1].Split('&')[1].Replace("PID_", "");
            HardwareInformation.Providers.USBVendorList.GetProductName(int.Parse(vendorId, NumberStyles.HexNumber),
                int.Parse(productId, NumberStyles.HexNumber));
        }
    }
}