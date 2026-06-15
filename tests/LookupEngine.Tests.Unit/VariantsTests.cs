using LookupEngine.Abstractions.Decomposition;

namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for <see cref="IVariant"/> factory methods and variant behavior.
/// </summary>
public sealed class VariantsTests
{
    [Test]
    public async Task Variants_Value_CreatesSingleVariant()
    {
        // Act
        var variant = Variants.Value("Test");

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(variant).IsNotNull();
            await Assert.That(variant.Value).IsEqualTo("Test");
            await Assert.That(variant.Description).IsNull();
        }
    }

    [Test]
    public async Task Variants_ValueWithDescription_IncludesDescription()
    {
        // Act
        var variant = Variants.Value("Test", "Test description");

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(variant.Value).IsEqualTo("Test");
            await Assert.That(variant.Description).IsEqualTo("Test description");
        }
    }

    [Test]
    public async Task Variants_Values_CreatesMultipleVariants()
    {
        // Act
        var variants = Variants.Values<string>(2)
            .Add("First")
            .Add("Second")
            .Consume();

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(variants).IsNotNull();
            await Assert.That(variants.Value).IsNotNull();
        }
    }

    [Test]
    public async Task Variants_ValuesWithDescriptions_IncludesDescriptions()
    {
        // Act
        var variants = Variants.Values<string>(2)
            .Add("First", "First description")
            .Add("Second", "Second description")
            .Consume();

        // Assert
        await Assert.That(variants).IsNotNull();
    }

    [Test]
    public async Task Variants_EmptyCollection_HandlesCorrectly()
    {
        // Act
        var variants = Variants.Empty<string>();

        // Assert
        await Assert.That(variants).IsNotNull();
    }

    [Test]
    public async Task Variants_AddNull_SkipsNull()
    {
        // Act
        var variants = Variants.Values<string>(2)
            .Add(null)
            .Add("Valid")
            .Consume();

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(variants).IsNotNull();
            await Assert.That(variants.Value).IsNotNull();
        }
    }

    [Test]
    public async Task Variants_AddEmptyCollection_SkipsEmpty()
    {
        // Act
        var variants = Variants.Values<List<int>>(2)
            .Add(new List<int>())
            .Add(new List<int> { 1 })
            .Consume();

        // Assert
        await Assert.That(variants).IsNotNull();
    }

    [Test]
    public async Task Variants_SingleVariantInCollection_ReturnsSingleValue()
    {
        // Act
        var variants = Variants.Values<string>(1)
            .Add("Only")
            .Consume();

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(variants.Value).IsEqualTo("Only");
            // Single variant should expose its description
            await Assert.That(variants.Description).IsNull();
        }
    }

    [Test]
    public async Task Variants_SingleVariantWithDescription_PreservesDescription()
    {
        // Act
        var variants = Variants.Values<string>(1)
            .Add("Only", "Description")
            .Consume();

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(variants.Value).IsEqualTo("Only");
            await Assert.That(variants.Description).IsEqualTo("Description");
        }
    }

    [Test]
    public async Task Variants_MultipleVariants_ReturnsCollection()
    {
        // Act
        var variants = Variants.Values<string>(2)
            .Add("First")
            .Add("Second")
            .Consume();

        // Assert
        using (Assert.Multiple())
        {
            // Multiple variants return the collection itself as Value
            await Assert.That(variants.Value).IsNotNull();
            await Assert.That(variants.Description).IsNull();
        }
    }
}
