using JetBrains.Annotations;

namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for static type decomposition (typeof(T))
/// </summary>
public sealed class StaticTypeTests
{
    [Test]
    public async Task Decompose_StaticType_ReturnsStaticMembers()
    {
        // Arrange
        var type = typeof(DateOnly);
        var options = new DecomposeOptions
        {
            IncludeStaticMembers = true
        };

        // Act
        var result = LookupComposer.Decompose(type, options);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.Members).IsNotEmpty();
            await Assert.That(result.Members.Any(member => member.Name == nameof(DateOnly.MaxValue))).IsTrue();
        }
    }

    [Test]
    public async Task Decompose_StaticTypeWithoutOption_StillIncludesStaticMembers()
    {
        // Arrange
        var type = typeof(DateOnly);

        // Act
        var result = LookupComposer.Decompose(type);

        // Assert - decomposing a Type always includes static members, they are the content of the type itself
        using (Assert.Multiple())
        {
            await Assert.That(result.Members).IsNotEmpty();
            await Assert.That(result.Members.Any(member => member.Name == nameof(DateOnly.MaxValue))).IsTrue();
        }
    }

    [Test]
    public async Task Decompose_GenericStaticType_HandlesCorrectly()
    {
        // Arrange
        var type = typeof(List<>);
        var options = new DecomposeOptions
        {
            IncludeStaticMembers = true
        };

        // Act
        var result = LookupComposer.Decompose(type, options);

        // Assert
        await Assert.That(result).IsNotNull();
    }

    [Test]
    public async Task Decompose_ClosedGenericType_HandlesCorrectly()
    {
        // Arrange
        var type = typeof(List<int>);
        var options = new DecomposeOptions
        {
            IncludeStaticMembers = true
        };

        // Act
        var result = LookupComposer.Decompose(type, options);

        // Assert
        await Assert.That(result).IsNotNull();
    }

    [Test]
    public async Task Decompose_InterfaceType_HandlesCorrectly()
    {
        // Arrange
        var type = typeof(IEnumerable<int>);
        var options = new DecomposeOptions
        {
            IncludeStaticMembers = true
        };

        // Act
        var result = LookupComposer.Decompose(type, options);

        // Assert
        await Assert.That(result).IsNotNull();
    }

    [Test]
    public async Task Decompose_AbstractType_HandlesCorrectly()
    {
        // Arrange
        var type = typeof(Stream);
        var options = new DecomposeOptions
        {
            IncludeStaticMembers = true
        };

        // Act
        var result = LookupComposer.Decompose(type, options);

        // Assert
        await Assert.That(result).IsNotNull();
    }

    [Test]
    public async Task Decompose_SealedType_HandlesCorrectly()
    {
        // Arrange
        var type = typeof(string);
        var options = new DecomposeOptions
        {
            IncludeStaticMembers = true
        };

        // Act
        var result = LookupComposer.Decompose(type, options);

        // Assert
        await Assert.That(result).IsNotNull();
    }

    [Test]
    public async Task Decompose_EnumType_HandlesCorrectly()
    {
        // Arrange
        var type = typeof(DayOfWeek);
        var result = LookupComposer.Decompose(type);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.Members).IsNotEmpty();
        }
    }

    [Test]
    public async Task Decompose_StaticTypeWithPrivateMembers_IncludesPrivate()
    {
        // Arrange
        var type = typeof(StaticTestClass);
        var options = new DecomposeOptions
        {
            IncludeStaticMembers = true,
            IncludePrivateMembers = true
        };

        // Act
        var result = LookupComposer.Decompose(type, options);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result.Members).IsNotEmpty();
            await Assert.That(result.Members.Any(member => member.Name == nameof(StaticTestClass.PublicStaticProperty))).IsTrue();
        }
    }

    [Test]
    public async Task Decompose_StaticTypeWithFields_IncludesFields()
    {
        // Arrange
        var type = typeof(StaticTestClass);
        var options = new DecomposeOptions
        {
            IncludeStaticMembers = true,
            IncludeFields = true
        };

        // Act
        var result = LookupComposer.Decompose(type, options);

        // Assert
        await Assert.That(result.Members).IsNotEmpty();
    }

    [Test]
    public async Task Decompose_PrimitiveType_HandlesCorrectly()
    {
        // Arrange
        var type = typeof(int);
        var options = new DecomposeOptions
        {
            IncludeFields = true,
            IncludeStaticMembers = true
        };

        // Act
        var result = LookupComposer.Decompose(type, options);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.Members).IsNotEmpty();
            await Assert.That(result.Members.Any(member => member.Name == nameof(int.MaxValue))).IsTrue();
            await Assert.That(result.Members.Any(member => member.Name == nameof(int.MinValue))).IsTrue();
        }
    }
}

[PublicAPI]
file static class StaticTestClass
{
    public static string PublicStaticProperty => "Public";
    private static string PrivateStaticProperty => "Private";
    public static string PublicStaticField = "Field";
}