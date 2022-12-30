#region using

using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;

#endregion

namespace HardwareInformation.Benchmarks.Providers
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31, 2, 0, 500, 10000)]
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
        // First has no ProductName, second does
        [Params("USB\\VID_0B05&PID_18F3\\9876543210", "USB\\VID_046D&PID_C07D&MI_01\\8&39C8A24B&0&0001")]
        public string deviceId;

        [Benchmark]
        public void FetchVendorAndProductIdString()
        {
            var vendorId = deviceId.Split('\\')[1].Split('&')[0].Replace("VID_", "");
            var productId = deviceId.Split('\\')[1].Split('&')[1].Replace("PID_", "");
            HardwareInformation.Providers.USBVendorList.GetVendorAndProductName(vendorId, productId);
        }

        [Benchmark]
        public void FetchVendorAndProductIdInt()
        {
            var vendorId = deviceId.Split('\\')[1].Split('&')[0].Replace("VID_", "");
            var productId = deviceId.Split('\\')[1].Split('&')[1].Replace("PID_", "");
            HardwareInformation.Providers.USBVendorList.GetVendorAndProductName(
                int.Parse(vendorId, NumberStyles.HexNumber),
                int.Parse(productId, NumberStyles.HexNumber));
        }
    }
}