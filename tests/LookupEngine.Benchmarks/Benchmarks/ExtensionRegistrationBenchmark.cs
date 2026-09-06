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

namespace LookupEngine.Benchmarks.Benchmarks;

// ```
//
// BenchmarkDotNet v0.15.8, Windows 11 (10.0.28000.2704/26H1/2026Update)
// AMD Ryzen 9 9950X3D 4.30GHz, 1 CPU, 32 logical and 16 physical cores
// .NET SDK 10.0.111
//   [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
//   DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
//
// ```
//
// | Method                              | Count | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
// |------------------------------------ |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
// | **Composer_DirectRegister**             | **1**     |     **17.46 ns** |   **0.120 ns** |   **0.106 ns** |  **0.98** |    **0.01** | **0.0030** |      **-** |     **152 B** |        **1.00** |
// | Struct_DefineRegister               | 1     |     18.15 ns |   0.329 ns |   0.292 ns |  1.02 |    0.02 | 0.0030 |      - |     152 B |        1.00 |
// | Class_DefineRegister                | 1     |     19.64 ns |   0.204 ns |   0.180 ns |  1.10 |    0.01 | 0.0038 |      - |     192 B |        1.26 |
// | StructCachedDelegate_DefineRegister | 1     |     17.81 ns |   0.162 ns |   0.136 ns |  1.00 |    0.01 | 0.0030 |      - |     152 B |        1.00 |
// | StructInterface_DefineRegister      | 1     |     19.78 ns |   0.289 ns |   0.270 ns |  1.11 |    0.02 | 0.0038 |      - |     192 B |        1.26 |
// | Composer_NotSupported               | 1     |     14.61 ns |   0.084 ns |   0.070 ns |  0.82 |    0.01 | 0.0024 |      - |     120 B |        0.79 |
// | Struct_NotSupported                 | 1     |     14.85 ns |   0.286 ns |   0.268 ns |  0.83 |    0.02 | 0.0024 |      - |     120 B |        0.79 |
// | Composer_MixedScenario              | 1     |     28.93 ns |   0.593 ns |   0.582 ns |  1.62 |    0.03 | 0.0054 |      - |     272 B |        1.79 |
// | Struct_MixedScenario                | 1     |     31.23 ns |   0.631 ns |   0.726 ns |  1.75 |    0.04 | 0.0054 |      - |     272 B |        1.79 |
// | Struct_WithMap                      | 1     |     17.68 ns |   0.300 ns |   0.280 ns |  0.99 |    0.02 | 0.0030 |      - |     152 B |        1.00 |
// | Struct_AsStatic                     | 1     |     18.18 ns |   0.231 ns |   0.216 ns |  1.02 |    0.01 | 0.0030 |      - |     152 B |        1.00 |
// |                                     |       |              |            |            |       |         |        |        |           |             |
// | **Composer_DirectRegister**             | **100**   |  **1,509.44 ns** |  **21.692 ns** |  **19.229 ns** |  **1.01** |    **0.02** | **0.3014** | **0.0076** |   **15200 B** |        **1.00** |
// | Struct_DefineRegister               | 100   |  1,513.46 ns |  20.294 ns |  18.983 ns |  1.01 |    0.02 | 0.3014 | 0.0076 |   15200 B |        1.00 |
// | Class_DefineRegister                | 100   |  1,787.65 ns |  23.136 ns |  21.642 ns |  1.20 |    0.02 | 0.3815 | 0.0095 |   19200 B |        1.26 |
// | StructCachedDelegate_DefineRegister | 100   |  1,495.07 ns |  22.076 ns |  20.650 ns |  1.00 |    0.02 | 0.3014 | 0.0076 |   15200 B |        1.00 |
// | StructInterface_DefineRegister      | 100   |  1,764.42 ns |  23.181 ns |  21.684 ns |  1.18 |    0.02 | 0.3815 | 0.0095 |   19200 B |        1.26 |
// | Composer_NotSupported               | 100   |  1,058.00 ns |  11.008 ns |   9.758 ns |  0.71 |    0.01 | 0.2384 | 0.0038 |   12000 B |        0.79 |
// | Struct_NotSupported                 | 100   |  1,088.09 ns |  14.486 ns |  13.550 ns |  0.73 |    0.01 | 0.2384 | 0.0038 |   12000 B |        0.79 |
// | Composer_MixedScenario              | 100   |  2,666.65 ns |  50.634 ns |  47.363 ns |  1.78 |    0.04 | 0.5417 | 0.0267 |   27200 B |        1.79 |
// | Struct_MixedScenario                | 100   |  2,710.76 ns |  33.607 ns |  28.063 ns |  1.81 |    0.03 | 0.5417 | 0.0267 |   27200 B |        1.79 |
// | Struct_WithMap                      | 100   |  1,497.80 ns |  20.341 ns |  18.031 ns |  1.00 |    0.02 | 0.3014 | 0.0076 |   15200 B |        1.00 |
// | Struct_AsStatic                     | 100   |  1,517.65 ns |  15.112 ns |  13.396 ns |  1.02 |    0.02 | 0.3014 | 0.0076 |   15200 B |        1.00 |
// |                                     |       |              |            |            |       |         |        |        |           |             |
// | **Composer_DirectRegister**             | **500**   |  **7,351.18 ns** |  **67.551 ns** |  **63.187 ns** |  **0.97** |    **0.02** | **1.5106** | **0.1907** |   **76000 B** |        **1.00** |
// | Struct_DefineRegister               | 500   |  7,464.42 ns |  79.623 ns |  70.584 ns |  0.99 |    0.02 | 1.5106 | 0.1907 |   76000 B |        1.00 |
// | Class_DefineRegister                | 500   |  8,606.03 ns |  76.942 ns |  64.250 ns |  1.14 |    0.02 | 1.9073 | 0.2289 |   96000 B |        1.26 |
// | StructCachedDelegate_DefineRegister | 500   |  7,564.18 ns | 130.932 ns | 122.474 ns |  1.00 |    0.02 | 1.5106 | 0.1831 |   76000 B |        1.00 |
// | StructInterface_DefineRegister      | 500   |  8,694.30 ns |  89.949 ns |  79.738 ns |  1.15 |    0.02 | 1.9073 | 0.2289 |   96000 B |        1.26 |
// | Composer_NotSupported               | 500   |  5,641.15 ns |  64.040 ns |  59.903 ns |  0.75 |    0.01 | 1.1902 | 0.1221 |   60000 B |        0.79 |
// | Struct_NotSupported                 | 500   |  5,679.29 ns | 112.389 ns | 115.415 ns |  0.75 |    0.02 | 1.1902 | 0.1221 |   60000 B |        0.79 |
// | Composer_MixedScenario              | 500   | 12,979.20 ns | 117.813 ns |  98.379 ns |  1.72 |    0.03 | 2.7008 | 0.5646 |  136000 B |        1.79 |
// | Struct_MixedScenario                | 500   | 13,605.26 ns | 268.109 ns | 275.329 ns |  1.80 |    0.05 | 2.7008 | 0.5188 |  136000 B |        1.79 |
// | Struct_WithMap                      | 500   |  7,561.85 ns |  90.655 ns |  84.799 ns |  1.00 |    0.02 | 1.5106 | 0.1907 |   76000 B |        1.00 |
// | Struct_AsStatic                     | 500   |  7,656.64 ns | 101.809 ns |  95.232 ns |  1.01 |    0.02 | 1.5106 | 0.1907 |   76000 B |        1.00 |

