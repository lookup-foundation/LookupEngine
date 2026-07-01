using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Abstractions.Enums;
using LookupEngine.Descriptors;
using LookupEngine.Options;

namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for <see cref="IDescriptorConfigurator"/> extension functionality and context data enrichment.
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
    public async Task Decompose_PlainObjectExtension_EquivalentToVariantExtension()
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
                    ExtensibleObject => new EquivalenceExtensionDescriptor(),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, options);
        var plainMember = result.Members.Single(member => member.Name == "PlainExtension");
        var variantMember = result.Members.Single(member => member.Name == "VariantExtension");

        //Assert
        using (Assert.Multiple())
        {
            await Assert.That(plainMember.Value.RawValue).IsEqualTo(variantMember.Value.RawValue);
            await Assert.That(plainMember.Value.Name).IsEqualTo(variantMember.Value.Name);
            await Assert.That(plainMember.Value.TypeName).IsEqualTo(variantMember.Value.TypeName);
            await Assert.That(plainMember.Value.RawValue).IsEqualTo("Extended");
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

    [Test]
    public async Task Decompose_StaticExtension_HiddenUnlessIncludeStaticMembers()
    {
        //Arrange
        var data = new ExtensibleObject();
        var hiddenOptions = CreateExtensionOptions(configuration => configuration.Extension("StaticExtension").AsStatic().Register(() => "Value"));
        var includedOptions = CreateExtensionOptions(configuration => configuration.Extension("StaticExtension").AsStatic().Register(() => "Value"));
        includedOptions.IncludeStaticMembers = true;

        //Act
        var hiddenResult = LookupComposer.Decompose(data, hiddenOptions);
        var includedResult = LookupComposer.Decompose(data, includedOptions);

        //Assert
        var decomposedMember = includedResult.Members.Single(member => member.Name == "StaticExtension");
        using (Assert.Multiple())
        {
            await Assert.That(hiddenResult.Members.Where(member => member.Name == "StaticExtension")).IsEmpty();
            await Assert.That(decomposedMember.MemberAttributes).HasFlag(MemberAttributes.Static);
            await Assert.That(decomposedMember.MemberAttributes).HasFlag(MemberAttributes.Extension);
            await Assert.That(decomposedMember.Value.RawValue).IsEqualTo("Value");
        }
    }

    [Test]
    public async Task Decompose_DeferredExtension_EvaluatesOnForce()
    {
        //Arrange
        var data = new ExtensibleObject();
        var options = CreateExtensionOptions(configuration => configuration.Extension("DeferredExtension").Defer(() => "Computed"));

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.Single(member => member.Name == "DeferredExtension");
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
            await Assert.That(member.Value.RawValue).IsNull();
            await Assert.That(member.Evaluator).IsNotNull();
        }

        member.Evaluate();
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Value.RawValue).IsEqualTo("Computed");
            await Assert.That(member.ComputationTime).IsGreaterThanOrEqualTo(0);
        }
    }

    [Test]
    public async Task Decompose_DeferredVoidExtension_InvokesActionOnForce()
    {
        //Arrange
        var data = new ExtensibleObject();
        var invoked = false;
        var options = CreateExtensionOptions(configuration => configuration.Extension("VoidExtension").Defer(() => { invoked = true; }));

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.Single(member => member.Name == "VoidExtension");
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
            await Assert.That(member.Value.RawValue).IsNull();
            await Assert.That(member.Evaluator).IsNotNull();
            await Assert.That(invoked).IsFalse();
        }

        member.Evaluate();
        using (Assert.Multiple())
        {
            await Assert.That(invoked).IsTrue();
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Value.RawValue).IsNull();
        }
    }

    [Test]
    public async Task Decompose_DeferredVoidContextExtension_InvokesActionWithContextOnForce()
    {
        //Arrange
        var data = new ExtensibleObject();
        var observedVersion = 0;
        var options = new DecomposeOptions<EngineTestContext>
        {
            Context = new EngineTestContext(),
            EnableExtensions = true,
            TypeResolver = (obj, _) => obj switch
            {
                ExtensibleObject => new DelegatingContextConfigurator(configuration => configuration.Extension("VoidContextExtension").Defer(context => { observedVersion = context.Version; })),
                _ => new ObjectDescriptor(obj)
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.Single(member => member.Name == "VoidContextExtension");
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
            await Assert.That(observedVersion).IsEqualTo(0);
        }

        member.Evaluate();
        using (Assert.Multiple())
        {
            await Assert.That(observedVersion).IsEqualTo(1);
            await Assert.That(member.Value.RawValue).IsNull();
        }
    }

    [Test]
    public async Task Decompose_DisabledExtension_HiddenUnlessIncludeUnsupported()
    {
        //Arrange
        var data = new ExtensibleObject();
        var hiddenOptions = CreateExtensionOptions(configuration => configuration.Extension("DisabledExtension").Disable());
        var includedOptions = CreateExtensionOptions(configuration => configuration.Extension("DisabledExtension").Disable());
        includedOptions.IncludeUnsupported = true;

        //Act
        var hiddenResult = LookupComposer.Decompose(data, hiddenOptions);
        var includedResult = LookupComposer.Decompose(data, includedOptions);

        //Assert
        var decomposedMember = includedResult.Members.Single(member => member.Name == "DisabledExtension");
        using (Assert.Multiple())
        {
            await Assert.That(hiddenResult.Members.Where(member => member.Name == "DisabledExtension")).IsEmpty();
            await Assert.That(decomposedMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Disabled);
            await Assert.That(decomposedMember.Evaluator).IsNull();
            await Assert.That(decomposedMember.Value.RawValue).IsNull();
            await Assert.That(decomposedMember.Evaluate).Throws<InvalidOperationException>();
            await Assert.That(decomposedMember.Value.RawValue).IsNull();
        }
    }

    [Test]
    public async Task Decompose_NotSupportedExtension_HiddenUnlessIncludeUnsupported()
    {
        //Arrange
        var data = new ExtensibleObject();
        var hiddenOptions = CreateExtensionOptions(configuration => configuration.Extension("UnsupportedExtension").NotSupported());
        var includedOptions = CreateExtensionOptions(configuration => configuration.Extension("UnsupportedExtension").NotSupported());
        includedOptions.IncludeUnsupported = true;

        //Act
        var hiddenResult = LookupComposer.Decompose(data, hiddenOptions);
        var includedResult = LookupComposer.Decompose(data, includedOptions);

        //Assert
        var decomposedMember = includedResult.Members.Single(member => member.Name == "UnsupportedExtension");
        using (Assert.Multiple())
        {
            await Assert.That(hiddenResult.Members.Where(member => member.Name == "UnsupportedExtension")).IsEmpty();
            await Assert.That(decomposedMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Unsupported);
            await Assert.That(decomposedMember.Evaluator).IsNull();
        }
    }

    [Test]
    public async Task Decompose_MappedStaticDeferredExtension_CombinesModifiersAndTerminal()
    {
        //Arrange
        var data = new ExtensibleObject();
        var options = CreateExtensionOptions(configuration => configuration
            .Extension("CombinedExtension")
            .AsStatic()
            .Map("ApiMember")
            .Defer(() => Variants.Value("Combined")));
        options.IncludeStaticMembers = true;

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.Single(member => member.Name == "CombinedExtension");
        using (Assert.Multiple())
        {
            await Assert.That(member.MemberAttributes).HasFlag(MemberAttributes.Static);
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
        }

        member.Evaluate();
        await Assert.That(member.Value.RawValue).IsEqualTo("Combined");
    }

    [Test]
    public async Task Decompose_DeferredContextExtension_InvokesHandlerWithContext()
    {
        //Arrange
        var data = new ExtensibleObject();
        var options = new DecomposeOptions<EngineTestContext>
        {
            Context = new EngineTestContext(),
            EnableExtensions = true,
            TypeResolver = (obj, _) => obj switch
            {
                ExtensibleObject => new DelegatingContextConfigurator(configuration => configuration.Extension("ContextDeferred").Defer(context => context.Version)),
                _ => new ObjectDescriptor(obj)
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.Single(member => member.Name == "ContextDeferred");
        await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);

        member.Evaluate();
        await Assert.That(member.Value.RawValue).IsEqualTo(1);
    }

    private static DecomposeOptions CreateExtensionOptions(Action<IMemberConfigurator> configure)
    {
        return new DecomposeOptions
        {
            EnableExtensions = true,
            TypeResolver = (obj, _) => obj switch
            {
                ExtensibleObject => new DelegatingConfigurator(configure),
                _ => new ObjectDescriptor(obj)
            }
        };
    }
}

file sealed class ExtensibleObject;

file sealed class DelegatingConfigurator(Action<IMemberConfigurator> configure) : Descriptor, IDescriptorConfigurator
{
    public void Configure(IMemberConfigurator configuration)
    {
        configure.Invoke(configuration);
    }
}

file sealed class DelegatingContextConfigurator(Action<IMemberConfigurator<EngineTestContext>> configure) : Descriptor, IDescriptorConfigurator<EngineTestContext>
{
    public void Configure(IMemberConfigurator<EngineTestContext> configuration)
    {
        configure.Invoke(configuration);
    }
}

file sealed class EquivalenceExtensionDescriptor : Descriptor, IDescriptorConfigurator
{
    public void Configure(IMemberConfigurator configuration)
    {
        configuration.Extension("PlainExtension").Register(() => "Extended");
        configuration.Extension("VariantExtension").Register(() => Variants.Value("Extended"));
    }
}

file sealed class EngineTestContext
{
    public int Version { get; } = 1;
    public string Metadata { get; } = "Test context";
}

file sealed class ExtensionDescriptor : Descriptor, IDescriptorConfigurator, IDescriptorConfigurator<EngineTestContext>
{
    public void Configure(IMemberConfigurator configuration)
    {
        configuration.Extension("Extension").Register(() => Variants.Value("Extended"));
    }

    public void Configure(IMemberConfigurator<EngineTestContext> configuration)
    {
        configuration.Extension("VersionExtension").Register(context => Variants.Value(context.Version));
        configuration.Extension("MetadataExtension").Register(context => Variants.Value(context.Metadata));
    }
}