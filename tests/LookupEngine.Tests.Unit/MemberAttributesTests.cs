using JetBrains.Annotations;
using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Abstractions.Enums;
using LookupEngine.Descriptors;

namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for MemberAttributes flag tracking
/// </summary>
public sealed class MemberAttributesTests
{
    [Test]
    public async Task Decompose_Property_HasPropertyFlag()
    {
        // Arrange
        var testObject = new { TestProperty = "Value" };

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        await Assert.That(result.Members[0].MemberAttributes).HasFlag(MemberAttributes.Property);
    }

    [Test]
    public async Task Decompose_Field_HasFieldFlag()
    {
        // Arrange
        var testObject = new FieldsObject { PublicField = "Value" };
        var options = new DecomposeOptions { IncludeFields = true };

        // Act
        var result = LookupComposer.Decompose(testObject, options);

        // Assert
        using (Assert.Multiple())
        {
            var fieldMember = result.Members.First(member => member.Name == nameof(FieldsObject.PublicField));
            await Assert.That(fieldMember.MemberAttributes).HasFlag(MemberAttributes.Field);
        }
    }

    [Test]
    public async Task Decompose_Method_HasMethodFlag()
    {
        // Arrange
        var testObject = new MethodsObject();

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        using (Assert.Multiple())
        {
            var methodMember = result.Members.First(member => member.Name == nameof(MethodsObject.PublicMethod));
            await Assert.That(methodMember.MemberAttributes).HasFlag(MemberAttributes.Method);
        }
    }

    [Test]
    public async Task Decompose_StaticMember_HasStaticFlag()
    {
        // Arrange
        var testObject = new StaticMembersObject();
        var options = new DecomposeOptions { IncludeStaticMembers = true };

        // Act
        var result = LookupComposer.Decompose(testObject, options);

        // Assert
        using (Assert.Multiple())
        {
            var staticMember = result.Members.First(member => member.Name == nameof(StaticMembersObject.StaticProperty));
            await Assert.That(staticMember.MemberAttributes).HasFlag(MemberAttributes.Static);
        }
    }

    [Test]
    public async Task Decompose_PrivateMember_HasPrivateFlag()
    {
        // Arrange
        var testObject = new PrivateMembersObject();
        var options = new DecomposeOptions { IncludePrivateMembers = true };

        // Act
        var result = LookupComposer.Decompose(testObject, options);

        // Assert
        using (Assert.Multiple())
        {
            var privateMembers = result.Members.Where(member => member.MemberAttributes.HasFlag(MemberAttributes.Private)).ToList();
            await Assert.That(privateMembers).IsNotEmpty();
        }
    }

    [Test]
    public async Task Decompose_Extension_HasExtensionFlag()
    {
        // Arrange
        var testObject = new ExtensibleObject();
        var options = new DecomposeOptions
        {
            EnableExtensions = true,
            TypeResolver = (obj, _) => obj switch
            {
                ExtensibleObject => new ExtensionDescriptor(),
                _ => new ObjectDescriptor(obj)
            }
        };

        // Act
        var result = LookupComposer.Decompose(testObject, options);

        // Assert
        using (Assert.Multiple())
        {
            var extensionMember = result.Members.First();
            await Assert.That(extensionMember.MemberAttributes).HasFlag(MemberAttributes.Extension);
        }
    }

    [Test]
    public async Task Decompose_Event_HasEventFlag()
    {
        // Arrange
        var testObject = new EventsObject();
        var options = new DecomposeOptions { IncludeEvents = true };

        // Act
        var result = LookupComposer.Decompose(testObject, options);

        // Assert
        using (Assert.Multiple())
        {
            var eventMembers = result.Members.Where(member => member.MemberAttributes.HasFlag(MemberAttributes.Event)).ToList();
            await Assert.That(eventMembers).IsNotEmpty();
        }
    }

    [Test]
    public async Task Decompose_StaticPrivateProperty_HasBothFlags()
    {
        // Arrange
        var type = typeof(CombinedAttributesClass);
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
            var staticPrivateMember = result.Members.FirstOrDefault(m =>
                m.MemberAttributes.HasFlag(MemberAttributes.Static) &&
                m.MemberAttributes.HasFlag(MemberAttributes.Private));

            await Assert.That(staticPrivateMember).IsNotNull();
        }
    }

    [Test]
    public async Task Decompose_PublicProperty_NoPrivateOrStaticFlags()
    {
        // Arrange
        var testObject = new { PublicProperty = "Value" };

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result.Members[0].MemberAttributes).DoesNotHaveFlag(MemberAttributes.Private);
            await Assert.That(result.Members[0].MemberAttributes).DoesNotHaveFlag(MemberAttributes.Static);
        }
    }
}

file sealed class ExtensibleObject;

file sealed class ExtensionDescriptor : Descriptor, IDescriptorExtension
{
    public void RegisterExtensions(IExtensionManager manager)
    {
        manager.Define("Extension").Register(Extension);
        return;

        IVariant Extension()
        {
            return Variants.Value("Extended");
        }
    }
}

// Test helper classes
file sealed class FieldsObject
{
    public string? PublicField;
}

file sealed class MethodsObject
{
    public string PublicMethod() => "Result";
}

file sealed class StaticMembersObject
{
    public static string StaticProperty => "Static";
}

[PublicAPI]
file sealed class PrivateMembersObject
{
    private string PrivateProperty => "Private";
}

[PublicAPI]
file sealed class EventsObject
{
    public event EventHandler? TestEvent;
}

[PublicAPI]
file sealed class CombinedAttributesClass
{
    private static string PrivateStaticProperty => "Combined";
}