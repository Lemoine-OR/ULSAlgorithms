using System.Text.RegularExpressions;
using ULSAlgorithms.Optimization.External;

namespace ULSAlgorithms.Optimization.Adapters.CoinOrCbc;

/// <summary>
/// Detects and validates the stand-alone COIN-OR CBC executable.
/// </summary>
public sealed partial class CoinOrCbcSolverAdapter :
    OptimizationSolverAdapterBase
{
    private readonly ExternalSolverProcessProbe _probe =
        new();

    /// <summary>Initializes the CBC adapter.</summary>
    public CoinOrCbcSolverAdapter()
        : base(
            SolverCapability.LinearProgramming,
            SolverCapability.MixedIntegerLinearProgramming,
            SolverCapability.Interruption,
            SolverCapability.LpExport,
            SolverCapability.OptimalityGapReporting,
            SolverCapability.SearchStatistics)
    {
    }

    /// <inheritdoc />
    public override string AdapterId =>
        "ULSAlgorithms.Solver.CoinOrCbc";

    /// <inheritdoc />
    public override string AdapterName =>
        "ULSAlgorithms COIN-OR CBC Adapter";

    /// <inheritdoc />
    public override SolverKind SolverKind =>
        SolverKind.CoinOrCbc;

    /// <inheritdoc />
    public override async ValueTask<SolverAvailabilityInfo>
        CheckAvailabilityAsync(
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string executablePath =
            ResolveExecutablePath();

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return new SolverAvailabilityInfo(
                SolverKind.CoinOrCbc,
                SolverAvailabilityStatus.NotInstalled,
                solverName: "COIN-OR CBC",
                diagnostics:
                [
                    "cbc executable was not found. Put it on PATH, define " +
                    "CBC_HOME/COINOR_HOME, or set " +
                    "ULSALGORITHMS_CBC_EXECUTABLE."
                ]);
        }

        try
        {
            ExternalSolverProcessProbeResult processResult =
                await _probe.RunAsync(
                    executablePath,
                    ["-quit"],
                    cancellationToken);

            string output =
                processResult.CombinedOutput;

            if (processResult.ExitCode != 0)
            {
                return Failure(
                    executablePath,
                    FirstMeaningfulLine(output) ??
                    $"cbc exited with code {processResult.ExitCode}.");
            }

            return new SolverAvailabilityInfo(
                SolverKind.CoinOrCbc,
                SolverAvailabilityStatus.Available,
                solverName: "COIN-OR CBC",
                solverVersion:
                    ParseVersion(output),
                installationPath:
                    Path.GetDirectoryName(executablePath) ??
                    string.Empty,
                nativeLibraryPath:
                    executablePath,
                licenseInformation:
                    "Open-source solver; no runtime license required.",
                diagnostics:
                [
                    $"CBC executable detected at '{executablePath}'.",
                    FirstMeaningfulLine(output) ??
                    "CBC process probe completed successfully."
                ]);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Failure(
                executablePath,
                exception.Message);
        }
    }

    private static SolverAvailabilityInfo Failure(
        string executablePath,
        string diagnostic)
    {
        return new SolverAvailabilityInfo(
            SolverKind.CoinOrCbc,
            SolverAvailabilityStatus.LoadFailure,
            solverName: "COIN-OR CBC",
            installationPath:
                Path.GetDirectoryName(executablePath) ??
                string.Empty,
            nativeLibraryPath:
                executablePath,
            diagnostics: [diagnostic]);
    }

    private static string ResolveExecutablePath()
    {
        return ExternalSolverExecutableLocator.Resolve(
            [
                "ULSALGORITHMS_CBC_EXECUTABLE",
                "LOTSIZING_CBC_EXECUTABLE"
            ],
            [
                "CBC_HOME",
                "COINOR_HOME"
            ],
            [
                Path.Combine("bin", "cbc.exe"),
                Path.Combine("bin", "cbc"),
                "cbc.exe",
                "cbc"
            ],
            ["cbc.exe", "cbc"],
            EnumerateLocalCandidates());
    }

    private static IEnumerable<string>
        EnumerateLocalCandidates()
    {
        string[] roots =
        [
            AppContext.BaseDirectory,
            Environment.CurrentDirectory
        ];

        foreach (string root in roots)
        {
            yield return
                Path.Combine(root, "cbc.exe");

            yield return
                Path.Combine(root, "cbc");

            yield return
                Path.Combine(
                    root,
                    "cbc",
                    "bin",
                    "cbc.exe");

            yield return
                Path.Combine(
                    root,
                    "tools",
                    "cbc",
                    "bin",
                    "cbc.exe");

            yield return
                Path.Combine(
                    root,
                    "solver",
                    "cbc",
                    "bin",
                    "cbc.exe");
        }
    }

    private static string ParseVersion(
        string text)
    {
        Match match =
            VersionRegex().Match(text ?? string.Empty);

        return match.Success
            ? match.Groups[1].Value
            : string.Empty;
    }

    private static string? FirstMeaningfulLine(
        string text)
    {
        return text
            .Split(
                ['\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .FirstOrDefault();
    }

    [GeneratedRegex(
        @"Version:\s*([0-9]+(?:\.[0-9]+){1,3})",
        RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant)]
    private static partial Regex VersionRegex();
}
