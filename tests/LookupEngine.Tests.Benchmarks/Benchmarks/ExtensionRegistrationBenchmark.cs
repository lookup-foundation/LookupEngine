// Copyright (c) Lookup Foundation and Contributors
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// THIS PROGRAM IS PROVIDED "AS IS" AND WITH ALL FAULTS.
// NO IMPLIED WARRANTY OF MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE IS PROVIDED.
// THERE IS NO GUARANTEE THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.

using BenchmarkDotNet.Attributes;

namespace LookupEngine.Tests.Benchmarks.Benchmarks;

/// <summary>
///     Compares builder shapes for the deferred extension-registration model used by the engine.
/// </summary>
public class ExtensionRegistrationBenchmark
{
    [Params(1, 100, 500)]
    public int Count { get; set; }

    private readonly ComposerManager _composerManager = new();
    private readonly StructManager _structManager = new();
    private readonly ClassManager _classManager = new();
    private readonly StructCachedDelegateManager _structCachedDelegateManager = new();
    private readonly StructInterfaceManager _structInterfaceManager = new();

    [Benchmark]
    public int Composer_DirectRegister()
    {
        _composerManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _composerManager.EnqueueExtension("Extension", MemberAttributes.Extension, static () => new Variant(42));
        }

        _composerManager.Flush();
        return _composerManager.MemberCount;
    }

    [Benchmark]
    public int Struct_DefineRegister()
    {
        _structManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structManager.Define("Extension").Register(static () => new Variant(42));
        }

        _structManager.Flush();
        return _structManager.MemberCount;
    }

    [Benchmark]
    public int Class_DefineRegister()
    {
        _classManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _classManager.Define("Extension").Register(static () => new Variant(42));
        }

        _classManager.Flush();
        return _classManager.MemberCount;
    }

    [Benchmark(Baseline = true)]
    public int StructCachedDelegate_DefineRegister()
    {
        _structCachedDelegateManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structCachedDelegateManager.Define("Extension").Register(static () => new Variant(42));
        }

        _structCachedDelegateManager.Flush();
        return _structCachedDelegateManager.MemberCount;
    }

    [Benchmark]
    public int StructInterface_DefineRegister()
    {
        _structInterfaceManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structInterfaceManager.Define("Extension").Register(static () => new Variant(42));
        }

        _structInterfaceManager.Flush();
        return _structInterfaceManager.MemberCount;
    }

    [Benchmark]
    public int Composer_NotSupported()
    {
        _composerManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _composerManager.EnqueueExtensionResult("Extension", MemberAttributes.Extension, MemberEvaluationPolicy.Unsupported);
        }

        _composerManager.Flush();
        return _composerManager.MemberCount;
    }

    [Benchmark]
    public int Struct_NotSupported()
    {
        _structManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structManager.Define("Extension").NotSupported();
        }

        _structManager.Flush();
        return _structManager.MemberCount;
    }

    [Benchmark]
    public int Composer_MixedScenario()
    {
        _composerManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _composerManager.EnqueueExtension("Extension", MemberAttributes.Extension, static () => new Variant(42));
            _composerManager.EnqueueExtensionResult("Disabled", MemberAttributes.Extension, MemberEvaluationPolicy.Disabled);
        }

        _composerManager.Flush();
        return _composerManager.MemberCount;
    }

    [Benchmark]
    public int Struct_MixedScenario()
    {
        _structManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structManager.Define("Extension").Register(static () => new Variant(42));
            _structManager.Define("Disabled").Disable();
        }

        _structManager.Flush();
        return _structManager.MemberCount;
    }

    [Benchmark]
    public int Struct_WithMap()
    {
        _structManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structManager.Define("Extension").Map("ApiMethod").Register(static () => new Variant(42));
        }

        _structManager.Flush();
        return _structManager.MemberCount;
    }

    [Benchmark]
    public int Struct_AsStatic()
    {
        _structManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structManager.Define("Extension").AsStatic().Register(static () => new Variant(42));
        }

        _structManager.Flush();
        return _structManager.MemberCount;
    }
}

/// <summary>
///     The member attributes threaded through the builder into the queued registration.
/// </summary>
[Flags]
public enum MemberAttributes
{
    Static = 0b10,
    Extension = 0b100000
}

/// <summary>
///     The evaluation policy recorded for a non-evaluated extension.
/// </summary>
public enum MemberEvaluationPolicy
{
    Evaluated = 0,
    Disabled = 2,
    Unsupported = 3
}

/// <summary>
///     Immutable result container used by all benchmark managers.
/// </summary>
public sealed class Variant(object? value)
{
    public object? Value { get; } = value;
}

/// <summary>
///     Direct registration without a builder: callers pass the attributes explicitly and the manager enqueues a closure that is evaluated on flush.
///     Establishes the cost floor of the deferred model.
/// </summary>
public sealed class ComposerManager
{
    private readonly List<object> _members = new(64);
    private readonly List<Action> _extensionQueue = new(64);
    private readonly bool _includeStatic = true;
    private readonly bool _includeUnsupported = true;

