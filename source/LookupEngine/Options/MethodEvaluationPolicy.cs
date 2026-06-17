using System.Reflection;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

/// <summary>
///     Determines which methods are evaluated eagerly during decomposition based on the namespace of their
///     declaring type. Methods that do not match are deferred and included with an evaluation handle.
///     The policy governs methods only; properties and synthetic extensions are evaluated eagerly by default.
/// </summary>
[PublicAPI]
public sealed class MethodEvaluationPolicy
{
    private readonly CompiledPattern[] _compiledNamespaces = [];

    /// <summary>
    ///     Namespace wildcard patterns that identify declaring types whose methods are evaluated eagerly.
    ///     The <c>*</c> wildcard matches zero or more characters; matching is ordinal and case-sensitive.
    ///     Empty by default, meaning all methods are deferred.
    /// </summary>
    /// <remarks>
    ///     Examples: <c>"MyApp.Domain"</c> matches that exact namespace; <c>"MyApp.*"</c> matches all
    ///     sub-namespaces; <c>"*"</c> matches everything (equivalent to <see cref="All"/>).
    /// </remarks>
    public string[] IncludedNamespaces
    {
        get;
        init
        {
            field = value;
            _compiledNamespaces = CompilePatterns(value);
        }
    } = [];

    /// <summary>
    ///     Return types excluded from eager evaluation even when the declaring type's namespace matches.
    ///     Excluded methods are deferred and available for force evaluation.
    ///     Defaults to <c>[typeof(void)]</c> so side-effect-only methods are never auto-invoked.
    /// </summary>
    public Type[] ExcludedReturnTypes { get; init; } = [typeof(void)];

    /// <summary>
    ///     A policy that defers all methods. No methods are evaluated eagerly.
    /// </summary>
    public static MethodEvaluationPolicy None { get; } = new();

    /// <summary>
    ///     A policy that evaluates all methods eagerly, except those whose return type is in
    ///     <see cref="ExcludedReturnTypes"/>.
    /// </summary>
    public static MethodEvaluationPolicy All { get; } = new() { IncludedNamespaces = ["*"] };

    /// <summary>
    ///     Check if the method is allowed to be evaluated during decomposition
    /// </summary>
    internal bool IsEvaluationAllowed(MethodInfo member, Type declaringType)
    {
        foreach (var returnType in ExcludedReturnTypes)
        {
            if (member.ReturnType == returnType) return false;
        }

        var memberNamespace = declaringType.Namespace ?? string.Empty;
        foreach (var compiled in _compiledNamespaces)
        {
            if (compiled.Matches(memberNamespace)) return true;
        }

        return false;
    }

    private static CompiledPattern[] CompilePatterns(string[] patterns)
    {
        if (patterns.Length == 0) return [];

        var compiled = new CompiledPattern[patterns.Length];
        for (var index = 0; index < patterns.Length; index++)
        {
            compiled[index] = new CompiledPattern(patterns[index]);
        }

        return compiled;
    }

    private readonly struct CompiledPattern(string pattern)
    {
        private readonly string? _prefix = pattern.Length > 0 && pattern[^1] == '*' && pattern.IndexOf('*') == pattern.Length - 1
            ? pattern[..^1]
            : null;

        public bool Matches(string input)
        {
            return _prefix is not null
                ? input.StartsWith(_prefix, StringComparison.Ordinal)
                : MatchesWildcardSegments(input, pattern);
        }
    }

    private static bool MatchesWildcardSegments(ReadOnlySpan<char> input, ReadOnlySpan<char> pattern)
    {
        var firstStar = pattern.IndexOf('*');
        if (firstStar < 0) return input.SequenceEqual(pattern);

        var prefix = pattern[..firstStar];
        if (!input.StartsWith(prefix, StringComparison.Ordinal)) return false;
        input = input[prefix.Length..];
        pattern = pattern[(firstStar + 1)..];

        while (true)
        {
            var nextStar = pattern.IndexOf('*');
            if (nextStar < 0)
            {
                return input.EndsWith(pattern, StringComparison.Ordinal);
            }

            var segment = pattern[..nextStar];
            if (!segment.IsEmpty)
            {
                var index = input.IndexOf(segment, StringComparison.Ordinal);
                if (index < 0) return false;
                input = input[(index + segment.Length)..];
            }

            pattern = pattern[(nextStar + 1)..];
        }
    }
}