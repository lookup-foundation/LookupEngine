namespace LookupEngine.Diagnostic;

/// <summary>
///     Measures a single diagnostic dimension around a member evaluation.
/// </summary>
internal interface IEngineDiagnoser
{
    /// <summary>Marks the start of the monitored region.</summary>
    void StartMonitoring();

    /// <summary>Marks the end of the monitored region.</summary>
    void StopMonitoring();
}