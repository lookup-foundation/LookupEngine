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

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Abstractions.Enums;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
public partial class LookupComposer : IMemberManager
{
    private static readonly ConcurrentDictionary<(Type Descriptor, Type Interface), bool> ImplementationOwnerCache = new();

    private readonly Dictionary<string, List<MemberRegistration>> _memberRegistrations = new();
    private readonly List<Action> _extensionQueue = [];

    /// <summary>
    ///     A descriptor-declared configuration of a member
    /// </summary>
    private readonly struct MemberRegistration(Func<ParameterInfo[], bool>? predicate, Func<IVariant>? handler, MemberEvaluationPolicy? evaluationPolicy)
    {
        public readonly Func<ParameterInfo[], bool>? Predicate = predicate;
        public readonly Func<IVariant>? Handler = handler;
        public readonly MemberEvaluationPolicy? EvaluationPolicy = evaluationPolicy;
    }

    /// <summary>
    ///     Configures an existing member of the described type by name
    /// </summary>
    public MemberResolverBuilder Member(string name)
    {
        return new MemberResolverBuilder(name, AddMemberRegistration);
    }

    /// <summary>
    ///     Defines a synthetic member that the described type does not have
    /// </summary>
    public ExtensionBuilder Extension(string name)
    {
        return new ExtensionBuilder(name, EnqueueExtension, EnqueueExtensionResult);
    }

    /// <summary>
    ///     Collect the descriptor configuration for the current hierarchy level
    /// </summary>
    private protected virtual void ConfigureMembers()
    {
        _memberRegistrations.Clear();
        _extensionQueue.Clear();

        if (MemberDeclaringDescriptor is IDescriptorConfigurator configurator && DeclaresOwnImplementation(configurator, typeof(IDescriptorConfigurator)))
        {
            configurator.Configure(this);
        }
    }

    /// <summary>
    ///     Evaluate and write the queued extension members after the real members
    /// </summary>
    private void FlushExtensions()
    {
        if (!_options.EnableExtensions) return;

        foreach (var registration in _extensionQueue)
        {
            registration.Invoke();
        }
    }

    /// <summary>
    ///     Find the member configuration matching the member name and runtime parameters
    /// </summary>
    private bool TryLookupMember(string name, ParameterInfo[] parameters, out Func<IVariant>? handler, out MemberEvaluationPolicy? evaluationPolicy)
    {
        handler = null;
        evaluationPolicy = null;

        if (!_memberRegistrations.TryGetValue(name, out var registrations)) return false;

        foreach (var registration in registrations)
        {
            if (registration.Predicate is not null && !registration.Predicate.Invoke(parameters)) continue;

            handler = registration.Handler;
            evaluationPolicy = registration.EvaluationPolicy;
            return true;
        }

        return false;
    }

    private protected void AddMemberRegistration(string name, Func<ParameterInfo[], bool>? predicate, Func<IVariant>? handler, MemberEvaluationPolicy? @override)
    {
        if (!_memberRegistrations.TryGetValue(name, out var registrations))
        {
            registrations = new List<MemberRegistration>(1);
            _memberRegistrations[name] = registrations;
        }

        registrations.Add(new MemberRegistration(predicate, handler, @override));
    }

    private protected void EnqueueExtension(string name, MemberAttributes attributes, Func<IVariant> handler)
    {
        _extensionQueue.Add(() =>
        {
            if ((attributes & MemberAttributes.Static) != 0 && !_options.IncludeStaticMembers) return;

            try
            {
                var result = EvaluateValue(handler);
                WriteExtensionMember(result, name, attributes);
            }
            catch (Exception exception)
            {
                WriteExtensionMember(exception, name, attributes);
            }
        });
    }

    private protected void EnqueueExtensionResult(string name, MemberAttributes attributes, MemberEvaluationPolicy policy)
    {
        _extensionQueue.Add(() =>
        {
            if (!_options.IncludeUnsupported) return;
            if ((attributes & MemberAttributes.Static) != 0 && !_options.IncludeStaticMembers) return;

            WriteExtensionResultMember(name, attributes, policy);
        });
    }

    /// <summary>
    ///     Checks whether the descriptor type declares its own implementation of the given interface
    /// </summary>
    private protected static bool DeclaresOwnImplementation(IDescriptorCollector descriptor, Type interfaceType)
    {
        var type = descriptor.GetType();
        var key = (type, interfaceType);

        if (ImplementationOwnerCache.TryGetValue(key, out var owns))
        {
            return owns;
        }

        var map = type.GetInterfaceMap(interfaceType);
        owns = map.TargetMethods[0].DeclaringType == type;
        ImplementationOwnerCache[key] = owns;

        return owns;
    }
}
