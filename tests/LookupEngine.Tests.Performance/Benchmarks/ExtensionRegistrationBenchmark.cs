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

namespace LookupEngine.Tests.Performance.Benchmarks;

/// <summary>
///     Benchmarks comparing extension registration approaches:
///     <list type="bullet">
///         <item>Composer: direct Register(name, handler) with NotSupportedException check after evaluation</item>
///         <item>Struct: Define(name) returns a struct builder (zero-alloc)</item>
///         <item>Class: Define(name) returns a class builder (heap allocation per call)</item>
///         <item>StructCachedDelegate: struct builder with a cached Action delegate (one allocation per manager lifetime)</item>
///         <item>StructInterface: struct builder returned via interface (boxing per call)</item>
///     </list>
/// </summary>
public class ExtensionRegistrationBenchmark
{
    [Params(1, 100, 500)]
    public int Count { get; set; }

    private ComposerManager _composerManager = new ComposerManager();
    private StructManager _structManager = new StructManager();
    private ClassManager _classManager = new ClassManager();
    private StructCachedDelegateManager _structCachedDelegateManager = new StructCachedDelegateManager();
    private StructInterfaceManager _structInterfaceManager = new StructInterfaceManager();

    [Benchmark(Baseline = true)]
    public int Composer_DirectRegister()
    {
        _composerManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _composerManager.Register("Extension", () => new Variant(42));
        }

        return _composerManager.MemberCount;
    }

    [Benchmark]
    public int Struct_DefineRegister()
    {
        _structManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structManager.Define("Extension").Register(() => new Variant(42));
        }

        return _structManager.MemberCount;
    }

    [Benchmark]
    public int Class_DefineRegister()
    {
        _classManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _classManager.Define("Extension").Register(() => new Variant(42));
        }

        return _classManager.MemberCount;
    }

    [Benchmark]
    public int StructCachedDelegate_DefineRegister()
    {
        _structCachedDelegateManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structCachedDelegateManager.Define("Extension").Register(() => new Variant(42));
        }

        return _structCachedDelegateManager.MemberCount;
    }

    [Benchmark]
    public int StructInterface_DefineRegister()
    {
        _structInterfaceManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structInterfaceManager.Define("Extension").Register(() => new Variant(42));
        }

        return _structInterfaceManager.MemberCount;
    }

    [Benchmark]
    public int Composer_NotSupported()
    {
        _composerManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _composerManager.Register("Extension", NotSupported);
        }

        return _composerManager.MemberCount;
    }

    [Benchmark]
    public int Struct_AsNotSupported()
    {
        _structManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structManager.Define("Extension").AsNotSupported();
        }

        return _structManager.MemberCount;
    }

    [Benchmark]
    public int Composer_MixedScenario()
    {
        _composerManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _composerManager.Register("Extension", () => new Variant(i * 2));
            _composerManager.Register("Disabled", NotSupported);
        }

        return _composerManager.MemberCount;
    }

    [Benchmark]
    public int Struct_MixedScenario()
    {
        _structManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structManager.Define("Extension").Register(() => new Variant(i * 2));
            _structManager.Define("Disabled").AsNotSupported();
        }

        return _structManager.MemberCount;
    }

    [Benchmark]
    public int Struct_WithMap()
    {
        _structManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structManager.Define("Extension").Map("ApiMethod").Register(() => new Variant(42));
        }

        return _structManager.MemberCount;
    }

    [Benchmark]
    public int Struct_AsStatic()
    {
        _structManager.Reset();
        for (var i = 0; i < Count; i++)
        {
            _structManager.Define("Extension").AsStatic().Register(() => new Variant(42));
        }

        return _structManager.MemberCount;
    }

    [Benchmark]
    public int Struct_TryRegister()
    {
        _structManager.Reset();
        var result = false;
        for (var i = 0; i < Count; i++)
        {
            result = _structManager.Define("Extension").TryRegister(() => new Variant(true));
        }

        return result ? 1 : 0;
    }

    private static Func<Variant> NotSupported { get; } = () => new Variant(new NotSupportedException());
}

/// <summary>
///     Immutable result container used by all benchmark managers
/// </summary>
sealed class Variant(object? value)
{
    public object? Value { get; } = value;
}

/// <summary>
///     Original LookupComposer approach: direct Register(name, handler) with NotSupportedException type check after evaluation
/// </summary>
sealed class ComposerManager
{
    private readonly List<object> _members = new(64);

    public int MemberCount => _members.Count;

    public void Reset() => _members.Clear();

    public void Register(string name, Func<Variant> handler)
    {
        try
        {
            var result = handler();
            if (result.Value is NotSupportedException) return;

            _members.Add(result);
        }
        catch (Exception exception)
        {
            _members.Add(exception);
        }
    }
}

/// <summary>
///     Struct builder approach: Define(name) returns a stack-allocated struct with direct manager reference (zero-alloc)
/// </summary>
sealed class StructManager
{
    private readonly List<object> _members = new(64);

    public int MemberCount => _members.Count;

    public void Reset() => _members.Clear();

    public StructBuilder Define(string name) => new(this, name);

    internal void RegisterExtension(string name, Func<Variant> handler)
    {
        try
        {
            var result = handler();
            _members.Add(result);
        }
        catch (Exception exception)
        {
            _members.Add(exception);
        }
    }

