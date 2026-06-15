namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for exception capture and error behavior.
/// </summary>
public sealed class ErrorHandlingTests
{
    [Test]
    public async Task Decompose_PropertyThrowsException_ExceptionBecomesValue()
    {
        // Arrange
        var testObject = new ThrowingPropertyObject();

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        await Assert.That(result.Members).IsNotEmpty();
        var throwingMember = result.Members.First(member => member.Name == nameof(ThrowingPropertyObject.ThrowingProperty));

        using (Assert.Multiple())
        {
            await Assert.That(throwingMember.Value.TypeName).Contains(nameof(Exception));
            await Assert.That(throwingMember.Value.RawValue).IsTypeOf<InvalidOperationException>();
        }
    }

    [Test]
    public async Task Decompose_MethodThrowsException_ExceptionBecomesValue()
    {
        // Arrange
        var testObject = new ThrowingMethodObject();
        var options = new DecomposeOptions
        {
            EvaluationPolicy = MethodEvaluationPolicy.All
        };

        // Act
        var result = LookupComposer.Decompose(testObject, options);

        // Assert
        await Assert.That(result.Members).IsNotEmpty();
        var throwingMember = result.Members.First(member => member.Name == nameof(ThrowingMethodObject.ThrowingMethod));

        await Assert.That(throwingMember.Value.TypeName).Contains(nameof(Exception));
    }

    [Test]
    public async Task Decompose_TargetInvocationException_UnwrapsInnerException()
    {
        // Arrange
        var testObject = new ThrowingPropertyObject();

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        var throwingMember = result.Members.First(member => member.Name == nameof(ThrowingPropertyObject.ThrowingProperty));

        // Should be InvalidOperationException, not TargetInvocationException
        await Assert.That(throwingMember.Value.RawValue).IsTypeOf<InvalidOperationException>();
    }

    [Test]
    public async Task Decompose_FieldWithExceptionValue_HandlesCorrectly()
    {
        // Arrange
        var testObject = new ExceptionFieldObject
        {
            ExceptionField = new ArgumentException("Test")
        };

        // Act
        var result = LookupComposer.Decompose(testObject, new DecomposeOptions {IncludeFields = true});

        // Assert
        await Assert.That(result.Members).IsNotEmpty();
        var exceptionField = result.Members.First(member => member.Name == nameof(ExceptionFieldObject.ExceptionField));

        using (Assert.Multiple())
        {
            await Assert.That(exceptionField.Value.TypeName).Contains("ArgumentException");
            await Assert.That(exceptionField.Value.RawValue).IsTypeOf<ArgumentException>();
        }
    }

    [Test]
    public async Task Decompose_NullReferenceInChain_HandlesGracefully()
    {
        // Arrange
        var testObject = new NullReferenceObject {Value = null};
        var options = new DecomposeOptions {IncludeFields = true};

        // Act
        var result = LookupComposer.Decompose(testObject, options);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.Members).IsNotEmpty();
        }
    }

    [Test]
    public async Task Decompose_MultipleExceptions_AllCaptured()
    {
        // Arrange
        var testObject = new MultipleThrowingObject();

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        var property1 = result.Members.FirstOrDefault(member => member.Name == nameof(MultipleThrowingObject.ThrowingProperty1));
        var property2 = result.Members.FirstOrDefault(member => member.Name == nameof(MultipleThrowingObject.ThrowingProperty2));

        using (Assert.Multiple())
        {
            await Assert.That(property1).IsNotNull();
            await Assert.That(property2).IsNotNull();
            await Assert.That(property1!.Value.RawValue).IsTypeOf<InvalidOperationException>();
            await Assert.That(property2!.Value.RawValue).IsTypeOf<ArgumentException>();
        }
    }
}

// Test helper classes
file sealed class ThrowingPropertyObject
{
    public string ThrowingProperty => throw new InvalidOperationException("Property throws");
}

file sealed class ThrowingMethodObject
{
    public string ThrowingMethod() => throw new InvalidOperationException("Method throws");
}

file sealed class ExceptionFieldObject
{
    public Exception? ExceptionField;
}

file sealed class NullReferenceObject
{
    public string? Value;
}

file sealed class MultipleThrowingObject
{
    public string ThrowingProperty1 => throw new InvalidOperationException("First exception");
    public string ThrowingProperty2 => throw new ArgumentException("Second exception");
}