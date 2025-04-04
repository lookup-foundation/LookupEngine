﻿// Copyright (c) Lookup Foundation and Contributors
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

namespace LookupEngine.Tests.Performance;

[MediumRunJob]
[MemoryDiagnoser(false)]
public sealed class SortBenchmark
{
    private MethodInfo[] _methodInfos = null!;

    [GlobalSetup]
    public void Setup()
    {
        var obj = new Thread(() => { });
        _methodInfos = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }

    [Benchmark]
    public void Linq()
    {
        var enumerable = _methodInfos.OrderBy(info => info.Name);
        foreach (var methodInfo in enumerable)
        {
            _ = methodInfo.GetParameters();
        }
    }

    [Benchmark]
    public void SortComparer()
    {
        Array.Sort(_methodInfos, new MethodInfoComparer());
        foreach (var methodInfo in _methodInfos)
        {
            _ = methodInfo.GetParameters();
        }
    }

    [Benchmark]
    public void SortComparison()
    {
        Array.Sort(_methodInfos, Comparison);
        foreach (var methodInfo in _methodInfos)
        {
            _ = methodInfo.GetParameters();
        }
    }

    private int Comparison(MethodInfo x, MethodInfo y)
    {
        return x.Name == y.Name ? 0 : string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class MethodInfoComparer : IComparer<MethodInfo>
{
    public int Compare(MethodInfo? x, MethodInfo? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (y is null) return 1;
        if (x is null) return -1;
        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
}