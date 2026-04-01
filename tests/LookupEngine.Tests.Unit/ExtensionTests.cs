using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Descriptors;
using LookupEngine.Options;

namespace LookupEngine.Tests.Unit;

/// <summary>
/// Tests for <see cref="IDescriptorExtension"/> functionality and context data enrichment.
/// </summary>
public sealed class ExtensionTests
{
    [Test]
    public async Task Decompose_IncludingExtensions_ExtensionHandled()
    {
        //Arrange
        var data = new ExtensibleObject();
        var options = new DecomposeOptions
        {
            EnableExtensions = true,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    ExtensibleObject => new ExtensionDescriptor(),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        //Act
        var defaultResult = LookupComposer.Decompose(data);
        var comparableResult = LookupComposer.Decompose(data, options);

        //Assert
        using (Assert.Multiple())
        {
            await Assert.That(defaultResult.Members).IsEmpty();
            await Assert.That(comparableResult.Members).IsNotEmpty();
            await Assert.That(comparableResult.Members[0].AllocatedBytes).IsPositive();
            await Assert.That(comparableResult.Members[0].ComputationTime).IsPositive();
        }
    }

    [Test]
    public async Task Decompose_IncludingContextExtensions_ExtensionHandled()
    {
        //Arrange
        var data = new ExtensibleObject();
        var context = new EngineTestContext();
        var options = new DecomposeOptions
        {
            EnableExtensions = true,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    ExtensibleObject => new ExtensionDescriptor(),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        var contextOptions = new DecomposeOptions<EngineTestContext>
        {
            Context = context,
            EnableExtensions = true,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    ExtensibleObject => new ExtensionDescriptor(),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        //Act
        var defaultResult = LookupComposer.Decompose(data);
        var comparableResult = LookupComposer.Decompose(data, options);
        var comparableContextResult = LookupComposer.Decompose(data, contextOptions);

        //Assert
        using (Assert.Multiple())
        {
            await Assert.That(defaultResult.Members).IsEmpty();
            await Assert.That(comparableResult.Members).IsNotEmpty();
            await Assert.That(comparableContextResult.Members).IsNotEmpty();
            await Assert.That(comparableContextResult.Members[0].AllocatedBytes).IsPositive();
            await Assert.That(comparableContextResult.Members[0].ComputationTime).IsPositive();
            await Assert.That(comparableContextResult.Members.Count).IsGreaterThan(comparableResult.Members.Count);
        }
    }
}

file sealed class ExtensibleObject;

file sealed class EngineTestContext
{
    public int Version { get; } = 1;
    public string Metadata { get; } = "Test context";
}

file sealed class ExtensionDescriptor : Descriptor, IDescriptorExtension, IDescriptorExtension<EngineTestContext>
{
    public void RegisterExtensions(IExtensionManager manager)
    {
        manager.Define("Extension").Register(() => Variants.Value("Extended"));
    }

    public void RegisterExtensions(IExtensionManager<EngineTestContext> manager)
    {
        manager.Define("VersionExtension").Register(context => Variants.Value(context.Version));
        manager.Define("MetadataExtension").Register(context => Variants.Value(context.Metadata));
    }
}