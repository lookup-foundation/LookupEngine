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

public class ExtensionRegistrationBenchmark
{
    [Params(1, 100, 500)]
    public int Count { get; set; }

    private readonly ComposerManager _composerManager = new();
    private readonly StructManager _structManager = new();
    private readonly ClassManager _classManager = new();
    private readonly StructCachedDelegateManager _structCachedDelegateManager = new();
    private readonly StructInterfaceManager _structInterfaceManager = new();

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
public sealed class Variant(object? value)
{
    public object? Value { get; } = value;
}

/// <summary>
///     Original LookupComposer approach: direct Register(name, handler) with NotSupportedException type check after evaluation
/// </summary>
public sealed class ComposerManager
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
public sealed class StructManager
{
    private readonly List<object> _members = new(64);

    public int MemberCount => _members.Count;

    public void Reset() => _members.Clear();

    public StructBuilder Define(string name) => new(this, name);

    public void RegisterExtension(string name, Func<Variant> handler)
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

    public bool TryRegisterExtension(string name, Func<Variant> handler)
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

    public void RegisterNotSupported(string name)
    {
    }

    public void RegisterDisabled(string name)
    {
    }
}

/// <summary>
///     Struct builder returned by <see cref="StructManager" />
/// </summary>
public struct StructBuilder(StructManager manager, string name)
{
    private string? _apiName;

    public StructBuilder Map(string apiName)
    {
        _apiName = apiName;
        return this;
    }

    public StructBuilder AsStatic()
    {
        return this;
    }

    public void Register(Func<Variant> handler) => manager.RegisterExtension(name, handler);

    public bool TryRegister(Func<Variant> handler) => manager.TryRegisterExtension(name, handler);

    public void AsNotSupported() => manager.RegisterNotSupported(name);

    public void AsDisabled() => manager.RegisterDisabled(name);
}

/// <summary>
///     Class builder approach: Define(name) returns a heap-allocated class builder (one allocation per Define call)
/// </summary>
public sealed class ClassManager
{
    private readonly List<object> _members = new(64);

    public int MemberCount => _members.Count;

    public void Reset() => _members.Clear();

    public ClassBuilder Define(string name) => new(this, name);

    public void RegisterExtension(string name, Func<Variant> handler)
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
public sealed class ClassBuilder(ClassManager manager, string name)
{
    private string? _apiName;

    public ClassBuilder Map(string apiName)
    {
        _apiName = apiName;
        return this;
    }

    public ClassBuilder AsStatic()
    {
        return this;
    }

    public void Register(Func<Variant> handler) => manager.RegisterExtension(name, handler);
}

/// <summary>
///     Struct builder with cached delegate approach: struct builder uses a cached Action delegate instead of direct manager reference
///     (one delegate allocation per manager lifetime)
/// </summary>
public sealed class StructCachedDelegateManager
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
public struct CachedDelegateBuilder(string name, Action<string, Func<Variant>> callback)
{
    private string? _apiName;

    public CachedDelegateBuilder Map(string apiName)
    {
        _apiName = apiName;
        return this;
    }

    public CachedDelegateBuilder AsStatic()
    {
        return this;
    }

    public void Register(Func<Variant> handler) => callback(name, handler);
}

/// <summary>
///     Struct builder returned via interface approach: Define(name) returns IBuilder causing boxing of the struct (one boxing per Define call)
/// </summary>
public sealed class StructInterfaceManager
{
    private readonly List<object> _members = new(64);

    public int MemberCount => _members.Count;

    public void Reset() => _members.Clear();

    public IExtensionBuilder Define(string name) => new InterfaceStructBuilder(this, name);

    public void RegisterExtension(string name, Func<Variant> handler)
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
public interface IExtensionBuilder
{
    IExtensionBuilder Map(string apiName);
    IExtensionBuilder AsStatic();
    void Register(Func<Variant> handler);
}

/// <summary>
///     Struct builder implementing <see cref="IExtensionBuilder" /> — boxed when returned from <see cref="StructInterfaceManager.Define" />
/// </summary>
file struct InterfaceStructBuilder(StructInterfaceManager manager, string name) : IExtensionBuilder
{
    private string? _apiName;

    public IExtensionBuilder Map(string apiName)
    {
        _apiName = apiName;
        return this;
    }

    public IExtensionBuilder AsStatic()
    {
        return this;
    }

    public void Register(Func<Variant> handler) => manager.RegisterExtension(name, handler);
}
