namespace ULSAlgorithms.Optimization.External;

/// <summary>
/// Captured result of a short external solver process probe.
/// </summary>
public sealed class ExternalSolverProcessProbeResult
{
    /// <summary>Initializes a process-probe result.</summary>
    public ExternalSolverProcessProbeResult(
        int exitCode,
        string standardOutput,
        string standardError)
    {
        ExitCode = exitCode;
        StandardOutput = standardOutput ?? string.Empty;
        StandardError = standardError ?? string.Empty;
    }

    /// <summary>Gets the native process exit code.</summary>
    public int ExitCode { get; }

    /// <summary>Gets captured standard output.</summary>
    public string StandardOutput { get; }

    /// <summary>Gets captured standard error.</summary>
    public string StandardError { get; }

    /// <summary>Gets stdout and stderr as one diagnostic string.</summary>
    public string CombinedOutput =>
        string.Join(
            Environment.NewLine,
            new[]
            {
                StandardOutput,
                StandardError
            }.Where(
                static text =>
                    !string.IsNullOrWhiteSpace(text)));
}
