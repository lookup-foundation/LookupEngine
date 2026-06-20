using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Abstractions.Enums;
using LookupEngine.Descriptors;
using LookupEngine.Options;

namespace LookupEngine.Tests.Unit;

/// <summary>
///     Tests for per-member evaluation overrides declared through <see cref="IDescriptorConfigurator"/>.
/// </summary>
public sealed class EvaluationOverrideTests
{
    [Test]
    public async Task Decompose_EvaluatedOverride_BeatsDeferringPolicy()
    {
        //Arrange
        var data = new OverridableObject();
        var options = CreateOptions(manager => manager.Member(nameof(OverridableObject.GetText)).Evaluate());

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var overriddenMember = result.Members.First(member => member.Name == nameof(OverridableObject.GetText));
        var policyMember = result.Members.First(member => member.Name == nameof(OverridableObject.GetHeavy));
        using (Assert.Multiple())
        {
            await Assert.That(overriddenMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(overriddenMember.Value.RawValue).IsEqualTo("Text");
            await Assert.That(policyMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
        }
    }

    [Test]
    public async Task Decompose_EvaluatedOverride_BeatsVoidDeferral()
    {
        //Arrange
        var data = new OverridableObject();
        var options = CreateOptions(manager => manager.Member(nameof(OverridableObject.Run)).Evaluate(), MethodEvaluationPolicy.All);

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(OverridableObject.Run));
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(data.RunCount).IsEqualTo(1);
        }
    }

    [Test]
    public async Task Decompose_DeferredOverride_BeatsEvaluatingPolicy()
    {
        //Arrange
        var data = new OverridableObject();
        var options = CreateOptions(manager => manager.Member(nameof(OverridableObject.GetHeavy)).Defer(), MethodEvaluationPolicy.All);

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var deferredMember = result.Members.First(member => member.Name == nameof(OverridableObject.GetHeavy));
        var policyMember = result.Members.First(member => member.Name == nameof(OverridableObject.GetText));
        await Assert.That(deferredMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);

        deferredMember.Evaluate();
        using (Assert.Multiple())
        {
            await Assert.That(deferredMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(deferredMember.Value.RawValue).IsEqualTo("Heavy");
            await Assert.That(policyMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
        }
    }

    [Test]
    public async Task Evaluate_DeferredHandler_InvokesHandler()
    {
        //Arrange
        var data = new OverridableObject();
        var options = CreateOptions(manager => manager.Member(nameof(OverridableObject.WithParameter)).Defer(() => Variants.Value("Resolved")));

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name.StartsWith(nameof(OverridableObject.WithParameter)));
        await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);

        member.Evaluate();
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Value.RawValue).IsEqualTo("Resolved");
        }
    }

    [Test]
    public async Task Resolve_PlainValueHandler_AutoWrapsVariant()
    {
        //Arrange
        var data = new OverridableObject();
        var options = CreateOptions(manager => manager.Member(nameof(OverridableObject.GetHeavy)).Evaluate(() => "Plain"));

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(OverridableObject.GetHeavy));
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Value.RawValue).IsEqualTo("Plain");
        }
    }

    [Test]
    public async Task Evaluate_DisabledMember_ReportsDisabledResult()
    {
        //Arrange
        var data = new OverridableObject();
        var options = CreateOptions(manager => manager.Member(nameof(OverridableObject.Delete)).Disable(), MethodEvaluationPolicy.All);

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(OverridableObject.Delete));
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Disabled);
            await Assert.That(member.Evaluator).IsNotNull();
            await Assert.That(member.Value.RawValue).IsNull();
            await Assert.That(data.DeleteCount).IsEqualTo(0);
        }

        member.Evaluate();
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Disabled);
            await Assert.That(member.Value.RawValue).IsTypeOf<InvalidOperationException>();
            await Assert.That(data.DeleteCount).IsEqualTo(0);
            await Assert.That(member.Evaluate).Throws<InvalidOperationException>();
        }
    }

    [Test]
    public async Task Decompose_DisabledParametricMethod_AlwaysIncluded()
    {
        //Arrange
        var data = new OverridableObject();
        var options = CreateOptions(manager => manager.Member(nameof(OverridableObject.WithParameter)).Disable());

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name.StartsWith(nameof(OverridableObject.WithParameter)));
        await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Disabled);
    }

    [Test]
    public async Task Decompose_DeferredParametricMethodWithoutHandler_RemainsUnsupported()
    {
        //Arrange
        var data = new OverridableObject();
        var hiddenOptions = CreateOptions(manager => manager.Member(nameof(OverridableObject.WithParameter)).Defer());
        var includedOptions = CreateOptions(manager => manager.Member(nameof(OverridableObject.WithParameter)).Defer());
        includedOptions.IncludeUnsupported = true;

        //Act
        var hiddenResult = LookupComposer.Decompose(data, hiddenOptions);
        var includedResult = LookupComposer.Decompose(data, includedOptions);

        //Assert
        var member = includedResult.Members.First(member => member.Name.StartsWith(nameof(OverridableObject.WithParameter)));
        using (Assert.Multiple())
        {
            await Assert.That(hiddenResult.Members.Where(member => member.Name.StartsWith(nameof(OverridableObject.WithParameter)))).IsEmpty();
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Unsupported);
            await Assert.That(member.Evaluator).IsNull();
        }
    }

    [Test]
    public async Task Decompose_PerOverloadOverride_DisablesMatchingOverloadOnly()
    {
        //Arrange
        var data = new OverloadedObject();
        var options = CreateOverloadOptions(manager =>
        {
            manager.Member(nameof(OverloadedObject.Get))
                .When(parameters => parameters is [{ParameterType: var type}] && type == typeof(string))
                .Disable();
            manager.Member(nameof(OverloadedObject.Get))
                .When(parameters => parameters is [{ParameterType: var type}] && type == typeof(int))
                .Evaluate(() => "By int");
        });

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var members = result.Members.Where(item => item.Name.StartsWith(nameof(OverloadedObject.Get))).ToList();
        var disabledMember = members.First(item => item.Name.Contains(nameof(String)));
        var evaluatedMember = members.First(item => item.Name.Contains(nameof(Int32)));
        using (Assert.Multiple())
        {
            await Assert.That(disabledMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Disabled);
            await Assert.That(evaluatedMember.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(evaluatedMember.Value.RawValue).IsEqualTo("By int");
        }
    }

    [Test]
    public async Task Decompose_DeferredProperty_EvaluatesByReflection()
    {
        //Arrange
        var data = new OverridablePropertyObject();
        var options = CreatePropertyOptions(manager => manager.Member(nameof(OverridablePropertyObject.Text)).Defer());

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(OverridablePropertyObject.Text));
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);
            await Assert.That(member.Value.RawValue).IsNull();
            await Assert.That(member.Value.TypeName).IsEqualTo(nameof(String));
        }

        member.Evaluate();
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Evaluated);
            await Assert.That(member.Value.RawValue).IsEqualTo("Text");
        }
    }

    [Test]
    public async Task Decompose_DisabledProperty_NeverEvaluated()
    {
        //Arrange
        var data = new OverridablePropertyObject();
        var options = CreatePropertyOptions(manager => manager.Member(nameof(OverridablePropertyObject.Secret)).Disable());

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(OverridablePropertyObject.Secret));
        using (Assert.Multiple())
        {
            await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Disabled);
            await Assert.That(member.Value.RawValue).IsNull();
            await Assert.That(data.SecretReadCount).IsEqualTo(0);
        }

        member.Evaluate();
        using (Assert.Multiple())
        {
            await Assert.That(member.Value.RawValue).IsTypeOf<InvalidOperationException>();
            await Assert.That(data.SecretReadCount).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Evaluate_DeferredContextProperty_InvokesHandlerWithContext()
    {
        //Arrange
        var data = new OverridablePropertyObject();
        var options = new DecomposeOptions<OverrideTestContext>
        {
            Context = new OverrideTestContext(),
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    OverridablePropertyObject => new ContextPropertyConfigurator(),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };

        //Act
        var result = LookupComposer.Decompose(data, options);

        //Assert
        var member = result.Members.First(member => member.Name == nameof(OverridablePropertyObject.ContextResolvable));
        await Assert.That(member.EvaluationPolicy).IsEqualTo(MemberEvaluationPolicy.Deferred);

        member.Evaluate();
        await Assert.That(member.Value.RawValue).IsEqualTo("Context metadata");
    }

    private static DecomposeOptions CreateOptions(Action<IMemberConfigurator> configure, MethodEvaluationPolicy? policy = null)
    {
        return new DecomposeOptions
        {
            EvaluationPolicy = policy ?? MethodEvaluationPolicy.None,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    OverridableObject => new DelegatingConfigurator(configure),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };
    }

    private static DecomposeOptions CreateOverloadOptions(Action<IMemberConfigurator> configure)
    {
        return new DecomposeOptions
        {
            EvaluationPolicy = MethodEvaluationPolicy.All,
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    OverloadedObject => new DelegatingConfigurator(configure),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };
    }

    private static DecomposeOptions CreatePropertyOptions(Action<IMemberConfigurator> configure)
    {
        return new DecomposeOptions
        {
            TypeResolver = (obj, _) =>
            {
                return obj switch
                {
                    OverridablePropertyObject => new DelegatingConfigurator(configure),
                    _ => new ObjectDescriptor(obj)
                };
            }
        };
    }
}

