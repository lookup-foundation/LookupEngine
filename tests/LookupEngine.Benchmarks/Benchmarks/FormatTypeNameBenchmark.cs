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

using System.Text;
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
// | Method                 | Type                            | Mean        | Error     | StdDev    | Median      | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
// |----------------------- |-------------------------------- |------------:|----------:|----------:|------------:|------:|--------:|-------:|----------:|------------:|
// | **StringConcatenation**    | **Dictionary&lt;String, List&lt;Int32&gt;&gt;** | **115.1318 ns** | **1.0315 ns** | **0.9649 ns** | **114.8756 ns** |  **1.00** |    **0.01** | **0.0124** |     **624 B** |        **1.00** |
// | StringBuilderRecursive | Dictionary&lt;String, List&lt;Int32&gt;&gt; | 102.5164 ns | 1.9521 ns | 1.8260 ns | 101.9920 ns |  0.89 |    0.02 | 0.0073 |     368 B |        0.59 |
// | SpanWithStringBuilder  | Dictionary&lt;String, List&lt;Int32&gt;&gt; | 112.2865 ns | 1.5998 ns | 1.4964 ns | 112.1544 ns |  0.98 |    0.01 | 0.0103 |     520 B |        0.83 |
// |                        |                                 |             |           |           |             |       |         |        |           |             |
// | **StringConcatenation**    | **List&lt;Int32&gt;**                     |  **49.8013 ns** | **1.0200 ns** | **2.1515 ns** |  **48.9871 ns** |  **1.00** |    **0.06** | **0.0038** |     **192 B** |        **1.00** |
// | StringBuilderRecursive | List&lt;Int32&gt;                     |  44.6391 ns | 0.4806 ns | 0.4720 ns |  44.6927 ns |  0.90 |    0.04 | 0.0036 |     184 B |        0.96 |
// | SpanWithStringBuilder  | List&lt;Int32&gt;                     |  43.8603 ns | 0.3126 ns | 0.2771 ns |  43.8365 ns |  0.88 |    0.04 | 0.0036 |     184 B |        0.96 |
// |                        |                                 |             |           |           |             |       |         |        |           |             |
// | **StringConcatenation**    | **String**                          |   **0.8574 ns** | **0.0145 ns** | **0.0128 ns** |   **0.8545 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
// | StringBuilderRecursive | String                          |  11.6464 ns | 0.2331 ns | 0.1946 ns |  11.6060 ns | 13.59 |    0.29 | 0.0029 |     144 B |          NA |
// | SpanWithStringBuilder  | String                          |   0.9322 ns | 0.0161 ns | 0.0143 ns |   0.9325 ns |  1.09 |    0.02 |      - |         - |          NA |

/// <summary>
///     Compares strategies for the <c>Name&lt;Arg1, Arg2&gt;</c> generic type name format, as implemented in <c>ReflexionFormater.FormatTypeName</c>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
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
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var typeName = type.Name;
        var apostropheIndex = typeName.IndexOf('`');
        if (apostropheIndex > 0)
        {
            typeName = typeName[..apostropheIndex];
        }

        typeName += "<";
        var genericArguments = type.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; i++)
        {
            typeName += FormatTypeName(genericArguments[i]);
            if (i < genericArguments.Length - 1)
            {
                typeName += ", ";
            }
        }

        typeName += ">";
        return typeName;
    }

    private static string FormatTypeNameWithStringBuilder(Type type)
    {
        var builder = new StringBuilder();
        FormatTypeNameRecursive(type, builder);
        return builder.ToString();
    }

    private static void FormatTypeNameRecursive(Type type, StringBuilder builder)
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
            if (i < genericArguments.Length - 1)
            {
                builder.Append(", ");
            }
        }

        builder.Append('>');
    }

    private static string FormatTypeNameWithSpan(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var typeName = type.Name.AsSpan();
        var apostropheIndex = typeName.IndexOf('`');
        var baseName = apostropheIndex > 0 ? typeName[..apostropheIndex] : typeName;

        var builder = new StringBuilder();
        builder.Append(baseName);
        builder.Append('<');

        var genericArguments = type.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; i++)
        {
            builder.Append(FormatTypeNameWithSpan(genericArguments[i]));
            if (i < genericArguments.Length - 1)
            {
                builder.Append(", ");
            }
        }

        builder.Append('>');
        return builder.ToString();
    }
}
