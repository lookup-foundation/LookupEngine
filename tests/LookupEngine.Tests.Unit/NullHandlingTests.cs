using LookupEngine.Descriptors;

namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for null value behavior throughout decomposition.
/// </summary>
public sealed class NullHandlingTests
{
    [Test]
    public async Task Decompose_NullInput_ReturnsNullableDecomposition()
    {
        // Act
        var result = LookupComposer.Decompose(null);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.RawValue).IsNull();
            await Assert.That(result.TypeName).IsEqualTo(nameof(Object));
            await Assert.That(result.Members).IsEmpty();
        }
    }

    [Test]
    public async Task DecomposeObject_NullInput_ReturnsNullableDecomposition()
    {
        // Act
        var result = LookupComposer.DecomposeObject(null);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.RawValue).IsNull();
        }
    }

    [Test]
    public async Task DecomposeMembers_NullInput_ReturnsEmptyList()
    {
        // Act
        var result = LookupComposer.DecomposeMembers(null);

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task Decompose_PropertyWithNullValue_HandlesCorrectly()
    {
        // Arrange
        var testObject = new NullPropertyObject {Value = null};

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        await Assert.That(result.Members).IsNotEmpty();

        var valueMember = result.Members.First(member => member.Name == nameof(NullPropertyObject.Value));
        await Assert.That(valueMember.Value.RawValue).IsNull();
    }

    [Test]
    public async Task Decompose_FieldWithNullValue_HandlesCorrectly()
    {
        // Arrange
        var testObject = new NullFieldObject {Field = null};
        var options = new DecomposeOptions {IncludeFields = true};

        // Act
        var result = LookupComposer.Decompose(testObject, options);

        // Assert
        await Assert.That(result.Members).IsNotEmpty();

        var fieldMember = result.Members.First(member => member.Name == nameof(NullFieldObject.Field));
        await Assert.That(fieldMember.Value.RawValue).IsNull();
    }

    [Test]
    public async Task Decompose_NullableValueType_HandlesCorrectly()
    {
        // Arrange
        var testObject = new NullableValueTypeObject
        {
            NullableInt = null,
            HasValue = 42
        };

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        var nullMember = result.Members.First(member => member.Name == nameof(NullableValueTypeObject.NullableInt));
        var valueMember = result.Members.First(member => member.Name == nameof(NullableValueTypeObject.HasValue));

        using (Assert.Multiple())
        {
            await Assert.That(nullMember.Value.RawValue).IsNull();
            await Assert.That(valueMember.Value.RawValue).IsEqualTo(42);
        }
    }

    [Test]
    public async Task Decompose_ObjectWithAllNullProperties_Succeeds()
    {
        // Arrange
        var testObject = new AllNullObject
        {
            Property1 = null,
            Property2 = null,
            Property3 = null
        };

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.Members).Count().IsEqualTo(3);
            foreach (var member in result.Members)
            {
                await Assert.That(member.Value.RawValue).IsNull();
            }
        }
    }

    [Test]
    public async Task Decompose_NullInEnumerable_HandlesGracefully()
    {
        // Arrange
        var list = new List<string?> {"First", null, "Third"};

        // Act
        var result = LookupComposer.Decompose(list);

        // Assert
        await Assert.That(result).IsNotNull();
    }

    [Test]
    public async Task Decompose_NullDescriptorName_UsesTypeNameFallback()
    {
        // Arrange
        var testObject = new NullDescriptorNameObject();
        var options = new DecomposeOptions
        {
            TypeResolver = (obj, _) => obj switch
            {
                NullDescriptorNameObject => new NullNameDescriptor(),
                _ => new ObjectDescriptor(obj)
            }
        };

        // Act
        var result = LookupComposer.Decompose(testObject, options);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result.Name).IsNotNullOrEmpty();
            await Assert.That(result.TypeName).IsNotNullOrEmpty();
        }
    }

    [Test]
    public async Task Decompose_NullNamespace_HandlesGracefully()
    {
        // Arrange - Anonymous types have null namespace
        var anonymous = new {Value = 42};

        // Act
        var result = LookupComposer.Decompose(anonymous);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.TypeFullName).IsNotNullOrEmpty();
        }
    }
}

// Test helper classes
file sealed class NullPropertyObject
{
    public string? Value { get; set; }
}

file sealed class NullFieldObject
{
    public string? Field;
}

file sealed class NullableValueTypeObject
{
    public int? NullableInt { get; set; }
    public int? HasValue { get; set; }
}

file sealed class AllNullObject
{
    public string? Property1 { get; set; }
    public string? Property2 { get; set; }
    public string? Property3 { get; set; }
}

file sealed class NullDescriptorNameObject
{
}

file sealed class NullNameDescriptor : LookupEngine.Abstractions.Decomposition.Descriptor
{
    // Name intentionally not set (will be null)
}