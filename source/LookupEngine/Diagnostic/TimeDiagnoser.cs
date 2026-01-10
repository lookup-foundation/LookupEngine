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

using System.Diagnostics;

namespace LookupEngine.Diagnostic;

/// <summary>
///     The engine diagnoser to measure the evaluation member time
/// </summary>
internal sealed class TimeDiagnoser : IEngineDiagnoser
{
    private long _startTimeStamp;
    private long _endTimeStamp;

    public void StartMonitoring()
    {
        _startTimeStamp = Stopwatch.GetTimestamp();
    }

    public void StopMonitoring()
    {
        _endTimeStamp = Stopwatch.GetTimestamp();
    }

    public TimeSpan GetElapsed()
    {
#if NET
        var elapsed = Stopwatch.GetElapsedTime(_startTimeStamp, _endTimeStamp);
#else
        var tickFrequency = (double) TimeSpan.TicksPerSecond / Stopwatch.Frequency;
        var elapsed = new TimeSpan((long)((_endTimeStamp - _startTimeStamp) * tickFrequency));
#endif
        _startTimeStamp = 0;
        _endTimeStamp = 0;

        return elapsed;
    }
}