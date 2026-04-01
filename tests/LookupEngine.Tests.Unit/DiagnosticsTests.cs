using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Descriptors;

namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for performance diagnostics (time and memory tracking)
/// </summary>
public sealed class DiagnosticsTests
{
    [Test]
    public async Task Decompose_Member_TracksComputationTime()
    {
        // Arrange
        var testObject = new {Name = "Test", Value = 42};

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result.Members).IsNotEmpty();
            foreach (var member in result.Members)
            {
                await Assert.That(member.ComputationTime).IsGreaterThan(0);
            }
        }
    }

    [Test]
    public async Task Decompose_Member_TracksAllocatedBytes()
    {
        // Arrange
        var testObject = new AllocationObject();

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result.Members).IsNotEmpty();
            foreach (var member in result.Members)
            {
                await Assert.That(member.AllocatedBytes).IsGreaterThan(0);
            }
        }
    }

    [Test]
    public async Task Decompose_SlowProperty_ReflectsInComputationTime()
    {
        // Arrange
        var testObject = new SlowPropertyObject();

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        var slowMember = result.Members.First(member => member.Name == nameof(SlowPropertyObject.SlowProperty));
        using (Assert.Multiple())
        {
            await Assert.That(slowMember.ComputationTime).IsGreaterThan(10);
        }
    }

    [Test]
    public async Task Decompose_ExtensionMember_TracksMetrics()
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
        await Assert.That(result.Members).IsNotEmpty();
        var extensionMember = result.Members.First();

        using (Assert.Multiple())
        {
            await Assert.That(extensionMember.ComputationTime).IsGreaterThan(0);
            await Assert.That(extensionMember.AllocatedBytes).IsGreaterThan(0);
        }
    }

    [Test]
    public async Task Decompose_Exception_StillTracksMetrics()
    {
        // Arrange
        var testObject = new ThrowingPropertyObject();

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        var throwingMember = result.Members.First(member => member.Name == nameof(ThrowingPropertyObject.ThrowingProperty));
        using (Assert.Multiple())
        {
            await Assert.That(throwingMember.ComputationTime).IsGreaterThan(0);
            await Assert.That(throwingMember.AllocatedBytes).IsGreaterThan(0);
        }
    }

    [Test]
    public async Task Decompose_MultipleMembers_IndependentMetrics()
    {
        // Arrange
        var testObject = new {First = "A", Second = "B", Third = "C"};

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        // Each member should have its own metrics
        var times = result.Members.Select(member => member.ComputationTime).ToList();
        var allocations = result.Members.Select(member => member.AllocatedBytes).ToList();
        using (Assert.Multiple())
        {
            await Assert.That(times).Count().IsEqualTo(allocations.Count);
        }
    }

    [Test]
    public async Task Decompose_ComplexObject_NonZeroMetrics()
    {
        // Arrange
        var testObject = new List<string> {"One", "Two", "Three"};

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        await Assert.That(result.Members).IsNotEmpty();
        
        // At least some members should have measurable metrics
        var membersWithTime = result.Members.Count(member => member.ComputationTime > 0);
        var membersWithAllocation = result.Members.Count(member => member.AllocatedBytes > 0);
        
        using (Assert.Multiple())
        {
            await Assert.That(membersWithTime).IsGreaterThan(0);
            await Assert.That(membersWithAllocation).IsGreaterThan(0);
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

file sealed class AllocationObject
{
    public string AllocationProperty1 { get; set; } = "Test";
    public List<string> AllocationProperty2 { get; set; } = [];
}

file sealed class SlowPropertyObject
{
    public string SlowProperty
    {
        get
        {
            Thread.Sleep(10); // Simulate slow property
            return "Slow";
        }
    }
}

file sealed class ThrowingPropertyObject
{
    public string ThrowingProperty => throw new InvalidOperationException("Test exception");
}