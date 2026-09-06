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

using BenchmarkDotNet.Attributes;

namespace LookupEngine.Benchmarks.Benchmarks;

// ```
//
// BenchmarkDotNet v0.15.8, Windows 11 (10.0.28000.2704/26H1/2026Update)
// AMD Ryzen 9 9950X3D 4.30GHz, 1 CPU, 32 logical and 16 physical cores
// .NET SDK 10.0.111
//   [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
//   DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
//
// ```
//
// | Method                       | Type              | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
// |----------------------------- |------------------ |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
// | **ListWithDynamicGrowth**        | **ArgumentException** | **27.95 ns** | **0.351 ns** | **0.328 ns** |  **1.00** |    **0.02** | **0.0017** |      **88 B** |        **1.00** |
// | StackBasedReversal           | ArgumentException | 47.44 ns | 0.352 ns | 0.312 ns |  1.70 |    0.02 | 0.0035 |     176 B |        2.00 |
// | ListWithPreallocatedCapacity | ArgumentException | 40.93 ns | 0.425 ns | 0.398 ns |  1.46 |    0.02 | 0.0017 |      88 B |        1.00 |
// |                              |                   |          |          |          |       |         |        |           |             |
// | **ListWithDynamicGrowth**        | **List&lt;Int32&gt;**       | **18.82 ns** | **0.142 ns** | **0.132 ns** |  **1.00** |    **0.01** | **0.0017** |      **88 B** |        **1.00** |
// | StackBasedReversal           | List&lt;Int32&gt;       | 29.75 ns | 0.421 ns | 0.394 ns |  1.58 |    0.02 | 0.0035 |     176 B |        2.00 |
// | ListWithPreallocatedCapacity | List&lt;Int32&gt;       | 20.75 ns | 0.191 ns | 0.179 ns |  1.10 |    0.01 | 0.0014 |      72 B |        0.82 |
// |                              |                   |          |          |          |       |         |        |           |             |
// | **ListWithDynamicGrowth**        | **String**            | **16.49 ns** | **0.224 ns** | **0.210 ns** |  **1.00** |    **0.02** | **0.0017** |      **88 B** |        **1.00** |
// | StackBasedReversal           | String            | 30.56 ns | 0.234 ns | 0.219 ns |  1.85 |    0.03 | 0.0035 |     176 B |        2.00 |
// | ListWithPreallocatedCapacity | String            | 20.79 ns | 0.299 ns | 0.279 ns |  1.26 |    0.02 | 0.0014 |      72 B |        0.82 |

/// <summary>
///     Compares strategies for base-type chain traversal, as used in <c>LookupComposer.GetTypeHierarchy</c>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class TypeHierarchyBenchmark
{
    [Params(typeof(string), typeof(ArgumentException), typeof(List<int>))]
    public Type Type { get; set; } = null!;

    [Benchmark(Baseline = true)]
    public List<Type> ListWithDynamicGrowth()
    {
        return GetTypeHierarchy(Type, true);
    }

    [Benchmark]
    public List<Type> StackBasedReversal()
    {
        return StackBasedHierarchy(Type, true);
    }

    [Benchmark]
    public List<Type> ListWithPreallocatedCapacity()
    {
        return PreallocatedCapacity(Type, true);
    }

    private static List<Type> GetTypeHierarchy(Type inputType, bool includeRoot)
    {
        var types = new List<Type>();
        while (inputType.BaseType is not null)
        {
            types.Add(inputType);
            inputType = inputType.BaseType;
        }

        if (includeRoot)
        {
            types.Add(inputType);
        }

        return types;
    }

    private static List<Type> StackBasedHierarchy(Type inputType, bool includeRoot)
    {
        var stack = new Stack<Type>();
        while (inputType.BaseType is not null)
        {
            stack.Push(inputType);
            inputType = inputType.BaseType;
        }

        if (includeRoot)
        {
            stack.Push(inputType);
        }

        return new List<Type>(stack);
    }

    private static List<Type> PreallocatedCapacity(Type inputType, bool includeRoot)
    {
        // Count depth first
        var depth = 0;
        var current = inputType;
        while (current.BaseType is not null)
        {
            depth++;
            current = current.BaseType;
        }

        if (includeRoot)
        {
            depth++;
        }

        // Allocate with exact capacity
        var types = new List<Type>(depth);
        while (inputType.BaseType is not null)
        {
            types.Add(inputType);
            inputType = inputType.BaseType;
        }

        if (includeRoot)
        {
            types.Add(inputType);
        }

        return types;
    }
}
