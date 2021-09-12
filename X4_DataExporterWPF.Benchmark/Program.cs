using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using X4_DataExporterWPF;
using X4_DataExporterWPF.DataExportWindow;

BenchmarkRunner.Run<ExportBenchmark>();

namespace X4_DataExporterWPF
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser]
    public class ExportBenchmark
    {
        [Benchmark(Baseline = true)]
        public void Export()
        {
            new DataExportModel().Export(
                new Progress<(int currentStep, int maxSteps)>(_ => { }),
                new Progress<(int currentStep, int maxSteps)>(_ => { }),
                @"C:\Program Files (x86)\Steam\steamapps\common\X4 Foundations",
                ":memory:",
                new(81, "日本語"),
                null
            );
        }
    }
}
