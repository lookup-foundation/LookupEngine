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

using System.Reflection;
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
// | Method                      | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
// |---------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
// | PropertiesPublicOnly        |  49.821 ns | 0.7289 ns | 0.6818 ns |  1.00 |    0.02 | 0.0052 |     264 B |        1.00 |
// | PropertiesPublicAndPrivate  |  44.678 ns | 0.9145 ns | 0.8982 ns |  0.90 |    0.02 | 0.0052 |     264 B |        1.00 |
// | FieldsPublicOnly            |   6.149 ns | 0.0539 ns | 0.0504 ns |  0.12 |    0.00 |      - |         - |        0.00 |
// | FieldsPublicAndPrivate      |  39.243 ns | 0.6533 ns | 0.6111 ns |  0.79 |    0.02 | 0.0048 |     240 B |        0.91 |
// | MethodsPublicOnly           | 220.177 ns | 1.4528 ns | 1.2879 ns |  4.42 |    0.06 | 0.0293 |    1480 B |        5.61 |
// | MethodsPublicAndPrivate     | 416.879 ns | 4.5150 ns | 4.2233 ns |  8.37 |    0.14 | 0.0324 |    1632 B |        6.18 |
// | PropertiesFilterSpecialName |  44.841 ns | 0.5552 ns | 0.7783 ns |  0.90 |    0.02 | 0.0052 |     264 B |        1.00 |
// | MethodsFilterSpecialName    | 213.569 ns | 2.1756 ns | 1.9286 ns |  4.29 |    0.07 | 0.0293 |    1480 B |        5.61 |
// | ListWithCapacity32          |  76.814 ns | 1.3779 ns | 1.2215 ns |  1.54 |    0.03 | 0.0114 |     576 B |        2.18 |
// | ListWithDefaultCapacity     |  95.701 ns | 1.9096 ns | 1.9610 ns |  1.92 |    0.05 | 0.0117 |     592 B |        2.24 |

/// <summary>
///     Reflection cost of member enumeration during decomposition, as used in <c>LookupComposer.DecomposeProperties/Methods/Fields</c>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class MemberEnumerationBenchmark
{
    private const BindingFlags PublicInstanceFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
    private const BindingFlags AllInstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private Type _type = null!;

    [GlobalSetup]
    public void Setup()
    {
        _type = typeof(Thread);
    }

    [Benchmark(Baseline = true)]
    public PropertyInfo[] PropertiesPublicOnly()
    {
        return _type.GetProperties(PublicInstanceFlags);
    }

    [Benchmark]
    public PropertyInfo[] PropertiesPublicAndPrivate()
    {
        return _type.GetProperties(AllInstanceFlags);
    }

    [Benchmark]
    public FieldInfo[] FieldsPublicOnly()
    {
        return _type.GetFields(PublicInstanceFlags);
    }

    [Benchmark]
    public FieldInfo[] FieldsPublicAndPrivate()
    {
        return _type.GetFields(AllInstanceFlags);
    }

    [Benchmark]
    public MethodInfo[] MethodsPublicOnly()
    {
        return _type.GetMethods(PublicInstanceFlags);
    }

    [Benchmark]
    public MethodInfo[] MethodsPublicAndPrivate()
    {
        return _type.GetMethods(AllInstanceFlags);
    }

    [Benchmark]
    public int PropertiesFilterSpecialName()
    {
        var members = _type.GetProperties(PublicInstanceFlags);
        var count = 0;
        foreach (var member in members)
        {
            if (!member.IsSpecialName)
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int MethodsFilterSpecialName()
    {
        var members = _type.GetMethods(PublicInstanceFlags);
        var count = 0;
        foreach (var member in members)
        {
            if (!member.IsSpecialName)
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int ListWithCapacity32()
    {
        var list = new List<object>(32);
        var properties = _type.GetProperties(PublicInstanceFlags);
        foreach (var prop in properties)
        {
            if (!prop.IsSpecialName)
            {
                list.Add(prop);
            }
        }

        return list.Count;
    }

    [Benchmark]
    public int ListWithDefaultCapacity()
    {
        var list = new List<object>();
        var properties = _type.GetProperties(PublicInstanceFlags);
        foreach (var prop in properties)
        {
            if (!prop.IsSpecialName)
            {
                list.Add(prop);
            }
        }

        return list.Count;
    }
}
