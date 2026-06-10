using LookupEngine.Descriptors;

namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for IEnumerable decomposition
/// </summary>
public sealed class EnumerableTests
{
    [Test]
    public async Task Decompose_List_IncludesItemsAsMembers()
    {
        // Arrange
        var list = new List<int> {1, 2, 3, 4, 5};

        // Act
        var result = LookupComposer.Decompose(list);

        // Assert
        var enumerableMembers = result.Members.Where(member => member.Name.Contains(nameof(Int32))).ToList();
        using (Assert.Multiple())
        {
            await Assert.That(result.Members).IsNotEmpty();
            await Assert.That(enumerableMembers).Count().IsEqualTo(5);
        }
    }

    [Test]
    public async Task Decompose_Array_IncludesArrayElements()
    {
        // Arrange
        var array = new[] {"First", "Second", "Third"};

        // Act
        var result = LookupComposer.Decompose(array);

        // Assert
        await Assert.That(result.Members).IsNotEmpty();
        var arrayMembers = result.Members.Where(member => member.Name.Contains(nameof(String))).ToList();
        
        await Assert.That(arrayMembers).Count().IsEqualTo(3);
    }

    [Test]
    public async Task Decompose_EmptyList_HasNoEnumerableMembers()
    {
        // Arrange
        var list = new List<int>();

        // Act
        var result = LookupComposer.Decompose(list);

        // Assert
        var enumerableMembers = result.Members.Where(member => member.Name.Contains(nameof(Int32))).ToList();
        using (Assert.Multiple())
        {
            await Assert.That(result.Members).IsNotNull();
            await Assert.That(enumerableMembers).IsEmpty();
        }
    }

