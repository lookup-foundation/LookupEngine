using System.Text.Json;
using LookupEngine.Abstractions;

namespace LookupEngine.Tests.Unit;

public sealed class SerializationTests
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true
    };
    
    [Test]
    public async Task Decompose_SerializeToJson_ReturnsValidJson()
    {
        // Arrange
        var testObject = new SerializableObject();
        var result = LookupComposer.Decompose(testObject);

        // Act
        var json = JsonSerializer.Serialize(result, _serializerOptions);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(json).IsNotNull();
            await Assert.That(json).Contains($"""
                                             "Name": "{nameof(SerializableObject.Name)}"
                                             """);
            await Assert.That(json).Contains($"""
                                              "Name": "{nameof(SerializableObject.Value)}"
                                              """);
            await Assert.That(json).DoesNotContain(nameof(DecomposedObject.RawValue));
            await Assert.That(json).DoesNotContain(nameof(DecomposedObject.Descriptor));
        }
    }

    [Test]
    public async Task Decompose_SerializeAndDeserialize_ReturnsEquivalentObject()
    {
        // Arrange
        var testObject = new SerializableObject();
        var original = LookupComposer.Decompose(testObject);

        // Act
        var json = JsonSerializer.Serialize(original, _serializerOptions);
        var deserialized = JsonSerializer.Deserialize<DecomposedObject>(json);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(deserialized).IsNotNull();
            await Assert.That(deserialized!.Name).IsEqualTo(original.Name);
            await Assert.That(deserialized.TypeName).IsEqualTo(original.TypeName);
            await Assert.That(deserialized.TypeFullName).IsEqualTo(original.TypeFullName);
            await Assert.That(deserialized.Members).Count().IsEqualTo(original.Members.Count);
            
            for (var i = 0; i < original.Members.Count; i++)
            {
                await Assert.That(deserialized.Members[i].Name).IsEqualTo(original.Members[i].Name);
                await Assert.That(deserialized.Members[i].Value.Name).IsEqualTo(original.Members[i].Value.Name);
                await Assert.That(deserialized.Members[i].Value.TypeName).IsEqualTo(original.Members[i].Value.TypeName);
                await Assert.That(deserialized.Members[i].Value.TypeFullName).IsEqualTo(original.Members[i].Value.TypeFullName);
            }
        }
    }
}

public class SerializableObject
{
    public string? Name { get; set; } = "Test";
    public int? Value { get; set; } = 42;
}
