using System.Reflection;
using System.Text.Json;
using LookupEngine.Abstractions;
using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Abstractions.Enums;
using LookupEngine.Descriptors;
using LookupEngine.Options;

namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for <see cref="MethodEvaluationPolicy"/> and deferred member force evaluation.
/// </summary>
public sealed class MethodEvaluationTests
{
    [Test]
    public async Task Decompose_DefaultPolicy_MethodsAreDeferred()
    {
        //Arrange
        var data = new EvaluableObject();

        //Act
        var result = LookupComposer.Decompose(data);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(EvaluableObject.GetText));
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
            await Assert.That(member.Evaluator).IsNotNull();
            await Assert.That(member.ComputationTime).IsEqualTo(0);
            await Assert.That(member.Value.RawValue).IsNull();
            await Assert.That(member.Value.TypeName).IsEqualTo(nameof(String));
        }
    }

    [Test]
    public async Task Decompose_AllPolicy_MethodsAreEvaluated()
    {
        //Arrange
        var data = new EvaluableObject();
        var options = new DecomposeOptions {EvaluationPolicy = MethodEvaluationPolicy.All};

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(EvaluableObject.GetText));
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Evaluator).IsNotNull();
            await Assert.That(member.Value.RawValue).IsEqualTo("Text");
        }
    }

    [Test]
    public async Task Decompose_MatchingFilter_MethodsAreEvaluated()
    {
        //Arrange
        var data = new EvaluableObject();
        var options = CreateFilteredOptions((_, declaringType) => declaringType.Namespace == "LookupEngine.Tests.Unit");

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(EvaluableObject.GetText));
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Value.RawValue).IsEqualTo("Text");
        }
    }

    [Test]
    public async Task Decompose_MismatchingFilter_MethodsAreDeferred()
    {
        //Arrange
        var data = new EvaluableObject();
        var options = CreateFilteredOptions((_, declaringType) => declaringType.Namespace == "System");

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(EvaluableObject.GetText));
        await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
    }

    [Test]
    public async Task Decompose_FilteredHierarchy_BaseMethodsAreDeferred()
    {
        //Arrange
        var data = new EvaluableObject();
        var options = new DecomposeOptions
        {
            IncludeRoot = true,
            EvaluationPolicy = new MethodEvaluationPolicy
            {
                EvaluatedFilter = (_, declaringType) =>
                {
                    if (declaringType.Namespace is null) return false;
                    if (declaringType.Namespace.StartsWith("LookupEngine", StringComparison.Ordinal)) return true;
                    return false;
                }
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var ownMember = result.Members.First(member => member.Name == nameof(EvaluableObject.GetText));
        var baseMember = result.Members.First(member => member.Name == nameof(ToString));
        using (Assert.Multiple())
        {
            await Assert.That(ownMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(baseMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
        }
    }

    [Test]
    public async Task Decompose_NullNamespaceType_HandledByFilter()
    {
        //Arrange
        var data = new
        {
            Name = "Test"
        };

        //Act
        var universalResult = LookupComposer.Decompose(data, CreateFilteredOptions((_, _) => true));
        var filteredResult = LookupComposer.Decompose(data, CreateFilteredOptions((_, declaringType) => declaringType.Namespace?.StartsWith("System", StringComparison.Ordinal) ?? false));

        //Assert
        var universalMember = universalResult.Members.First(member => member.Name == nameof(ToString));
        var filteredMember = filteredResult.Members.First(member => member.Name == nameof(ToString));
        using (Assert.Multiple())
        {
            await Assert.That(universalMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(filteredMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
        }
    }

    [Test]
    public async Task Decompose_AllPolicy_VoidMethodsAreDeferred()
    {
        //Arrange
        var data = new VoidMethodObject();
        var options = new DecomposeOptions
        {
            EvaluationPolicy = MethodEvaluationPolicy.All
        };

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(VoidMethodObject.Run));
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
            await Assert.That(member.Value.TypeName).IsEqualTo("Void");
            await Assert.That(data.InvocationCount).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Decompose_ReturnTypeFilter_MethodsAreDeferred()
    {
        //Arrange
        var data = new ReturnTypesObject();
        var options = new DecomposeOptions
        {
            EvaluationPolicy = new MethodEvaluationPolicy
            {
                EvaluatedFilter = (member, _) => member.ReturnType != typeof(bool)
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var excludedMember = result.Members.First(member => member.Name == nameof(ReturnTypesObject.IsValid));
        var evaluatedMember = result.Members.First(member => member.Name == nameof(ReturnTypesObject.GetNumber));
        using (Assert.Multiple())
        {
            await Assert.That(excludedMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
            await Assert.That(evaluatedMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(evaluatedMember.Value.RawValue).IsEqualTo(42);
        }
    }

    [Test]
    public async Task Decompose_EvaluateAllFilter_VoidMethodsAreEvaluated()
    {
        //Arrange
        var data = new VoidMethodObject();
        var options = new DecomposeOptions
        {
            EvaluationPolicy = new MethodEvaluationPolicy
            {
                EvaluatedFilter = (_, _) => true
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(VoidMethodObject.Run));
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Value.RawValue).IsNull();
            await Assert.That(member.Value.TypeName).IsEqualTo("Void");
            await Assert.That(member.Value.TypeFullName).IsEqualTo("System.Void");
            await Assert.That(data.InvocationCount).IsEqualTo(1);
        }
    }

    [Test]
    public async Task Decompose_ParametricMethodWithoutResolver_RemainsUnsupported()
    {
        //Arrange
        var data = new ParametricObject();
        var options = new DecomposeOptions
        {
            IncludeUnsupported = true
        };

        //Act
        var defaultResult = LookupComposer.Decompose(data);
        var comparableResult = LookupComposer.Decompose(data, options);

        //Assert
        var member = comparableResult.Members.First(member => member.Name.StartsWith(nameof(ParametricObject.WithParameter)));
        using (Assert.Multiple())
        {
            await Assert.That(defaultResult.Members.Where(decomposedMembers => decomposedMembers.Name.StartsWith(nameof(ParametricObject.WithParameter)))).IsEmpty();
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Unsupported);
            await Assert.That(member.Evaluator).IsNull();
            await Assert.That(member.Value.RawValue).IsNull();
        }
    }

    [Test]
    public async Task Evaluate_DeferredMethod_UpdatesMemberInPlace()
    {
        //Arrange
        var data = new EvaluableObject();
        var result = LookupComposer.Decompose(data);
        var member = result.Members.First(member => member.Name == nameof(EvaluableObject.GetNumber));
        var originalDepth = member.Depth;

        //Act
        member.Evaluate();

        //Assert
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Evaluator).IsNotNull();
            await Assert.That(member.Value.RawValue).IsEqualTo(42);
            await Assert.That(member.Value.TypeName).IsEqualTo(nameof(Int32));
            await Assert.That(member.ComputationTime).IsGreaterThanOrEqualTo(0);
            await Assert.That(member.AllocatedBytes).IsGreaterThanOrEqualTo(0);
            await Assert.That(member.Depth).IsEqualTo(originalDepth);
            await Assert.That(member.Name).IsEqualTo(nameof(EvaluableObject.GetNumber));
        }
    }

    [Test]
    public async Task Evaluate_DeferredVoidMethod_InvokesMethod()
    {
        //Arrange
        var data = new VoidMethodObject();
        var result = LookupComposer.Decompose(data);
        var member = result.Members.First(member => member.Name == nameof(VoidMethodObject.Run));

        //Act
        member.Evaluate();

        //Assert
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Value.RawValue).IsNull();
            await Assert.That(member.Value.TypeName).IsEqualTo("Void");
            await Assert.That(member.Value.TypeFullName).IsEqualTo("System.Void");
            await Assert.That(data.InvocationCount).IsEqualTo(1);
        }
    }

    [Test]
    public async Task Evaluate_DeferredResolvedMethod_InvokesResolverHandler()
    {
        //Arrange
        var data = new ResolvableObject();
        var options = new DecomposeOptions
        {
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    ResolvableObject => new ResolverDescriptor(),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name.StartsWith(nameof(ResolvableObject.ResolvableMethod)));
        await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);

        member.Evaluate();
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Value.RawValue).IsEqualTo("Resolved");
            await Assert.That(member.Value.Description).IsEqualTo("Value description");
        }
    }

    [Test]
    public async Task Evaluate_DeferredContextMethod_InvokesHandlerWithContext()
    {
        //Arrange
        var data = new ResolvableObject();
        var contextOptions = new DecomposeOptions<EvaluationTestContext>
        {
            Context = new EvaluationTestContext(),
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    ResolvableObject => new ResolverDescriptor(),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, contextOptions);

        //Assert
        var member = result.Members.First(member => member.Name.StartsWith(nameof(ResolvableObject.ContextMethod)));
        await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);

        member.Evaluate();
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Value.RawValue).IsEqualTo("Test context");
        }
    }

    [Test]
    public async Task Evaluate_ThrowingMethod_ExceptionBecomesValue()
    {
        //Arrange
        var data = new ThrowingMethodObject();
        var result = LookupComposer.Decompose(data);
        var member = result.Members.First(member => member.Name == nameof(ThrowingMethodObject.ThrowingMethod));

        //Act
        member.Evaluate();

        //Assert
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Value.RawValue).IsTypeOf<InvalidOperationException>();
            await Assert.That(member.Value.TypeName).Contains(nameof(Exception));
        }
    }

    [Test]
    public async Task Evaluate_RedirectedMethod_ValueIsRedirected()
    {
        //Arrange
        var data = new RedirectReturningObject();
        var options = new DecomposeOptions
        {
            EnableRedirection = true,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    RedirectSourceObject => new SourceRedirectionDescriptor(),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(RedirectReturningObject.GetValue));
        await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);

        member.Evaluate();
        using (Assert.Multiple())
        {
            await Assert.That(member.Value.RawValue).IsEqualTo("Redirected");
            await Assert.That(member.Value.TypeName).IsEqualTo(nameof(String));
        }
    }

    [Test]
    public async Task Revaluate_EvaluatedMember_Evaluated()
    {
        //Arrange
        var data = new EvaluableObject();
        var options = new DecomposeOptions {EvaluationPolicy = MethodEvaluationPolicy.All};
        var result = LookupComposer.Decompose(data, options);
        var member = result.Members.First(member => member.Name == nameof(EvaluableObject.GetText));

        //Act
        member.Evaluate();

        //Assert
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Evaluator).IsNotNull();
            await Assert.That(member.Value.RawValue).IsEqualTo("Text");
        }
    }

    [Test]
    public async Task Revaluate_Twice_Evaluated()
    {
        //Arrange
        var data = new EvaluableObject();
        var result = LookupComposer.Decompose(data);
        var member = result.Members.First(member => member.Name == nameof(EvaluableObject.GetText));

        //Act
        member.Evaluate();
        member.Evaluate();

        //Assert
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Evaluator).IsNotNull();
            await Assert.That(member.Value.RawValue).IsEqualTo("Text");
        }
    }

    [Test]
    public async Task Revaluate_Field_Evaluated()
    {
        //Arrange
        var data = new FieldHolder();
        var options = new DecomposeOptions {IncludeFields = true};
        var result = LookupComposer.Decompose(data, options);
        var member = result.Members.First(member => member.Name == nameof(FieldHolder.Counter));

        //Assert
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Evaluator).IsNotNull();
            await Assert.That(member.Value.RawValue).IsEqualTo(0);
        }

        data.Counter = 7;
        member.Evaluate();
        await Assert.That(member.Value.RawValue).IsEqualTo(7);
    }

    [Test]
    public async Task Decompose_DeferredMembers_SerializationRoundTrip()
    {
        //Arrange
        var data = new EvaluableObject();
        var serializerOptions = new JsonSerializerOptions {WriteIndented = true};

        //Act
        var result = LookupComposer.Decompose(data);
        var json = JsonSerializer.Serialize(result, serializerOptions);
        var deserialized = JsonSerializer.Deserialize<DecomposedObject>(json);

        //Assert
        var member = deserialized!.Members.First(member => member.Name == nameof(EvaluableObject.GetText));
        using (Assert.Multiple())
        {
            await Assert.That(json).Contains($"\"{nameof(DecomposedMember.EvaluationPolicy)}\": 1");
            await Assert.That(json).DoesNotContain(nameof(DecomposedMember.Evaluator));
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
            await Assert.That(member.Evaluator).IsNull();
            await Assert.That(member.Value.TypeName).IsEqualTo(nameof(String));
        }
    }

    private static DecomposeOptions CreateFilteredOptions(Func<MethodInfo, Type, bool> filter)
    {
        return new DecomposeOptions
        {
            EvaluationPolicy = new MethodEvaluationPolicy
            {
                EvaluatedFilter = filter
            }
        };
    }
}