    [Test]
    public async Task Decompose_Dictionary_IncludesKeyValuePairs()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            ["one"] = 1,
            ["two"] = 2
        };

        // Act
        var result = LookupComposer.Decompose(dictionary);

        // Assert
        var pairMembers = result.Members
            .Where(member => member.Name.Contains($"{nameof(Dictionary<,>)}<{nameof(String)}, {nameof(Int32)}>"))
            .ToList();

        using (Assert.Multiple())
        {
            await Assert.That(result.Members).IsNotEmpty();
            await Assert.That(pairMembers).Count().IsEqualTo(2);
        }
    }

    [Test]
    public async Task Decompose_CustomEnumerable_HandlesCorrectly()
    {
        // Arrange
        var customEnumerable = new CustomEnumerable();

        // Act
        var result = LookupComposer.Decompose(customEnumerable);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(result.Members).IsNotEmpty();
        }
    }

    [Test]
    public async Task Decompose_EnumerableWithNullElements_HandlesNulls()
    {
        // Arrange
        var list = new List<string?> {"First", null, "Third"};

        // Act
        var result = LookupComposer.Decompose(list);

        // Assert
        var enumerableMembers = result.Members.Where(member => member.Name.Contains(nameof(String))).ToList();
        using (Assert.Multiple())
        {
            await Assert.That(result.Members).IsNotEmpty();
            await Assert.That(enumerableMembers).Count().IsEqualTo(3);
        }
    }

    [Test]
    public async Task Decompose_NestedEnumerable_HandlesRecursion()
    {
        // Arrange
        var nestedList = new List<List<int>>
        {
            new() {1, 2},
            new() {3, 4}
        };

        // Act
        var result = LookupComposer.Decompose(nestedList);

        // Assert
        await Assert.That(result.Members).IsNotEmpty();
    }

    [Test]
    public async Task Decompose_String_TreatedAsEnumerable()
    {
        // Arrange
        var text = "Test";

        // Act
        var result = LookupComposer.Decompose(text);

        // Assert
        var charMembers = result.Members.Where(member => member.Name.StartsWith($"{nameof(String)}[")).ToList();
        using (Assert.Multiple())
        {
            await Assert.That(result.Members).IsNotEmpty();
            await Assert.That(charMembers).Count().IsEqualTo(4);
        }
    }

    [Test]
    public async Task Decompose_EnumerableIndexing_IsSequential()
    {
        // Arrange
        var list = new List<string> {"A", "B", "C"};

        // Act
        var result = LookupComposer.Decompose(list);

        // Assert
        var enumerableMembers = result.Members.Where(member => member.Name.Contains(nameof(String))).ToList();
        using (Assert.Multiple())
        {
            await Assert.That(enumerableMembers[0].Name).IsEqualTo($"{nameof(List<>)}<{nameof(String)}>[0]");
            await Assert.That(enumerableMembers[1].Name).IsEqualTo($"{nameof(List<>)}<{nameof(String)}>[1]");
            await Assert.That(enumerableMembers[2].Name).IsEqualTo($"{nameof(List<>)}<{nameof(String)}>[2]");
        }
    }

    [Test]
    public async Task Decompose_EnumeratorDisposed_NoMemoryLeak()
    {
        // Arrange
        var disposableEnumerable = new DisposableEnumerable();

        // Act
        var result = LookupComposer.Decompose(disposableEnumerable);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result).IsNotNull();
            await Assert.That(disposableEnumerable.IsDisposed).IsTrue();
        }
    }

    [Test]
    public async Task Decompose_Enumerable_EnumeratesSourceOnce()
    {
        // Arrange
        var countingEnumerable = new CountingEnumerable();

        // Act
        var result = LookupComposer.Decompose(countingEnumerable);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(countingEnumerable.EnumerationCount).IsEqualTo(1);
            await Assert.That(result.Members.Count(member => member.Name.Contains('['))).IsEqualTo(2);
        }
    }

    [Test]
    public async Task Decompose_EnumerableItems_HaveSameDepth()
    {
        // Arrange
        var list = new List<string> {"A", "B", "C"};

        // Act
        var result = LookupComposer.Decompose(list);

        // Assert
        var itemDepths = result.Members
            .Where(member => member.Name.Contains('['))
            .Select(member => member.Depth)
            .Distinct()
            .ToList();

        await Assert.That(itemDepths).Count().IsEqualTo(1);
    }

    [Test]
    public async Task EnumerableDescriptor_IsEmpty_EvaluatesCorrectly()
    {
        // Arrange
        var emptyCollection = new EnumerableDescriptor(new List<int>());
        var filledCollection = new EnumerableDescriptor(new List<int> {1});
        var emptyIterator = new EnumerableDescriptor(EmptyIterator());
        var filledIterator = new EnumerableDescriptor(FilledIterator());

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(emptyCollection.IsEmpty).IsTrue();
            await Assert.That(filledCollection.IsEmpty).IsFalse();
            await Assert.That(emptyIterator.IsEmpty).IsTrue();
            await Assert.That(filledIterator.IsEmpty).IsFalse();
        }

        static IEnumerable<int> EmptyIterator()
        {
            yield break;
        }

        static IEnumerable<int> FilledIterator()
        {
            yield return 1;
        }
    }

    [Test]
    public async Task EnumerableDescriptor_Enumerator_ReturnsFreshEnumerator()
    {
        // Arrange
        var descriptor = new EnumerableDescriptor(new List<int> {1, 2, 3});

        // Act - IsEmpty must not advance or dispose the exposed enumerator
        var isEmpty = descriptor.IsEmpty;

        var firstPass = 0;
        var enumerator = descriptor.Enumerator;
        while (enumerator.MoveNext()) firstPass++;

        var secondPass = 0;
        var freshEnumerator = descriptor.Enumerator;
        while (freshEnumerator.MoveNext()) secondPass++;

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(isEmpty).IsFalse();
            await Assert.That(firstPass).IsEqualTo(3);
            await Assert.That(secondPass).IsEqualTo(3);
        }
    }
}

// Test helper classes
file sealed class CountingEnumerable : System.Collections.IEnumerable
{
    private int _enumerationCount;

    public int EnumerationCount => _enumerationCount;

    public System.Collections.IEnumerator GetEnumerator()
    {
        _enumerationCount++;
        return new[] {1, 2}.GetEnumerator();
    }
}

file sealed class CustomEnumerable : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator()
    {
        yield return 1;
        yield return 2;
        yield return 3;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

file sealed class DisposableEnumerable : IEnumerable<int>
{
    public bool IsDisposed { get; private set; }

    public IEnumerator<int> GetEnumerator()
    {
        return new DisposableEnumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class DisposableEnumerator(DisposableEnumerable parent) : IEnumerator<int>
    {
        private int _current;

        public int Current => _current;
        object System.Collections.IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_current >= 3) return false;
            _current++;
            return true;
        }

        public void Reset() => _current = 0;

        public void Dispose()
        {
            parent.IsDisposed = true;
        }
    }
}