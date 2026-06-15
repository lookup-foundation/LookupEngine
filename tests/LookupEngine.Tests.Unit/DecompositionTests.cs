namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for core decomposition behavior.
/// </summary>
public sealed class DecompositionTests
{
    [Test]
    public async Task Decompose_NullValue_ReturnsNullableDecomposition()
    {
        // Act
        var result = LookupComposer.Decompose(null);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.Name).Contains(nameof(Object));
            await Assert.That(result.TypeName).IsEqualTo(nameof(Object));
            await Assert.That(result.TypeFullName).IsEqualTo("System.Object");
            await Assert.That(result.RawValue).IsNull();
            await Assert.That(result.Members).IsEmpty();
        }
    }

    [Test]
    public async Task Decompose_SimpleObject_ReturnsDecomposedStructure()
    {
        // Arrange
        var testObject = new {Name = "Test", Value = 42};

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.Members).Count().IsGreaterThanOrEqualTo(2);
            await Assert.That(result.Members[0].Name).IsEqualTo(nameof(testObject.Name));
            await Assert.That(result.Members[1].Name).IsEqualTo(nameof(testObject.Value));
        }
    }

    [Test]
    public async Task Decompose_PrimitiveTypes_ReturnsCorrectTypeName()
    {
        // Arrange
        var intValue = 42;
        var stringValue = "test";
        var boolValue = true;
        var doubleValue = 3.14;

        // Act
        var intResult = LookupComposer.Decompose(intValue);
        var stringResult = LookupComposer.Decompose(stringValue);
        var boolResult = LookupComposer.Decompose(boolValue);
        var doubleResult = LookupComposer.Decompose(doubleValue);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(intResult.TypeName).IsEqualTo(nameof(Int32));
            await Assert.That(stringResult.TypeName).IsEqualTo(nameof(String));
            await Assert.That(boolResult.TypeName).IsEqualTo(nameof(Boolean));
            await Assert.That(doubleResult.TypeName).IsEqualTo(nameof(Double));
        }
    }

    [Test]
    public async Task Decompose_ComplexObject_IncludesAllPublicMembers()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 11, 12, 30, 0);

        // Act
        var result = LookupComposer.Decompose(dateTime);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result.Members).IsNotEmpty();
            await Assert.That(result.Members.Any(member => member.Name == nameof(DateTime.Year))).IsTrue();
            await Assert.That(result.Members.Any(member => member.Name == nameof(DateTime.Month))).IsTrue();
            await Assert.That(result.Members.Any(member => member.Name == nameof(DateTime.Day))).IsTrue();
        }
    }

    [Test]
    public async Task DecomposeObject_ReturnsObjectWithoutMembers()
    {
        // Arrange
        var testObject = new {Name = "Test", Value = 42};

        // Act
        var result = LookupComposer.DecomposeObject(testObject);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.Members).IsEmpty();
            await Assert.That(result.RawValue).IsNotNull();
        }
    }

    [Test]
    public async Task DecomposeMembers_ReturnsOnlyMembers()
    {
        // Arrange
        var testObject = new {Name = "Test", Value = 42};

        // Act
        var result = LookupComposer.DecomposeMembers(testObject);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result).Count().IsGreaterThanOrEqualTo(2);
            await Assert.That(result[0].Name).IsEqualTo(nameof(testObject.Name));
            await Assert.That(result[1].Name).IsEqualTo(nameof(testObject.Value));
        }
    }

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
    public async Task Decompose_WithInheritance_IncludesBaseMembers()
    {
        // Arrange
        var exception = new ArgumentException("Test message", nameof(ArgumentException.ParamName));

        // Act
        var result = LookupComposer.Decompose(exception);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result.Members).IsNotEmpty();
            await Assert.That(result.Members.Any(member => member.Name == nameof(ArgumentException.Message))).IsTrue();
            await Assert.That(result.Members.Any(member => member.Name == nameof(ArgumentException.ParamName))).IsTrue();
        }
    }

    [Test]
    public async Task Decompose_GenericType_HandlesCorrectly()
    {
        // Arrange
        var list = new List<int> {1, 2, 3};

        // Act
        var result = LookupComposer.Decompose(list);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.TypeName).Contains(nameof(List<>));
            await Assert.That(result.Members).IsNotEmpty();
        }
    }

    [Test]
    public async Task Decompose_MemberDepth_IsCorrect()
    {
        // Arrange
        var testObject = new {Name = "Test"};

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        await Assert.That(result.Members[0].Depth).IsEqualTo(0);
    }

    [Test]
    public async Task Decompose_DeclaringTypeInfo_IsCorrect()
    {
        // Arrange
        var exception = new ArgumentException();

        // Act
        var result = LookupComposer.Decompose(exception);

        // Assert
        var messageMember = result.Members.First(member => member.Name == nameof(ArgumentException.Message));
        using (Assert.Multiple())
        {
            await Assert.That(messageMember.DeclaringTypeName).IsNotNullOrEmpty();
            await Assert.That(messageMember.DeclaringTypeFullName).Contains(nameof(System));
        }
    }

    [Test]
    public async Task Decompose_NullNamespaceType_TypeFullNameHasNoLeadingDot()
    {
        // Arrange - Anonymous types have null namespace
        var anonymous = new {Value = 42};

        // Act
        var result = LookupComposer.Decompose(anonymous);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result.TypeFullName).IsEqualTo(result.TypeName);
            await Assert.That(result.TypeFullName.StartsWith('.')).IsFalse();
            foreach (var member in result.Members)
            {
                await Assert.That(member.DeclaringTypeFullName.StartsWith('.')).IsFalse();
                await Assert.That(member.Value.TypeFullName.StartsWith('.')).IsFalse();
            }
        }
    }
}