file sealed class OverridableObject
{
    public int RunCount;
    public int DeleteCount;

    public string GetText()
    {
        return "Text";
    }

    public string GetHeavy()
    {
        return "Heavy";
    }

    public void Run()
    {
        RunCount++;
    }

    public string Delete()
    {
        DeleteCount++;
        return "Deleted";
    }

    public string WithParameter(int parameter)
    {
        return parameter.ToString();
    }
}

file sealed class OverloadedObject
{
    public string Get(string key)
    {
        return key;
    }

    public string Get(int index)
    {
        return index.ToString();
    }
}

file sealed class OverridablePropertyObject
{
    public int SecretReadCount;

    public string Text => "Text";
    public string ContextResolvable => "ContextResolvable";

    public string Secret
    {
        get
        {
            SecretReadCount++;
            return "Secret";
        }
    }
}

file sealed class OverrideTestContext
{
    public string Metadata { get; } = "Context metadata";
}

file sealed class DelegatingConfigurator(Action<IMemberConfigurator> configure) : Descriptor, IDescriptorConfigurator
{
    public void Configure(IMemberConfigurator configuration)
    {
        configure.Invoke(configuration);
    }
}

file sealed class ContextPropertyConfigurator : Descriptor, IDescriptorConfigurator<OverrideTestContext>
{
    public void Configure(IMemberConfigurator<OverrideTestContext> manager)
    {
        manager.Member(nameof(OverridablePropertyObject.ContextResolvable)).Defer(context => context.Metadata);
    }
}
