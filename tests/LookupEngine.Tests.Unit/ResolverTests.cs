using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Descriptors;
using LookupEngine.Options;

namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for <see cref="IDescriptorConfigurator"/> member resolution and custom type resolution.
/// </summary>
public sealed class ResolverTests
{
    [Test]
    public async Task Decompose_IncludingUnresolvedData_ResolvedData()
    {
        //Arrange
        var data = new ResolvableObject();
        var options = new DecomposeOptions
        {
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    ResolvableObject => new ResolverDescriptor(),
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
        }
    }

    [Test]
    public async Task Decompose_IncludingUnresolvedContextData_ResolvedData()
    {
        //Arrange
        var data = new ResolvableObject();
        var context = new EngineTestContext();
        var options = new DecomposeOptions
        {
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    ResolvableObject => new ResolverDescriptor(),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        var contextOptions = new DecomposeOptions<EngineTestContext>
        {
            Context = context,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    ResolvableObject => new ResolverDescriptor(),
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
            await Assert.That(comparableContextResult.Members.Count).IsGreaterThan(comparableResult.Members.Count);
        }
    }
    [Test]
    public async Task Decompose_SharedValueDescriptor_DescriptionDoesNotLeakBetweenMembers()
    {
        //Arrange
        var data = new DescribedContainerObject();
        var sharedDescriptor = new SharedValueDescriptor();
        var options = new DecomposeOptions
        {
            EvaluationPolicy = MethodEvaluationPolicy.All,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    DescribedContainerObject => new DescribingResolverDescriptor(),
                    DescribedValueObject => sharedDescriptor,
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var describedMember = result.Members.First(member => member.Name.StartsWith(nameof(DescribedContainerObject.DescribedMethod)));
        var plainMember = result.Members.First(member => member.Name.StartsWith(nameof(DescribedContainerObject.PlainMethod)));
        using (Assert.Multiple())
        {
            await Assert.That(describedMember.Value.Description).IsEqualTo("Variant description");
            await Assert.That(plainMember.Value.Description).IsNull();
            await Assert.That(sharedDescriptor.Description).IsNull();
        }
    }
}

file sealed class DescribedContainerObject
{
    public DescribedValueObject DescribedMethod(int parameter)
    {
        return new DescribedValueObject();
    }

    public DescribedValueObject PlainMethod(int parameter)
    {
        return new DescribedValueObject();
    }
}

file sealed class DescribedValueObject;

file sealed class SharedValueDescriptor : Descriptor;

file sealed class DescribingResolverDescriptor : Descriptor, IDescriptorConfigurator
{
    public void Configure(IMemberConfigurator configuration)
    {
        configuration.Member(nameof(DescribedContainerObject.DescribedMethod)).Resolve(() => Variants.Value(new DescribedValueObject(), "Variant description"));
        configuration.Member(nameof(DescribedContainerObject.PlainMethod)).Resolve(() => Variants.Value(new DescribedValueObject()));
    }
}

file sealed class ResolvableObject
{
    public string UnsupportedMethod(int parameter)
    {
        return parameter.ToString();
    }

    public string UnsupportedDescribedMethod(int parameter)
    {
        return parameter.ToString();
    }

    public string UnsupportedMultiMethod(int parameter)
    {
        return parameter.ToString();
    }
}

file sealed class EngineTestContext
{
    public int Version { get; } = 1;
    public string Metadata { get; } = "Test context";
}

file sealed class ResolverDescriptor : Descriptor, IDescriptorConfigurator, IDescriptorConfigurator<EngineTestContext>
{
    public ResolverDescriptor()
    {
        Name = "Redirection";
    }

    public void Configure(IMemberConfigurator configuration)
    {
        configuration.Member(nameof(ResolvableObject.UnsupportedMethod)).Resolve(() => Variants.Value("Resolved"));
        configuration.Member(nameof(ResolvableObject.UnsupportedDescribedMethod)).Resolve(() => Variants.Value("Resolved", "Value description"));
    }

    public void Configure(IMemberConfigurator<EngineTestContext> configuration)
    {
        configuration.Member(nameof(ResolvableObject.UnsupportedMultiMethod)).Resolve(_ => Variants.Values<string>(2)
            .Add("Resolved 1")
            .Add("Resolved 2", "Value description")
            .Consume());
    }
}