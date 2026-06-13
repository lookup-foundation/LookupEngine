// Copyright (c) Lookup Foundation and Contributors
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// THIS PROGRAM IS PROVIDED "AS IS" AND WITH ALL FAULTS.
// NO IMPLIED WARRANTY OF MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE IS PROVIDED.
// THERE IS NO GUARANTEE THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.

using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using LookupEngine.Abstractions;

namespace LookupEngine.Tests.Performance.Benchmarks;

/// <summary>
///     Tracks the end-to-end cost of <see cref="LookupComposer.Decompose(object, DecomposeOptions)" />
///     across the available option combinations. Each benchmark decomposes the same stable sample
///     object so that timings and allocations can be compared across engine versions.
/// </summary>
public class DecomposeBenchmark
{
    private SampleData _target = null!;

    private static readonly DecomposeOptions DefaultOptions = new();
    private static readonly DecomposeOptions IncludeRootOptions = new() {IncludeRoot = true};
    private static readonly DecomposeOptions IncludeFieldsOptions = new() {IncludeFields = true};
    private static readonly DecomposeOptions IncludeEventsOptions = new() {IncludeEvents = true};
    private static readonly DecomposeOptions IncludeUnsupportedOptions = new() {IncludeUnsupported = true};
    private static readonly DecomposeOptions IncludePrivateOptions = new() {IncludePrivateMembers = true};
    private static readonly DecomposeOptions IncludeStaticOptions = new() {IncludeStaticMembers = true};
    private static readonly DecomposeOptions EvaluateMethodsOptions = new() {EvaluationPolicy = MethodEvaluationPolicy.All};
    private static readonly DecomposeOptions ExtensionsOptions = new() {EnableExtensions = true};
    private static readonly DecomposeOptions RedirectionOptions = new() {EnableRedirection = true};

    private static readonly DecomposeOptions AllEnabledOptions = new()
    {
        IncludeRoot = true,
        IncludeFields = true,
        IncludeEvents = true,
        IncludeUnsupported = true,
        IncludePrivateMembers = true,
        IncludeStaticMembers = true,
        EnableExtensions = true,
        EnableRedirection = true,
        EvaluationPolicy = MethodEvaluationPolicy.All
    };

    [GlobalSetup]
    public void Setup()
    {
        _target = new SampleData
        {
            Child = new SampleData()
        };
    }

    [Benchmark(Baseline = true)]
    public DecomposedObject Default()
    {
        return LookupComposer.Decompose(_target, DefaultOptions);
    }

    [Benchmark]
    public DecomposedObject IncludeRoot()
    {
        return LookupComposer.Decompose(_target, IncludeRootOptions);
    }

    [Benchmark]
    public DecomposedObject IncludeFields()
    {
        return LookupComposer.Decompose(_target, IncludeFieldsOptions);
    }

    [Benchmark]
    public DecomposedObject IncludeEvents()
    {
        return LookupComposer.Decompose(_target, IncludeEventsOptions);
    }

    [Benchmark]
    public DecomposedObject IncludeUnsupported()
    {
        return LookupComposer.Decompose(_target, IncludeUnsupportedOptions);
    }

    [Benchmark]
    public DecomposedObject IncludePrivateMembers()
    {
        return LookupComposer.Decompose(_target, IncludePrivateOptions);
    }

    [Benchmark]
    public DecomposedObject IncludeStaticMembers()
    {
        return LookupComposer.Decompose(_target, IncludeStaticOptions);
    }

    [Benchmark]
    public DecomposedObject EvaluateMethods()
    {
        return LookupComposer.Decompose(_target, EvaluateMethodsOptions);
    }

    [Benchmark]
    public DecomposedObject EnableExtensions()
    {
        return LookupComposer.Decompose(_target, ExtensionsOptions);
    }

    [Benchmark]
    public DecomposedObject EnableRedirection()
    {
        return LookupComposer.Decompose(_target, RedirectionOptions);
    }

    [Benchmark]
    public DecomposedObject AllEnabled()
    {
        return LookupComposer.Decompose(_target, AllEnabledOptions);
    }
}

[PublicAPI]
[SuppressMessage("ReSharper", "ConvertToAutoProperty")]
public sealed class SampleData
{
    public static string Category => "Default";

    public readonly int Field = 100;
    private readonly string _secret = "hidden";
    private int _counter = 7;

    public int Id { get; set; } = 42;
    public string Name { get; set; } = "Benchmark";
    public DateTime Timestamp { get; set; } = DateTime.UnixEpoch;
    public double Ratio { get; set; } = 3.14;
    public bool Enabled { get; set; } = true;
    public List<int> Numbers { get; set; } = [1, 2, 3, 4, 5];
    public SampleData? Child { get; set; }

    public string Formatted => $"{Name}:{Id}";

    private string Secret => _secret;

    public event EventHandler? Changed;

    public int Compute()
    {
        return Id * 2;
    }

    public string Describe()
    {
        return $"Sample {Name} ({_counter})";
    }

    public void Raise()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
}