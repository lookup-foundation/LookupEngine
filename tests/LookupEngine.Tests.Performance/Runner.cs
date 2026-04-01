using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using LookupEngine.Tests.Performance.Benchmarks;

var configuration = ManualConfig.Create(DefaultConfig.Instance)
    .AddJob(Job.ShortRun)
    .AddDiagnoser(MemoryDiagnoser.Default)
    .AddExporter(MarkdownExporter.GitHub);

// BenchmarkRunner.Run<FormatTypeNameBenchmark>(configuration);
// BenchmarkRunner.Run<TypeHierarchyBenchmark>(configuration);
// BenchmarkRunner.Run<MemberEnumerationBenchmark>(configuration);

// BenchmarkRunner.Run<ResolveTypeBenchmark>(configuration);
// BenchmarkRunner.Run<SortBenchmark>(configuration);
// BenchmarkRunner.Run<TypeEqualBenchmark>(configuration);
BenchmarkRunner.Run<ExtensionRegistrationBenchmark>(configuration);