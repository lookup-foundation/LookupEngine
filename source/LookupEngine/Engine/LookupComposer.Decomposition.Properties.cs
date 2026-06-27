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
using LookupEngine.Abstractions.Enums;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

[UsedImplicitly]
public partial class LookupComposer
{
    /// <summary>
    ///     Add properties to the decomposition
    /// </summary>
    private void DecomposeProperties(BindingFlags bindingFlags)
    {
        var members = MemberDeclaringType.GetProperties(bindingFlags);
        foreach (var member in members)
        {
            if (member.IsSpecialName) continue;

            var parameters = member.CanRead ? member.GetMethod!.GetParameters() : [];
            TryLookupMember(member.Name, parameters, out var handler, out var evaluationOverride);

            if (evaluationOverride == MemberEvaluationPolicy.Disabled)
            {
                WriteDisabledMember(member, member.PropertyType, parameters);
                continue;
            }

            if (handler is null)
            {
                if (!member.CanRead)
                {
                    if (!_options.IncludeUnsupported) continue;

                    WriteUnsupportedMember(member, member.PropertyType, parameters);
                    continue;
                }

                if (parameters.Length > 0)
                {
                    if (!_options.IncludeUnsupported) continue;

                    WriteUnsupportedMember(member, member.PropertyType, parameters);
                    continue;
                }
            }

            if (evaluationOverride == MemberEvaluationPolicy.Deferred)
            {
                WriteDeferredMember(member, member.PropertyType, parameters, handler);
                continue;
            }

            object? value;
            try
            {
                value = handler is not null ? EvaluateValue(handler) : EvaluateValue(member);
            }
            catch (TargetInvocationException exception)
            {
                value = exception.InnerException;
            }
            catch (Exception exception)
            {
                value = exception;
            }

            WriteDecompositionMember(value, member, parameters);
        }
    }
}