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

// ReSharper disable once CheckNamespace
namespace LookupEngine;

public partial class LookupComposer
{
    /// <summary>
    ///     Add fields to the decomposition
    /// </summary>
    private void DecomposeFields(BindingFlags bindingFlags)
    {
        if (!_options.IncludeFields) return;

        var members = MemberDeclaringType.GetFields(bindingFlags);
        foreach (var member in members)
        {
            if (member.IsSpecialName) continue;

            object? value;
            try
            {
                value = EvaluateValue(member);
            }
            catch (TargetInvocationException exception)
            {
                value = exception.InnerException;
            }
            catch (Exception exception)
            {
                value = exception;
            }

            WriteDecompositionMember(value, member, target => CreateEvaluationComposer().EvaluateDeferredMember(target, member, null));
        }
    }
}