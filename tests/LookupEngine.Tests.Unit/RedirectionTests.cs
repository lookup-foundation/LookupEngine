using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Descriptors;
using LookupEngine.Options;

namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for <see cref="IDescriptorRedirector"/> functionality and value redirection behavior.
/// </summary>
public sealed class RedirectionTests
{
    [Test]
    public async Task Decompose_ExcludingRedirection_ValueStaysOriginalType()
    {
        //Arrange
        var data = new RedirectContainerObject();
        var options = new DecomposeOptions
        {
            EnableRedirection = false,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    RedirectableObject => new RedirectionDescriptor(),
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
            await Assert.That(defaultResult.Members).IsNotEmpty();
            await Assert.That(comparableResult.Members).IsNotEmpty();
            await Assert.That(comparableResult.Members[0].Value.TypeName).IsEqualTo(defaultResult.Members[0].Value.TypeName);
        }
    }

    [Test]
    public async Task Decompose_IncludingRedirection_RedirectedToAnotherValue()
    {
        //Arrange
        var data = new RedirectContainerObject();
        var options = new DecomposeOptions
        {
            EnableRedirection = true,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    RedirectableObject => new RedirectionDescriptor(),
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
            await Assert.That(defaultResult.Members).IsNotEmpty();
            await Assert.That(comparableResult.Members).IsNotEmpty();
            await Assert.That(comparableResult.Members[0].Value.TypeName).IsNotEqualTo(defaultResult.Members[0].Value.TypeName);
        }
    }

    [Test]
    public async Task Decompose_IncludingContextRedirection_RedirectedToAnotherValue()
    {
        //Arrange
        var context = new EngineTestContext();
        var data = new RedirectContainerObject();
        var options = new DecomposeOptions
        {
            EnableRedirection = true,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    RedirectableObject => new RedirectionDescriptor(),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        var contextOptions = new DecomposeOptions<EngineTestContext>
        {
            Context = context,
            EnableRedirection = true,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    RedirectableObject => new RedirectionDescriptor(),
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
            await Assert.That(defaultResult.Members).IsNotEmpty();
            await Assert.That(comparableResult.Members).IsNotEmpty();
            await Assert.That(comparableContextResult.Members).IsNotEmpty();
            await Assert.That(comparableContextResult.Members[0].Value.TypeName).IsNotEqualTo(defaultResult.Members[0].Value.TypeName);
            await Assert.That(comparableContextResult.Members[0].Value.TypeName).IsNotEqualTo(comparableResult.Members[0].Value.TypeName);
        }
    }
    [Test]
    public async Task Decompose_CyclicRedirection_Terminates()
    {
        //Arrange
        var data = new CycleContainerObject();
        var options = new DecomposeOptions
        {
            EnableRedirection = true,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    CycleFirstObject => new CycleFirstDescriptor(),
                    CycleSecondObject => new CycleSecondDescriptor(),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.Members).IsNotEmpty();
        }
    }

    [Test]
    public async Task Decompose_CyclicContextRedirection_Terminates()
    {
        //Arrange
        var data = new CycleContainerObject();
        var contextOptions = new DecomposeOptions<EngineTestContext>
        {
            Context = new EngineTestContext(),
            EnableRedirection = true,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    CycleFirstObject => new CycleContextDescriptor(),
                    CycleSecondObject => new CycleContextDescriptor(),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, contextOptions);

        //Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.Members).IsNotEmpty();
        }
    }
}

file sealed class RedirectContainerObject
{
    public RedirectableObject PropertyToRedirect => new();
}

file sealed class CycleContainerObject
{
    public CycleFirstObject PropertyToRedirect => new();
}

file sealed class CycleFirstObject;

file sealed class CycleSecondObject;

file sealed class CycleFirstDescriptor : Descriptor, IDescriptorRedirector
{
    public bool TryRedirect(string target, out object result)
    {
        result = new CycleSecondObject();
        return true;
    }
}

file sealed class CycleSecondDescriptor : Descriptor, IDescriptorRedirector
{
    public bool TryRedirect(string target, out object result)
    {
        result = new CycleFirstObject();
        return true;
    }
}

file sealed class CycleContextDescriptor : Descriptor, IDescriptorRedirector<EngineTestContext>
{
    public bool TryRedirect(string target, EngineTestContext context, out object result)
    {
        result = new CycleSecondObject();
        return true;
    }
}

file sealed class RedirectableObject
{
    public Random Random { get; set; } = new(69);
}

file sealed class EngineTestContext
{
    public int Version { get; } = 1;
    public string Metadata { get; } = "Test context";
}

file sealed class RedirectionDescriptor : Descriptor, IDescriptorRedirector, IDescriptorRedirector<EngineTestContext>
{
    public RedirectionDescriptor()
    {
        Name = "Redirection";
    }

    public bool TryRedirect(string target, out object result)
    {
        result = 69;
        return true;
    }

    public bool TryRedirect(string target, EngineTestContext context, out object result)
    {
        result = context.Version switch
        {
            1 => $"Target: {target}, context: {context.Metadata}",
            _ => $"Target: {target}, context: {context.Version}"
        };

        return true;
    }
}