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
// | Method                | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
// |---------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
// | LinqSelectJoin        | 132.6 ns | 1.88 ns | 1.76 ns |  1.25 |    0.02 | 0.0110 |     552 B |        0.84 |
// | StringBuilderAppend   | 123.9 ns | 1.44 ns | 1.34 ns |  1.17 |    0.01 | 0.0138 |     696 B |        1.06 |
// | StringBuilderSpanTrim | 106.3 ns | 0.56 ns | 0.47 ns |  1.00 |    0.01 | 0.0130 |     656 B |        1.00 |

/// <summary>
///     Compares strategies for the <c>Name (Type1, ref Type2, ...)</c> member name format, as implemented in <c>ReflexionFormater.FormatMemberName</c>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class FormatMemberNameBenchmark
{
    private MemberInfo _member = null!;
    private ParameterInfo[] _parameters = null!;

    [GlobalSetup]
    public void Setup()
    {
        var method = typeof(SampleApi).GetMethod(nameof(SampleApi.Invoke))!;
        _member = method;
        _parameters = method.GetParameters();
    }

    [Benchmark]
    public string LinqSelectJoin()
    {
        if (_parameters.Length == 0)
        {
            return _member.Name;
        }

        var formatedParameters = _parameters.Select(info =>
        {
            return info.ParameterType.IsByRef switch
            {
                true => $"ref {FormatTypeName(info.ParameterType).Replace("&", string.Empty)}",
                false => FormatTypeName(info.ParameterType)
            };
        });

        return $"{_member.Name} ({string.Join(", ", formatedParameters)})";
    }

    [Benchmark]
    public string StringBuilderAppend()
    {
        if (_parameters.Length == 0)
        {
            return _member.Name;
        }

        var builder = new StringBuilder();
        builder.Append(_member.Name);
        builder.Append(" (");

        for (var i = 0; i < _parameters.Length; i++)
        {
            var parameterType = _parameters[i].ParameterType;
            if (parameterType.IsByRef)
            {
                builder.Append("ref ");
                builder.Append(FormatTypeName(parameterType).Replace("&", string.Empty));
            }
            else
            {
                builder.Append(FormatTypeName(parameterType));
            }

            if (i < _parameters.Length - 1)
            {
                builder.Append(", ");
            }
        }

        builder.Append(')');
        return builder.ToString();
    }

    [Benchmark(Baseline = true)]
    public string StringBuilderSpanTrim()
    {
        if (_parameters.Length == 0)
        {
            return _member.Name;
        }

        var builder = new StringBuilder();
        builder.Append(_member.Name);
        builder.Append(" (");

        for (var i = 0; i < _parameters.Length; i++)
        {
            var parameterType = _parameters[i].ParameterType;
            if (parameterType.IsByRef)
            {
                builder.Append("ref ");
                var name = FormatTypeName(parameterType).AsSpan();
                builder.Append(name[^1] == '&' ? name[..^1] : name);
            }
            else
            {
                builder.Append(FormatTypeName(parameterType));
            }

            if (i < _parameters.Length - 1)
            {
                builder.Append(", ");
            }
        }

        builder.Append(')');
        return builder.ToString();
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
}

public sealed class SampleApi
{
    public void Invoke(int count, string name, ref double weight, List<int> items)
    {
        weight = count + items.Count + name.Length;
    }
}
