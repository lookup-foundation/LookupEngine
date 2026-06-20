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

namespace LookupEngine.Tests.Benchmarks.Benchmarks;

/// <summary>
///     Compares strategies for base-type chain traversal, as used in <c>LookupComposer.GetTypeHierarchy</c>.
/// </summary>
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

        if (includeRoot) types.Add(inputType);

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

        if (includeRoot) stack.Push(inputType);

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

        if (includeRoot) depth++;

        // Allocate with exact capacity
        var types = new List<Type>(depth);
        while (inputType.BaseType is not null)
        {
            types.Add(inputType);
            inputType = inputType.BaseType;
        }

        if (includeRoot) types.Add(inputType);

        return types;
    }
}