    public int MemberCount => _members.Count;

    public void Reset()
    {
        _members.Clear();
        _extensionQueue.Clear();
    }

    public void EnqueueExtension(string name, MemberAttributes attributes, Func<Variant> handler)
    {
        _extensionQueue.Add(() =>
        {
            if ((attributes & MemberAttributes.Static) != 0 && !_includeStatic) return;

            try
            {
                var result = handler();
                _members.Add(result);
            }
            catch (Exception exception)
            {
                _members.Add(exception);
            }
        });
    }

    public void EnqueueExtensionResult(string name, MemberAttributes attributes, MemberEvaluationPolicy policy)
    {
        _extensionQueue.Add(() =>
        {
            if (!_includeUnsupported) return;
            if ((attributes & MemberAttributes.Static) != 0 && !_includeStatic) return;

            _members.Add(policy);
        });
    }

    public void Flush()
    {
        foreach (var registration in _extensionQueue)
        {
            registration.Invoke();
        }
    }
}

/// <summary>
///     Struct builder approach: <c>Define(name)</c> returns a stack-allocated struct that holds a direct manager reference (zero builder allocation).
///     Each registration enqueues a closure.
/// </summary>
public sealed class StructManager
{
    private readonly List<object> _members = new(64);
    private readonly List<Action> _extensionQueue = new(64);
    private readonly bool _includeStatic = true;
    private readonly bool _includeUnsupported = true;

    public int MemberCount => _members.Count;

    public void Reset()
    {
        _members.Clear();
        _extensionQueue.Clear();
    }

    public StructBuilder Define(string name) => new(this, name);

    public void EnqueueExtension(string name, MemberAttributes attributes, Func<Variant> handler)
    {
        _extensionQueue.Add(() =>
        {
            if ((attributes & MemberAttributes.Static) != 0 && !_includeStatic) return;

            try
            {
                var result = handler();
                _members.Add(result);
            }
            catch (Exception exception)
            {
                _members.Add(exception);
            }
        });
    }

    public void EnqueueExtensionResult(string name, MemberAttributes attributes, MemberEvaluationPolicy policy)
    {
        _extensionQueue.Add(() =>
        {
            if (!_includeUnsupported) return;
            if ((attributes & MemberAttributes.Static) != 0 && !_includeStatic) return;

            _members.Add(policy);
        });
    }

    public void Flush()
    {
        foreach (var registration in _extensionQueue)
        {
            registration.Invoke();
        }
    }
}

/// <summary>
///     Struct builder returned by <see cref="StructManager" />, with attributes mutated by fluent calls.
/// </summary>
public struct StructBuilder(StructManager manager, string name)
{
    private MemberAttributes _attributes = MemberAttributes.Extension;

    public StructBuilder Map(string apiName)
    {
        return this;
    }

    public StructBuilder AsStatic()
    {
        _attributes |= MemberAttributes.Static;
        return this;
    }

    public readonly void Register(Func<Variant> handler) => manager.EnqueueExtension(name, _attributes, handler);

    public readonly void NotSupported() => manager.EnqueueExtensionResult(name, _attributes, MemberEvaluationPolicy.Unsupported);

    public readonly void Disable() => manager.EnqueueExtensionResult(name, _attributes, MemberEvaluationPolicy.Disabled);
}

/// <summary>
///     Class builder approach: <c>Define(name)</c> returns a heap-allocated class builder (one allocation per <c>Define</c> call).
/// </summary>
public sealed class ClassManager
{
    private readonly List<object> _members = new(64);
    private readonly List<Action> _extensionQueue = new(64);
    private readonly bool _includeStatic = true;

    public int MemberCount => _members.Count;

    public void Reset()
    {
        _members.Clear();
        _extensionQueue.Clear();
    }

    public ClassBuilder Define(string name) => new(this, name);

    public void EnqueueExtension(string name, MemberAttributes attributes, Func<Variant> handler)
    {
        _extensionQueue.Add(() =>
        {
            if ((attributes & MemberAttributes.Static) != 0 && !_includeStatic) return;

            try
            {
                var result = handler();
                _members.Add(result);
            }
            catch (Exception exception)
            {
                _members.Add(exception);
            }
        });
    }

    public void Flush()
    {
        foreach (var registration in _extensionQueue)
        {
            registration.Invoke();
        }
    }
}

/// <summary>
///     Class builder returned by <see cref="ClassManager" />.
/// </summary>
public sealed class ClassBuilder(ClassManager manager, string name)
{
    private MemberAttributes _attributes = MemberAttributes.Extension;

    public ClassBuilder Map(string apiName)
    {
        return this;
    }

    public ClassBuilder AsStatic()
    {
        _attributes |= MemberAttributes.Static;
        return this;
    }

