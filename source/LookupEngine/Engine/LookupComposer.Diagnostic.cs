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
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Diagnostic;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

public partial class LookupComposer
{
    private protected readonly TimeDiagnoser TimeDiagnoser = new();
    private protected readonly MemoryDiagnoser MemoryDiagnoser = new();

    /// <summary>
    ///     Evaluate value with diagnostics
    /// </summary>
    private object? EvaluateValue(FieldInfo member)
    {
        try
        {
            TimeDiagnoser.StartMonitoring();
            MemoryDiagnoser.StartMonitoring();

            return member.GetValue(_input);
        }
        finally
        {
            MemoryDiagnoser.StopMonitoring();
            TimeDiagnoser.StopMonitoring();
        }
    }

    /// <summary>
    ///     Evaluate value with diagnostics
    /// </summary>
    private object? EvaluateValue(PropertyInfo member)
    {
        try
        {
            TimeDiagnoser.StartMonitoring();
            MemoryDiagnoser.StartMonitoring();

            return member.GetValue(_input);
        }
        finally
        {
            MemoryDiagnoser.StopMonitoring();
            TimeDiagnoser.StopMonitoring();
        }
    }

    /// <summary>
    ///     Evaluate value with diagnostics
    /// </summary>
    private object? EvaluateValue(MethodInfo member)
    {
        try
        {
            TimeDiagnoser.StartMonitoring();
            MemoryDiagnoser.StartMonitoring();

            return member.Invoke(_input, null);
        }
        finally
        {
            MemoryDiagnoser.StopMonitoring();
            TimeDiagnoser.StopMonitoring();
        }
    }

    /// <summary>
    ///     Evaluate value with diagnostics
    /// </summary>
    private protected IVariant EvaluateValue(Func<IVariant> handler)
    {
        try
        {
            TimeDiagnoser.StartMonitoring();
            MemoryDiagnoser.StartMonitoring();

            return handler.Invoke();
        }
        finally
        {
            MemoryDiagnoser.StopMonitoring();
            TimeDiagnoser.StopMonitoring();
        }
    }
}