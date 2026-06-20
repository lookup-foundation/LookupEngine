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

namespace LookupEngine.Tests.Benchmarks.Benchmarks;

/// <summary>
///     Compares strategies for the <c>Name (Type1, ref Type2, ...)</c> member name format, as implemented in <c>ReflexionFormater.FormatMemberName</c>.
/// </summary>
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
        if (_parameters.Length == 0) return _member.Name;

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
        if (_parameters.Length == 0) return _member.Name;

        var builder = new System.Text.StringBuilder();
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

            if (i < _parameters.Length - 1) builder.Append(", ");
        }

        builder.Append(')');
        return builder.ToString();
    }

    [Benchmark(Baseline = true)]
    public string StringBuilderSpanTrim()
    {
        if (_parameters.Length == 0) return _member.Name;

        var builder = new System.Text.StringBuilder();
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

            if (i < _parameters.Length - 1) builder.Append(", ");
        }

        builder.Append(')');
        return builder.ToString();
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
}

public sealed class SampleApi
{
    public void Invoke(int count, string name, ref double weight, List<int> items)
    {
        weight = count + items.Count + name.Length;
    }
}
