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
[DependsOn<CompileProjectModule>(Optional = true)]
public sealed class TestProjectModule : Module<CommandResult>
{
    protected override async Task<CommandResult?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        return await context.DotNet().Test(new DotNetTestOptions
        {
            Project = Projects.LookupEngine_Tests_Unit.FullName,
            Configuration = "Release"
        }, cancellationToken: cancellationToken);
    }
}