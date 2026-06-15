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

namespace LookupEngine.Tests.Performance.Benchmarks;

/// <summary>
///     Reflection cost of member enumeration during decomposition, as used in <c>LookupComposer.DecomposeProperties/Methods/Fields</c>.
/// </summary>
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
            if (!member.IsSpecialName) count++;
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
            if (!member.IsSpecialName) count++;
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
            if (!prop.IsSpecialName) list.Add(prop);
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
            if (!prop.IsSpecialName) list.Add(prop);
        }

        return list.Count;
    }
}
