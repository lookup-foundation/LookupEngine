namespace Build.Options;

[PublicAPI]
public sealed record PublishOptions
{
    public string? Version { get; init; }
    public string ChangelogFile { get; init; } = "CHANGELOG.md";
}
