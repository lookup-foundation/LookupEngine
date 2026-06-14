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

namespace LookupEngine.Diagnostic;

/// <summary>
///     Measures the bytes allocated on the current thread between <see cref="StartMonitoring"/> and <see cref="StopMonitoring"/>.
/// </summary>
internal sealed class MemoryDiagnoser : IEngineDiagnoser
{
    private long _initialAllocatedBytes;
    private long _finalAllocatedBytes;

    public void StartMonitoring()
    {
        _initialAllocatedBytes = GetTotalAllocatedBytes();
    }

    public void StopMonitoring()
    {
        _finalAllocatedBytes = GetTotalAllocatedBytes();
    }

    public long GetAllocatedBytes()
    {
        var allocatedBytes = _finalAllocatedBytes - _initialAllocatedBytes;

        _finalAllocatedBytes = 0;
        _initialAllocatedBytes = 0;

        return allocatedBytes;
    }

    private static long GetTotalAllocatedBytes()
    {
        // Ref: https://github.com/dotnet/BenchmarkDotNet/blob/master/src/BenchmarkDotNet/Engines/GcStats.cs
        // GC.Collect() slows down the execution a lot, accuracy does not degrade;
        // GC.GetTotalAllocatedBytes() depends heavily on the garbage collection and gives inaccurate results;
        // AppDomain.MonitoringIsEnabled almost does not see memory changes when methods are called.
        // GetAllocatedBytesForCurrentThread is the perfect choice for reflexion calls

        return GC.GetAllocatedBytesForCurrentThread();
    }
}