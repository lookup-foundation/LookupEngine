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

namespace LookupEngine.Tests.Performance.Benchmarks;

/// <summary>
///     Compares strategies for the <c>Name&lt;Arg1, Arg2&gt;</c> generic type name format, as implemented in <c>ReflexionFormater.FormatTypeName</c>.
/// </summary>
public class FormatTypeNameBenchmark
{
    [Params(typeof(string), typeof(List<int>), typeof(Dictionary<string, List<int>>))]
    public Type Type { get; set; } = null!;

    [Benchmark(Baseline = true)]
    public string StringConcatenation()
    {
        return FormatTypeName(Type);
    }

    [Benchmark]
    public string StringBuilderRecursive()
    {
        return FormatTypeNameWithStringBuilder(Type);
    }

    [Benchmark]
    public string SpanWithStringBuilder()
    {
        return FormatTypeNameWithSpan(Type);
    }

    private static string FormatTypeName(Type type)
    {
        if (!type.IsGenericType) return type.Name;

        var typeName = type.Name;
        var apostropheIndex = typeName.IndexOf('`');
        if (apostropheIndex > 0) typeName = typeName[..apostropheIndex];
        typeName += "<";
        var genericArguments = type.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; i++)
        {
            typeName += FormatTypeName(genericArguments[i]);
            if (i < genericArguments.Length - 1) typeName += ", ";
        }

        typeName += ">";
        return typeName;
    }

    private static string FormatTypeNameWithStringBuilder(Type type)
    {
        var builder = new System.Text.StringBuilder();
        FormatTypeNameRecursive(type, builder);
        return builder.ToString();
    }

    private static void FormatTypeNameRecursive(Type type, System.Text.StringBuilder builder)
    {
        if (!type.IsGenericType)
        {
            builder.Append(type.Name);
            return;
        }

        var typeName = type.Name;
        var apostropheIndex = typeName.IndexOf('`');
        if (apostropheIndex > 0)
        {
            builder.Append(typeName.AsSpan(0, apostropheIndex));
        }
        else
        {
            builder.Append(typeName);
        }

        builder.Append('<');
        var genericArguments = type.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; i++)
        {
            FormatTypeNameRecursive(genericArguments[i], builder);
            if (i < genericArguments.Length - 1) builder.Append(", ");
        }

        builder.Append('>');
    }

    private static string FormatTypeNameWithSpan(Type type)
    {
        if (!type.IsGenericType) return type.Name;

        var typeName = type.Name.AsSpan();
        var apostropheIndex = typeName.IndexOf('`');
        var baseName = apostropheIndex > 0 ? typeName[..apostropheIndex] : typeName;

        var builder = new System.Text.StringBuilder();
        builder.Append(baseName);
        builder.Append('<');

        var genericArguments = type.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; i++)
        {
            builder.Append(FormatTypeNameWithSpan(genericArguments[i]));
            if (i < genericArguments.Length - 1) builder.Append(", ");
        }

        builder.Append('>');
        return builder.ToString();
    }
}
