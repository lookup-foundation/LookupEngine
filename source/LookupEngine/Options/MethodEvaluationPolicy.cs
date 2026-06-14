using System.Reflection;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

/// <summary>
///     Controls which methods are evaluated automatically during decomposition
/// </summary>
[PublicAPI]
public sealed class MethodEvaluationPolicy
{
    private readonly CompiledPattern[] _compiledNamespaces = [];

    /// <summary>
    ///     Namespace wildcard patterns of declaring types whose methods are evaluated automatically
    /// </summary>
    /// <remarks>
    ///     The <c>*</c> wildcard matches zero or more characters, matching is ordinal and case-sensitive.
    ///     Empty by default: no methods are evaluated, all are deferred
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
    ///     Return types excluded from automatic evaluation even when the namespace matches
    /// </summary>
    /// <remarks>
    ///     Excluded methods are deferred and available for force evaluation.
    ///     By default, methods without a return value are only executed explicitly
    /// </remarks>
    public Type[] ExcludedReturnTypes { get; init; } = [typeof(void)];

    /// <summary>
    ///     Defer evaluation of all methods
    /// </summary>
    public static MethodEvaluationPolicy None { get; } = new();

    /// <summary>
    ///     Evaluate all methods except the excluded return types
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

    /// <summary>
    ///     A namespace wildcard pattern parsed once when the policy is created.
    /// </summary>
    /// <remarks>
    ///     Trailing-star patterns (<c>Autodesk.Revit.*</c>, <c>*</c>) collapse to a single vectorized
    ///     prefix check; any other shape falls back to segment matching.
    /// </remarks>
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

    /// <summary>
    ///     Match the input against a pattern where <c>*</c> matches zero or more characters by splitting
    ///     the pattern on <c>*</c> and matching each literal segment with vectorized
    ///     <c>StartsWith</c>/<c>IndexOf</c>/<c>EndsWith</c>.
    /// </summary>
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