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

using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Exceptions;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

public partial class LookupComposer<TContext>
{
    /// <summary>
    ///     Redirect the in-context member value to another object
    /// </summary>
    private protected override object RedirectValue(object value)
    {
        if (!_options.EnableRedirection) return value;

        var redirections = 0;
        var valueDescriptor = _options.TypeResolver.Invoke(value, null);
        while (redirections++ < MaxRedirections)
        {
            var redirected = false;

            // Generic interface is prioritised
            if (valueDescriptor is IDescriptorRedirector<TContext> genericRedirector)
            {
                if (genericRedirector.TryRedirect(string.Empty, _options.Context, out value))
                {
                    redirected = true;
                }
            }
            else if (valueDescriptor is IDescriptorRedirector redirector)
            {
                if (redirector.TryRedirect(string.Empty, out value))
                {
                    redirected = true;
                }
            }

            if (!redirected) break;

            valueDescriptor = _options.TypeResolver.Invoke(value, null);
        }

        return value;
    }

    /// <summary>
    ///     Redirect the decomposed in-context value to another object
    /// </summary>
    private protected override object RedirectValue(object value, string target, out Descriptor valueDescriptor, out string? description)
    {
        var variant = value as IVariant;
        if (variant is not null)
        {
            value = variant.Value ?? throw new EngineException("Nullable variant must be handled before decomposition");
        }

        valueDescriptor = _options.TypeResolver.Invoke(value, null);

        description = valueDescriptor.Description;
        if (variant is not null && description is null)
        {
            description = variant.Description;
        }

        if (_options.EnableRedirection)
        {
            var redirections = 0;
            while (redirections++ < MaxRedirections)
            {
                var redirected = false;

                // Generic interface is prioritised
                if (valueDescriptor is IDescriptorRedirector<TContext> genericRedirector)
                {
                    if (genericRedirector.TryRedirect(target, _options.Context, out value))
                    {
                        redirected = true;
                    }
                }
                else if (valueDescriptor is IDescriptorRedirector redirector)
                {
                    if (redirector.TryRedirect(target, out value))
                    {
                        redirected = true;
                    }
                }

                if (!redirected) break;

                valueDescriptor = _options.TypeResolver.Invoke(value, null);

                if (valueDescriptor.Description is not null)
                {
                    description = valueDescriptor.Description;
                }
            }
        }

        return value;
    }
}