/// <summary>
///     Compares builder shapes for the deferred extension-registration model used by the engine.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ExtensionRegistrationBenchmark
{
    private readonly ClassManager _classManager = new();

    private readonly ComposerManager _composerManager = new();
    private readonly StructCachedDelegateManager _structCachedDelegateManager = new();
    private readonly StructInterfaceManager _structInterfaceManager = new();
    private readonly StructManager _structManager = new();

    [Params(1, 100, 500)] public int Count { get; set; }

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
[PublicAPI]
public enum MemberAttributes
{
    Static = 0b10,
    Extension = 0b100000
}

/// <summary>
///     The evaluation policy recorded for a non-evaluated extension.
/// </summary>
[PublicAPI]
public enum MemberEvaluationPolicy
{
    Evaluated = 0,
    Disabled = 2,
    Unsupported = 3
}

/// <summary>
///     Immutable result container used by all benchmark managers.
/// </summary>
[PublicAPI]
public sealed class Variant(object? value)
{
    public object? Value { get; } = value;
}

/// <summary>
///     Direct registration without a builder: callers pass the attributes explicitly and the manager enqueues a closure that is evaluated on flush.
///     Establishes the cost floor of the deferred model.
/// </summary>
[PublicAPI]
public sealed class ComposerManager
{
    private readonly List<Action> _extensionQueue = new(64);
    private readonly bool _includeStatic = true;
    private readonly bool _includeUnsupported = true;
    private readonly List<object> _members = new(64);

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
            if ((attributes & MemberAttributes.Static) != 0 && !_includeStatic)
            {
                return;
            }

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
            if (!_includeUnsupported)
            {
                return;
            }

            if ((attributes & MemberAttributes.Static) != 0 && !_includeStatic)
            {
                return;
            }

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
[PublicAPI]
public sealed class StructManager
{
    private readonly List<Action> _extensionQueue = new(64);
    private readonly bool _includeStatic = true;
    private readonly bool _includeUnsupported = true;
    private readonly List<object> _members = new(64);

    public int MemberCount => _members.Count;

    public void Reset()
    {
        _members.Clear();
        _extensionQueue.Clear();
    }

    public StructBuilder Define(string name)
    {
        return new StructBuilder(this, name);
    }

    public void EnqueueExtension(string name, MemberAttributes attributes, Func<Variant> handler)
    {
        _extensionQueue.Add(() =>
        {
            if ((attributes & MemberAttributes.Static) != 0 && !_includeStatic)
            {
                return;
            }

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
            if (!_includeUnsupported)
            {
                return;
            }

            if ((attributes & MemberAttributes.Static) != 0 && !_includeStatic)
            {
                return;
            }

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
[PublicAPI]
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

    public readonly void Register(Func<Variant> handler)
    {
        manager.EnqueueExtension(name, _attributes, handler);
    }

    public readonly void NotSupported()
    {
        manager.EnqueueExtensionResult(name, _attributes, MemberEvaluationPolicy.Unsupported);
    }

    public readonly void Disable()
    {
        manager.EnqueueExtensionResult(name, _attributes, MemberEvaluationPolicy.Disabled);
    }
}

/// <summary>
///     Class builder approach: <c>Define(name)</c> returns a heap-allocated class builder (one allocation per <c>Define</c> call).
/// </summary>
[PublicAPI]
public sealed class ClassManager
{
    private readonly List<Action> _extensionQueue = new(64);
    private readonly bool __includeStatic = true;
    private readonly List<object> _members = new(64);

    public int MemberCount => _members.Count;

    public void Reset()
    {
        _members.Clear();
        _extensionQueue.Clear();
    }

    public ClassBuilder Define(string name)
    {
        return new ClassBuilder(this, name);
    }

    public void EnqueueExtension(string name, MemberAttributes attributes, Func<Variant> handler)
    {
        _extensionQueue.Add(() =>
        {
            if ((attributes & MemberAttributes.Static) != 0 && !__includeStatic)
            {
                return;
            }

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
[PublicAPI]
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

    public void Register(Func<Variant> handler)
    {
        manager.EnqueueExtension(name, _attributes, handler);
    }
}

/// <summary>
///     Struct builder with two cached delegates instead of a direct manager reference (the shape used by the engine): the register and result callbacks are allocated once per manager lifetime, not per <c>Define</c> call.
/// </summary>
[PublicAPI]
public sealed class StructCachedDelegateManager
{
    private readonly List<Action> _extensionQueue = new(64);
    private readonly bool __includeStatic = true;
    private readonly bool _includeUnsupported = true;
    private readonly List<object> _members = new(64);
    private readonly Action<string, MemberAttributes, Func<Variant>> _registerCallback;
    private readonly Action<string, MemberAttributes, MemberEvaluationPolicy> _registerResultCallback;

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

    public CachedDelegateBuilder Define(string name)
    {
        return new CachedDelegateBuilder(name, _registerCallback, _registerResultCallback);
    }

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
            if ((attributes & MemberAttributes.Static) != 0 && !__includeStatic)
            {
                return;
            }

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
            if (!_includeUnsupported)
            {
                return;
            }

            if ((attributes & MemberAttributes.Static) != 0 && !__includeStatic)
            {
                return;
            }

            _members.Add(policy);
        });
    }
}

/// <summary>
///     Struct builder returned by <see cref="StructCachedDelegateManager" />.
/// </summary>
[PublicAPI]
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

    public readonly void Register(Func<Variant> handler)
    {
        registerCallback(name, _attributes, handler);
    }

    public readonly void NotSupported()
    {
        registerResultCallback(name, _attributes, MemberEvaluationPolicy.Unsupported);
    }

    public readonly void Disable()
    {
        registerResultCallback(name, _attributes, MemberEvaluationPolicy.Disabled);
    }
}

/// <summary>
///     Extension manager where <c>Define(name)</c> returns an <see cref="IExtensionBuilder" />, which boxes the struct (one allocation per <c>Define</c> call).
/// </summary>
[PublicAPI]
public sealed class StructInterfaceManager
{
    private readonly List<Action> _extensionQueue = new(64);
    private readonly bool __includeStatic = true;
    private readonly List<object> _members = new(64);

    public int MemberCount => _members.Count;

    public void Reset()
    {
        _members.Clear();
        _extensionQueue.Clear();
    }

    public IExtensionBuilder Define(string name)
    {
        return new InterfaceStructBuilder(this, name);
    }

    public void EnqueueExtension(string name, MemberAttributes attributes, Func<Variant> handler)
    {
        _extensionQueue.Add(() =>
        {
            if ((attributes & MemberAttributes.Static) != 0 && !__includeStatic)
            {
                return;
            }

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
[PublicAPI]
public interface IExtensionBuilder
{
    IExtensionBuilder Map(string apiName);
    IExtensionBuilder AsStatic();
    void Register(Func<Variant> handler);
}

/// <summary>
///     Struct builder that implements <see cref="IExtensionBuilder" /> — boxed when returned from <see cref="StructInterfaceManager.Define" />.
/// </summary>
[PublicAPI]
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

    public void Register(Func<Variant> handler)
    {
        manager.EnqueueExtension(name, _attributes, handler);
    }
}
