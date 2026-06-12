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
using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Abstractions.Enums;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

public partial class LookupComposer<TContext> : IMemberManager<TContext>
{
    MemberResolverBuilder<TContext> IMemberManager<TContext>.Member(string name)
    {
        return new MemberResolverBuilder<TContext>(name, AddContextMemberRegistration);
    }

    ExtensionBuilder<TContext> IMemberManager<TContext>.Extension(string name)
    {
        return new ExtensionBuilder<TContext>(name, EnqueueContextExtension, EnqueueExtensionResult);
    }

    private protected override void ConfigureMembers()
    {
        base.ConfigureMembers();

        if (MemberDeclaringDescriptor is IDescriptorConfigurator<TContext> configurator && DeclaresOwnImplementation(configurator, typeof(IDescriptorConfigurator<TContext>)))
        {
            configurator.Configure(this);
        }
    }

    private void AddContextMemberRegistration(string name, Func<ParameterInfo[], bool>? predicate, Func<TContext, IVariant>? handler, MemberEvaluationPolicy? @override)
    {
        var options = _options;
        var wrapped = handler is null ? null : new Func<IVariant>(() => handler.Invoke(options.Context));
        AddMemberRegistration(name, predicate, wrapped, @override);
    }

    private void EnqueueContextExtension(string name, MemberAttributes attributes, Func<TContext, IVariant> handler)
    {
        var options = _options;
        EnqueueExtension(name, attributes, () => handler.Invoke(options.Context));
    }
}
