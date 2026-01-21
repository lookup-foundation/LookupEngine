using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.GitHub.Extensions;
using ModularPipelines.Modules;

namespace Build.Modules;

/// <summary>
///     Generate and format the changelog for publishing on the GitHub.
/// </summary>
[DependsOn<GenerateChangelogModule>]
[DependsOn<ResolveBuildVersionModule>]
public sealed class GenerateGitHubChangelogModule : Module<string>
{
    protected override async Task<string?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var versioningResult = await context.GetModule<ResolveBuildVersionModule>();
        var changelogResult = await context.GetModule<GenerateChangelogModule>();
        var versioning = versioningResult.ValueOrDefault!;
        var changelog = changelogResult.ValueOrDefault!;
        if (changelog.Length == 0) return changelog;

        return AppendGitHubCompareUrl(context, changelog, versioning);
    }

    /// <summary>
    ///     Append a GitHub compare URL to the changelog if it is not already included.
    /// </summary>
    private static string AppendGitHubCompareUrl(IPipelineContext context, string changelog, ResolveVersioningResult versioning)
    {
        if (changelog.Contains("Full changelog", StringComparison.OrdinalIgnoreCase)) return changelog;

        var repositoryInfo = context.GitHub().RepositoryInfo;
        var url = $"https://github.com/{repositoryInfo.Identifier}/compare/{versioning.PreviousVersion}...{versioning.Version}";

        return $"{changelog}{Environment.NewLine}{Environment.NewLine}**Full changelog**: {url}";
    }
}