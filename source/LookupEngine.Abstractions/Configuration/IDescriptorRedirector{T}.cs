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

namespace LookupEngine.Abstractions.Configuration;

/// <summary>
///     Allows a descriptor to substitute the described object with a different one during decomposition, using caller-supplied context to resolve the substitute.
///     Active only when <c>DecomposeOptions.EnableRedirection</c> is <see langword="true"/>.
/// </summary>
/// <typeparam name="TContext">The type of execution context passed through from the decomposition options.</typeparam>
public interface IDescriptorRedirector<in TContext>
{
    /// <summary>
    ///     Attempts to resolve a substitute object for the described value using the execution context.
    /// </summary>
    /// <param name="target">The member name that triggered the redirect attempt.</param>
    /// <param name="context">The execution context provided by the caller.</param>
    /// <param name="result">The substitute object when redirection succeeds.</param>
    /// <returns><see langword="true"/> if a substitute was resolved; otherwise <see langword="false"/>.</returns>
    bool TryRedirect(string target, TContext context, out object result);
}