    public void Register(Func<Variant> handler) => manager.EnqueueExtension(name, _attributes, handler);
}

/// <summary>
///     Struct builder with two cached delegates instead of a direct manager reference (the shape used by the engine): the register and result callbacks are allocated once per manager lifetime, not per <c>Define</c> call.
/// </summary>
public sealed class StructCachedDelegateManager
{
    private readonly List<object> _members = new(64);
    private readonly List<Action> _extensionQueue = new(64);
    private readonly Action<string, MemberAttributes, Func<Variant>> _registerCallback;
    private readonly Action<string, MemberAttributes, MemberEvaluationPolicy> _registerResultCallback;
    private readonly bool _includeStatic = true;
    private readonly bool _includeUnsupported = true;

    public StructCachedDelegateManager()
    {
        _registerCallback = EnqueueExtension;
        _registerResultCallback = EnqueueExtensionResult;
    }

    public int MemberCount => _members.Count;

    public void Reset()
    {
        _members.Clear();
        _extensionQueue.Clear();
    }

    public CachedDelegateBuilder Define(string name) => new(name, _registerCallback, _registerResultCallback);

    public void Flush()
    {
        foreach (var registration in _extensionQueue)
        {
            registration.Invoke();
        }
    }

    private void EnqueueExtension(string name, MemberAttributes attributes, Func<Variant> handler)
    {
        _extensionQueue.Add(() =>
        {
            if ((attributes & MemberAttributes.Static) != 0 && !_includeStatic) return;

            try
            {
                var result = handler();
                _members.Add(result);
            }
            catch (Exception exception)
            {
                _members.Add(exception);
            }
        });
    }

    private void EnqueueExtensionResult(string name, MemberAttributes attributes, MemberEvaluationPolicy policy)
    {
        _extensionQueue.Add(() =>
        {
            if (!_includeUnsupported) return;
            if ((attributes & MemberAttributes.Static) != 0 && !_includeStatic) return;

            _members.Add(policy);
        });
    }
}

/// <summary>
///     Struct builder returned by <see cref="StructCachedDelegateManager" />.
/// </summary>
public struct CachedDelegateBuilder(
    string name,
    Action<string, MemberAttributes, Func<Variant>> registerCallback,
    Action<string, MemberAttributes, MemberEvaluationPolicy> registerResultCallback)
{
    private MemberAttributes _attributes = MemberAttributes.Extension;

    public CachedDelegateBuilder Map(string apiName)
    {
        return this;
    }

    public CachedDelegateBuilder AsStatic()
    {
        _attributes |= MemberAttributes.Static;
        return this;
    }

    public readonly void Register(Func<Variant> handler) => registerCallback(name, _attributes, handler);

    public readonly void NotSupported() => registerResultCallback(name, _attributes, MemberEvaluationPolicy.Unsupported);

    public readonly void Disable() => registerResultCallback(name, _attributes, MemberEvaluationPolicy.Disabled);
}

/// <summary>
///     Extension manager where <c>Define(name)</c> returns an <see cref="IExtensionBuilder" />, which boxes the struct (one allocation per <c>Define</c> call).
/// </summary>
public sealed class StructInterfaceManager
{
    private readonly List<object> _members = new(64);
    private readonly List<Action> _extensionQueue = new(64);
    private readonly bool _includeStatic = true;

    public int MemberCount => _members.Count;

    public void Reset()
    {
        _members.Clear();
        _extensionQueue.Clear();
    }

    public IExtensionBuilder Define(string name) => new InterfaceStructBuilder(this, name);

    public void EnqueueExtension(string name, MemberAttributes attributes, Func<Variant> handler)
    {
        _extensionQueue.Add(() =>
        {
            if ((attributes & MemberAttributes.Static) != 0 && !_includeStatic) return;

            try
            {
                var result = handler();
                _members.Add(result);
            }
            catch (Exception exception)
            {
                _members.Add(exception);
            }
        });
    }

    public void Flush()
    {
        foreach (var registration in _extensionQueue)
        {
            registration.Invoke();
        }
    }
}

/// <summary>
///     Builder interface for the struct-box benchmark scenario.
/// </summary>
public interface IExtensionBuilder
{
    IExtensionBuilder Map(string apiName);
    IExtensionBuilder AsStatic();
    void Register(Func<Variant> handler);
}

/// <summary>
///     Struct builder that implements <see cref="IExtensionBuilder" /> — boxed when returned from <see cref="StructInterfaceManager.Define" />.
/// </summary>
file struct InterfaceStructBuilder(StructInterfaceManager manager, string name) : IExtensionBuilder
{
    private MemberAttributes _attributes = MemberAttributes.Extension;

    public IExtensionBuilder Map(string apiName)
    {
        return this;
    }

    public IExtensionBuilder AsStatic()
    {
        _attributes |= MemberAttributes.Static;
        return this;
    }

    public void Register(Func<Variant> handler) => manager.EnqueueExtension(name, _attributes, handler);
}
