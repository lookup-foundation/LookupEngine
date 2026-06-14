using LookupEngine.Options;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

/// <summary>
///     Provides functionality to work with the internal structure of an object
/// </summary>
[PublicAPI]
public sealed partial class LookupComposer<TContext> : LookupComposer
{
    private readonly DecomposeOptions<TContext> _options;

    internal LookupComposer(object value, DecomposeOptions<TContext> options) : base(value, options)
    {
        _options = options;
    }
}