namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for <see cref="DecomposeOptions"/> configuration and its effect on decomposition behavior.
/// </summary>
public sealed class OptionsTests
{
    [Test]
    public async Task Decompose_DefaultOptions_HasMembers()
    {
        //Arrange
        var data = "TestString";

        //Act
        var defaultResult = LookupComposer.Decompose(data);

        //Assert
        await Assert.That(defaultResult.Members).IsNotEmpty();
    }

    [Test]
    public async Task Decompose_IncludingFields_HasFields()
    {
        //Arrange
        var data = new PublicFieldsObject();
        var options = new DecomposeOptions
        {
            IncludeFields = true
        };

        //Act
        var defaultResult = LookupComposer.Decompose(data);
        var comparableResult = LookupComposer.Decompose(data, options);

        //Assert
        await Assert.That(comparableResult.Members.Count).IsGreaterThan(defaultResult.Members.Count);
    }

    [Test]
    public async Task Decompose_IncludingPrivate_HasPrivateMembers()
    {
        //Arrange
        var data = "TestString";
        var options = new DecomposeOptions
        {
            IncludePrivateMembers = true,
        };

        //Act
        var defaultResult = LookupComposer.Decompose(data);
        var comparableResult = LookupComposer.Decompose(data, options);

        //Assert
        await Assert.That(comparableResult.Members.Count).IsGreaterThan(defaultResult.Members.Count);
    }

    [Test]
    public async Task Decompose_IncludingUnsupported_HasUnsupported()
    {
        //Arrange
        var data = "TestString";
        var options = new DecomposeOptions
        {
            IncludeUnsupported = true
        };

        //Act
        var defaultResult = LookupComposer.Decompose(data);
        var comparableResult = LookupComposer.Decompose(data, options);

        //Assert
        await Assert.That(comparableResult.Members.Count).IsGreaterThan(defaultResult.Members.Count);
    }

    [Test]
    public async Task Decompose_IncludingRoot_HasRootMembers()
    {
        //Arrange
        var data = "TestString";
        var options = new DecomposeOptions
        {
            IncludeRoot = true
        };

        //Act
        var defaultResult = LookupComposer.Decompose(data);
        var comparableResult = LookupComposer.Decompose(data, options);

        //Assert
        await Assert.That(comparableResult.Members.Count).IsGreaterThan(defaultResult.Members.Count);
    }

    [Test]
    public async Task Decompose_IncludingStatic_HasStaticMembers()
    {
        //Arrange
        var data = DateTime.Now;
        var options = new DecomposeOptions
        {
            IncludeStaticMembers = true
        };

        //Act
        var defaultResult = LookupComposer.Decompose(data);
        var comparableResult = LookupComposer.Decompose(data, options);

        //Assert
        await Assert.That(comparableResult.Members.Count).IsGreaterThan(defaultResult.Members.Count);
    }

    [Test]
    public async Task Decompose_IncludingEvents_HasEvents()
    {
        //Arrange
        var data = AppDomain.CurrentDomain;
        var options = new DecomposeOptions
        {
            IncludeEvents = true
        };

        //Act
        var defaultResult = LookupComposer.Decompose(data);
        var comparableResult = LookupComposer.Decompose(data, options);

        //Assert
        await Assert.That(comparableResult.Members.Count).IsGreaterThan(defaultResult.Members.Count);
    }
}

[PublicAPI]
file sealed class PublicFieldsObject
{
    public string? Field;
}