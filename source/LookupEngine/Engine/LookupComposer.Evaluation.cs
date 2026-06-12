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
using LookupEngine.Abstractions;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Abstractions.Enums;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

public partial class LookupComposer
{
    /// <summary>
    ///     Create a fresh composer for deferred member evaluation
    /// </summary>
    private protected virtual LookupComposer CreateEvaluationComposer()
    {
        return new LookupComposer(Input, _options);
    }

    /// <summary>
    ///     Evaluate a deferred member, updating the value and metrics in place
    /// </summary>
    internal void EvaluateDeferredMember(DecomposedMember target, MemberInfo member, Func<IVariant>? handler)
    {
        object? value;
        try
        {
            value = handler is not null ? EvaluateValue(handler) : EvaluateMember(member);
        }
        catch (TargetInvocationException exception)
        {
            value = exception.InnerException;
        }
        catch (Exception exception)
        {
            value = exception;
        }

        target.Value = CreateValue(member.Name, value);
        target.ComputationTime = TimeDiagnoser.GetElapsed().TotalMilliseconds;
        target.AllocatedBytes = MemoryDiagnoser.GetAllocatedBytes();
        target.EvaluationPolicy = MemberEvaluationPolicy.Evaluated;
    }

    /// <summary>
    ///     Report the disabled evaluation result for a disabled member
    /// </summary>
    internal void EvaluateDisabledMember(DecomposedMember target, string memberName)
    {
        target.Value = CreateValue(memberName, new InvalidOperationException("Member execution disabled"));
    }

    /// <summary>
    ///     Evaluate a member value by reflection
    /// </summary>
    private object? EvaluateMember(MemberInfo member)
    {
        return member switch
        {
            MethodInfo method => EvaluateValue(method),
            PropertyInfo property => EvaluateValue(property),
            _ => throw new NotSupportedException($"Unsupported member type: {member.MemberType}")
        };
    }
}