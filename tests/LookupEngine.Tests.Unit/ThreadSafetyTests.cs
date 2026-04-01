using LookupEngine.Abstractions;
using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Descriptors;

namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for thread-safety and concurrent decomposition
/// </summary>
public sealed class ThreadSafetyTests
{
    [Test]
    public async Task Decompose_ConcurrentCalls_AllSucceed()
    {
        // Arrange
        var testObject = new {Name = "Test", Value = 42};
        const int threadCount = 10;
        var tasks = new List<Task<DecomposedObject>>();

        // Act
        for (var i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() => LookupComposer.Decompose(testObject)));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(results).Count().IsEqualTo(threadCount);
            foreach (var result in results)
            {
                await Assert.That(result).IsNotNull();
                await Assert.That(result.Members).Count().IsGreaterThanOrEqualTo(2);
                await Assert.That(result.Members[0].Name).IsEqualTo(nameof(testObject.Name));
                await Assert.That(result.Members[1].Name).IsEqualTo(nameof(testObject.Value));
            }
        }
    }

    [Test]
    public async Task Decompose_ParallelDecomposition_ProducesConsistentResults()
    {
        // Arrange
        var testObjects = Enumerable.Range(0, 100)
            .Select(i => new {Index = i, Value = i * 2})
            .ToList();

        // Act
        var results = new DecomposedObject[testObjects.Count];
        Parallel.For(0, testObjects.Count, i => { results[i] = LookupComposer.Decompose(testObjects[i]); });

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(results).Count().IsEqualTo(100);
            foreach (var result in results)
            {
                await Assert.That(result).IsNotNull();
                await Assert.That(result.Members).Count().IsGreaterThanOrEqualTo(2);
            }
        }
    }

    [Test]
    public async Task Decompose_WithOptions_ThreadSafe()
    {
        // Arrange
        var testObject = new {Name = "Test"};
        var options = new DecomposeOptions
        {
            IncludeStaticMembers = true,
            IncludePrivateMembers = true
        };

        const int threadCount = 10;
        var tasks = new List<Task<DecomposedObject>>();

        // Act
        for (var i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() => LookupComposer.Decompose(testObject, options)));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(results).Count().IsEqualTo(threadCount);
            foreach (var result in results)
            {
                await Assert.That(result).IsNotNull();
            }
        }
    }

    [Test]
    public async Task Decompose_DifferentObjectsConcurrently_NoInterference()
    {
        // Arrange
        var object1 = new {Type = "First", Value = 1};
        var object2 = new {Type = "Second", Count = 2};
        var object3 = new DateTime(2025, 1, 11);

        // Act
        var task1 = Task.Run(() => LookupComposer.Decompose(object1));
        var task2 = Task.Run(() => LookupComposer.Decompose(object2));
        var task3 = Task.Run(() => LookupComposer.Decompose(object3));

        var results = await Task.WhenAll(task1, task2, task3);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(results[0]).IsNotNull();
            await Assert.That(results[1]).IsNotNull();
            await Assert.That(results[2]).IsNotNull();
        }
    }

    [Test]
    public async Task Decompose_SharedOptions_ThreadSafe()
    {
        // Arrange
        var sharedOptions = new DecomposeOptions
        {
            IncludeStaticMembers = true
        };

        var tasks = Enumerable.Range(0, 20)
            .Select(i => Task.Run(() =>
            {
                var obj = new {Index = i};
                return LookupComposer.Decompose(obj, sharedOptions);
            }))
            .ToList();

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(results).Count().IsEqualTo(20);
            foreach (var result in results)
            {
                await Assert.That(result).IsNotNull();
            }
        }
    }

    [Test]
    public async Task DecomposeMembers_Concurrent_AllSucceed()
    {
        // Arrange
        var testObject = new {A = 1, B = 2, C = 3};
        const int threadCount = 10;

        // Act
        var tasks = Enumerable.Range(0, threadCount)
            .Select(_ => Task.Run(() => LookupComposer.DecomposeMembers(testObject)))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(results).Count().IsEqualTo(threadCount);
            foreach (var result in results)
            {
                await Assert.That(result).Count().IsGreaterThanOrEqualTo(3);
                await Assert.That(result[0].Name).IsEqualTo(nameof(testObject.A));
                await Assert.That(result[1].Name).IsEqualTo(nameof(testObject.B));
                await Assert.That(result[2].Name).IsEqualTo(nameof(testObject.C));
            }
        }
    }

    [Test]
    public async Task Decompose_WithCustomDescriptors_ThreadSafe()
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
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => LookupComposer.Decompose(testObject, options)))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(results).Count().IsEqualTo(10);
            foreach (var result in results)
            {
                await Assert.That(result.Members).IsNotEmpty();
            }
        }
    }

    [Test]
    public async Task Decompose_HighConcurrency_NoDeadlocks()
    {
        // Arrange
        const int taskCount = 100;
        var testObject = new {Value = "Test"};

        // Act
        var tasks = Enumerable.Range(0, taskCount)
            .Select(_ => Task.Run(() =>
            {
                var result = LookupComposer.Decompose(testObject);
                return result.Members.Count;
            }))
            .ToList();

        var memberCounts = await Task.WhenAll(tasks);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(memberCounts).Count().IsEqualTo(taskCount);
            await Assert.That(memberCounts.Distinct()).Count().IsEqualTo(1);
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