file sealed class EvaluableObject
{
    public string GetText()
    {
        return "Text";
    }

    public int GetNumber()
    {
        return 42;
    }
}

file sealed class FieldHolder
{
    public int Counter;
}

file sealed class VoidMethodObject
{
    public int InvocationCount;

    public void Run()
    {
        InvocationCount++;
    }
}

file sealed class ReturnTypesObject
{
    public bool IsValid()
    {
        return true;
    }

    public int GetNumber()
    {
        return 42;
    }
}

file sealed class ParametricObject
{
    public string WithParameter(int parameter)
    {
        return parameter.ToString();
    }
}

file sealed class ThrowingMethodObject
{
    public string ThrowingMethod()
    {
        throw new InvalidOperationException("Method throws");
    }
}

file sealed class RedirectReturningObject
{
    public RedirectSourceObject GetValue()
    {
        return new RedirectSourceObject();
    }
}

file sealed class RedirectSourceObject;

file sealed class SourceRedirectionDescriptor : Descriptor, IDescriptorRedirector
{
    public bool TryRedirect(string target, out object result)
    {
        result = "Redirected";
        return true;
    }
}

file sealed class ResolvableObject
{
    public string ResolvableMethod(int parameter)
    {
        return parameter.ToString();
    }

    public string ContextMethod(int parameter)
    {
        return parameter.ToString();
    }
}

file sealed class EvaluationTestContext
{
    public string Metadata { get; } = "Test context";
}

file sealed class ResolverDescriptor : Descriptor, IDescriptorConfigurator, IDescriptorConfigurator<EvaluationTestContext>
{
    public void Configure(IMemberConfigurator configuration)
    {
        configuration.Member(nameof(ResolvableObject.ResolvableMethod)).Resolve(() => Variants.Value("Resolved", "Value description"));
    }

    public void Configure(IMemberConfigurator<EvaluationTestContext> configuration)
    {
        configuration.Member(nameof(ResolvableObject.ContextMethod)).Resolve(context => Variants.Value(context.Metadata));
    }
}