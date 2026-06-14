namespace LookupEngine.Options;

/// <summary>
///     Extends <see cref="DecomposeOptions"/> with a required execution context passed through to
///     context-aware descriptors and evaluation handlers.
/// </summary>
/// <typeparam name="TContext">The type of execution context available to registered handlers.</typeparam>
[PublicAPI]
public class DecomposeOptions<TContext> : DecomposeOptions
{
    /// <summary>
    ///     The execution context forwarded to context-aware descriptors during decomposition.
    ///     Any object carrying additional data needed by handlers qualifies as a context.
    /// </summary>
    public required TContext Context { get; set; }
}