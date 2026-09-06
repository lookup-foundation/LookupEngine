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
// | Method                              | Type       | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
// |------------------------------------ |----------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
// | **PatternMatching**                     | **?**          | **1.352 ns** | **0.0236 ns** | **0.0210 ns** |  **0.96** |    **0.02** |         **-** |          **NA** |
// | PatternMatchingWithTypeEquality     | ?          | 1.413 ns | 0.0139 ns | 0.0116 ns |  1.00 |    0.01 |         - |          NA |
// | PatternMatchingWithFullNameEquality | ?          | 1.868 ns | 0.0202 ns | 0.0179 ns |  1.32 |    0.02 |         - |          NA |
// |                                     |            |          |           |           |       |         |           |             |
// | **PatternMatching**                     | **ButtonBase** | **1.364 ns** | **0.0262 ns** | **0.0245 ns** |  **0.68** |    **0.01** |         **-** |          **NA** |
// | PatternMatchingWithTypeEquality     | ButtonBase | 2.002 ns | 0.0263 ns | 0.0246 ns |  1.00 |    0.02 |         - |          NA |
// | PatternMatchingWithFullNameEquality | ButtonBase | 4.209 ns | 0.0513 ns | 0.0480 ns |  2.10 |    0.03 |         - |          NA |

/// <summary>
///     Compares strategies for descriptor resolution by runtime type in a pattern-match switch, as used in <c>DecomposeOptions.DefaultResolveMap</c>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class TypeEqualBenchmark
{
    private object Object { get; } = new RoundButton();

    [Params(null, typeof(ButtonBase))] public Type? Type { get; set; }

    [Benchmark]
    public string? PatternMatching()
    {
        return PatternMatchingSwitch(Object);
    }

    [Benchmark(Baseline = true)]
    public string? PatternMatchingWithTypeEquality()
    {
        return PatternMatchingWithTypeCheck(Object, Type);
    }

    [Benchmark]
    public string? PatternMatchingWithFullNameEquality()
    {
        return PatternMatchingWithFullName(Object, Type);
    }

    private string? PatternMatchingSwitch(object? obj)
    {
        return obj switch
        {
            RoundButton value => value.ToString(),
            Button value => value.ToString(),
            ButtonBase value => value.ToString(),
            _ => Object.ToString()
        };
    }

    private string? PatternMatchingWithTypeCheck(object? obj, Type? type)
    {
        return obj switch
        {
            RoundButton value when type is null || type == typeof(RoundButton) => value.ToString(),
            Button value when type is null || type == typeof(Button) => value.ToString(),
            ButtonBase value when type is null || type == typeof(ButtonBase) => value.ToString(),
            _ => Object.ToString()
        };
    }

    private string? PatternMatchingWithFullName(object? obj, Type? type)
    {
        return obj switch
        {
            RoundButton value when type is null || type.FullName == typeof(RoundButton).FullName => value.ToString(),
            Button value when type is null || type.FullName == typeof(Button).FullName => value.ToString(),
            ButtonBase value when type is null || type.FullName == typeof(ButtonBase).FullName => value.ToString(),
            _ => Object.ToString()
        };
    }
}

[PublicAPI]
public class ButtonBase;

[PublicAPI]
public class Button : ButtonBase;

[PublicAPI]
public sealed class RoundButton : Button;