    internal bool TryRegisterExtension(string name, Func<Variant> handler)
    {
        try
        {
            var result = handler();
            _members.Add(result);
            return result.Value is true;
        }
        catch (Exception exception)
        {
            _members.Add(exception);
            return false;
        }
    }

    internal void RegisterNotSupported(string name)
    {
    }

    internal void RegisterDisabled(string name)
    {
    }
}

/// <summary>
///     Struct builder returned by <see cref="StructManager" />
/// </summary>
struct StructBuilder
{
    private readonly StructManager _manager;
    private readonly string _name;
    private string? _apiName;

    public StructBuilder(StructManager manager, string name)
    {
        _manager = manager;
        _name = name;
    }

    public StructBuilder Map(string apiName)
    {
        _apiName = apiName;
        return this;
    }

    public StructBuilder AsStatic()
    {
        return this;
    }

    public void Register(Func<Variant> handler) => _manager.RegisterExtension(_name, handler);

    public bool TryRegister(Func<Variant> handler) => _manager.TryRegisterExtension(_name, handler);

    public void AsNotSupported() => _manager.RegisterNotSupported(_name);

    public void AsDisabled() => _manager.RegisterDisabled(_name);
}

/// <summary>
///     Class builder approach: Define(name) returns a heap-allocated class builder (one allocation per Define call)
/// </summary>
sealed class ClassManager
{
    private readonly List<object> _members = new(64);

    public int MemberCount => _members.Count;

    public void Reset() => _members.Clear();

    public ClassBuilder Define(string name) => new(this, name);

    internal void RegisterExtension(string name, Func<Variant> handler)
    {
        try
        {
            var result = handler();
            _members.Add(result);
        }
        catch (Exception exception)
        {
            _members.Add(exception);
        }
    }
}

/// <summary>
///     Class builder returned by <see cref="ClassManager" />
/// </summary>
sealed class ClassBuilder
{
    private readonly ClassManager _manager;
    private readonly string _name;
    private string? _apiName;

    public ClassBuilder(ClassManager manager, string name)
    {
        _manager = manager;
        _name = name;
    }

    public ClassBuilder Map(string apiName)
    {
        _apiName = apiName;
        return this;
    }

    public ClassBuilder AsStatic()
    {
        return this;
    }

    public void Register(Func<Variant> handler) => _manager.RegisterExtension(_name, handler);
}

/// <summary>
///     Struct builder with cached delegate approach: struct builder uses a cached Action delegate instead of direct manager reference
///     (one delegate allocation per manager lifetime)
/// </summary>
sealed class StructCachedDelegateManager
{
    private readonly List<object> _members = new(64);
    private readonly Action<string, Func<Variant>> _registerCallback;

    public StructCachedDelegateManager()
    {
        _registerCallback = RegisterExtension;
    }

    public int MemberCount => _members.Count;

    public void Reset() => _members.Clear();

    public CachedDelegateBuilder Define(string name) => new(name, _registerCallback);

    private void RegisterExtension(string name, Func<Variant> handler)
    {
        try
        {
            var result = handler();
            _members.Add(result);
        }
        catch (Exception exception)
        {
            _members.Add(exception);
        }
    }
}

/// <summary>
///     Struct builder returned by <see cref="StructCachedDelegateManager" />
/// </summary>
struct CachedDelegateBuilder
{
    private readonly string _name;
    private readonly Action<string, Func<Variant>> _callback;
    private string? _apiName;

    public CachedDelegateBuilder(string name, Action<string, Func<Variant>> callback)
    {
        _name = name;
        _callback = callback;
    }

    public CachedDelegateBuilder Map(string apiName)
    {
        _apiName = apiName;
        return this;
    }

    public CachedDelegateBuilder AsStatic()
    {
        return this;
    }

    public void Register(Func<Variant> handler) => _callback(_name, handler);
}

/// <summary>
///     Struct builder returned via interface approach: Define(name) returns IBuilder causing boxing of the struct (one boxing per Define call)
/// </summary>
sealed class StructInterfaceManager
{
    private readonly List<object> _members = new(64);

    public int MemberCount => _members.Count;

    public void Reset() => _members.Clear();

    public IExtensionBuilder Define(string name) => new InterfaceStructBuilder(this, name);

    internal void RegisterExtension(string name, Func<Variant> handler)
    {
        try
        {
            var result = handler();
            _members.Add(result);
        }
        catch (Exception exception)
        {
            _members.Add(exception);
        }
    }
}

/// <summary>
///     Builder interface for the boxing benchmark scenario
/// </summary>
interface IExtensionBuilder
{
    IExtensionBuilder Map(string apiName);
    IExtensionBuilder AsStatic();
    void Register(Func<Variant> handler);
}

/// <summary>
///     Struct builder implementing <see cref="IExtensionBuilder" /> — boxed when returned from <see cref="StructInterfaceManager.Define" />
/// </summary>
struct InterfaceStructBuilder : IExtensionBuilder
{
    private readonly StructInterfaceManager _manager;
    private readonly string _name;
    private string? _apiName;

    public InterfaceStructBuilder(StructInterfaceManager manager, string name)
    {
        _manager = manager;
        _name = name;
    }

    public IExtensionBuilder Map(string apiName)
    {
        _apiName = apiName;
        return this;
    }

    public IExtensionBuilder AsStatic()
    {
        return this;
    }

    public void Register(Func<Variant> handler) => _manager.RegisterExtension(_name, handler);
}
