using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using Sourcy.DotNet;

namespace Build.Modules;

/// <summary>
///     Test the project.
/// </summary>
public sealed class TestProjectModule : Module<CommandResult>
{
    protected override async Task<CommandResult?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        return await context.DotNet().Test(new DotNetTestOptions
        {
            Configuration = Configuration.Release,
            Verbosity = Verbosity.Minimal,
            Arguments = ["--project", Projects.LookupEngine_Tests_Unit.FullName]
        }, cancellationToken);
    }
}