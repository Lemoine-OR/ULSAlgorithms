using System.Text.RegularExpressions;
using ULSAlgorithms.Optimization.External;

namespace ULSAlgorithms.Optimization.Adapters.Gurobi;

/// <summary>
/// Detects and validates Gurobi through the official gurobi_cl executable.
/// </summary>
public sealed partial class GurobiSolverAdapter :
    OptimizationSolverAdapterBase
{
    private readonly ExternalSolverProcessProbe _probe =
        new();

    /// <summary>Initializes the Gurobi adapter.</summary>
    public GurobiSolverAdapter()
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
        "ULSAlgorithms.Solver.Gurobi";

    /// <inheritdoc />
    public override string AdapterName =>
        "ULSAlgorithms Gurobi Adapter";

    /// <inheritdoc />
    public override SolverKind SolverKind =>
        SolverKind.Gurobi;

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
                SolverKind.Gurobi,
                SolverAvailabilityStatus.NotInstalled,
                solverName: "Gurobi Optimizer",
                diagnostics:
                [
                    "gurobi_cl was not found. Put it on PATH, define " +
                    "GUROBI_HOME, or set ULSALGORITHMS_GUROBI_EXECUTABLE."
                ]);
        }

        try
        {
            ExternalSolverProcessProbeResult versionResult =
                await _probe.RunAsync(
                    executablePath,
                    ["--version"],
                    cancellationToken);

            string versionOutput =
                versionResult.CombinedOutput;

            if (versionResult.ExitCode != 0)
            {
                return Failure(
                    executablePath,
                    SolverAvailabilityStatus.LoadFailure,
                    FirstMeaningfulLine(versionOutput) ??
                    $"gurobi_cl exited with code {versionResult.ExitCode}.");
            }

            string version =
                ParseVersion(versionOutput);

            ExternalSolverProcessProbeResult licenseResult =
                await _probe.RunAsync(
                    executablePath,
                    ["--license"],
                    cancellationToken);

            string licenseOutput =
                licenseResult.CombinedOutput;

            if (licenseResult.ExitCode != 0 ||
                LooksLikeLicenseFailure(licenseOutput))
            {
                return new SolverAvailabilityInfo(
                    SolverKind.Gurobi,
                    SolverAvailabilityStatus.LicenseUnavailable,
                    solverName: "Gurobi Optimizer",
                    solverVersion: version,
                    installationPath:
                        Path.GetDirectoryName(executablePath) ??
                        string.Empty,
                    nativeLibraryPath: executablePath,
                    licenseInformation:
                        FirstMeaningfulLine(licenseOutput) ??
                        "Gurobi license check failed.",
                    diagnostics:
                    [
                        FirstMeaningfulLine(licenseOutput) ??
                        "gurobi_cl --license did not report a usable license."
                    ]);
            }

            return new SolverAvailabilityInfo(
                SolverKind.Gurobi,
                SolverAvailabilityStatus.Available,
                solverName: "Gurobi Optimizer",
                solverVersion: version,
                installationPath:
                    Path.GetDirectoryName(executablePath) ??
                    string.Empty,
                nativeLibraryPath: executablePath,
                licenseInformation:
                    FirstMeaningfulLine(licenseOutput) ??
                    "Gurobi license information resolved.",
                diagnostics:
                [
                    $"Gurobi command-line executable detected at " +
                    $"'{executablePath}'.",
                    FirstMeaningfulLine(versionOutput) ??
                    "Gurobi version command completed successfully."
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
                ClassifyFailure(exception),
                exception.Message);
        }
    }

    private static SolverAvailabilityInfo Failure(
        string executablePath,
        SolverAvailabilityStatus status,
        string diagnostic)
    {
        return new SolverAvailabilityInfo(
            SolverKind.Gurobi,
            status,
            solverName: "Gurobi Optimizer",
            installationPath:
                Path.GetDirectoryName(executablePath) ??
                string.Empty,
            nativeLibraryPath: executablePath,
            diagnostics: [diagnostic]);
    }

    private static string ResolveExecutablePath()
    {
        return ExternalSolverExecutableLocator.Resolve(
            [
                "ULSALGORITHMS_GUROBI_EXECUTABLE",
                "LOTSIZING_GUROBI_EXECUTABLE"
            ],
            ["GUROBI_HOME"],
            [
                Path.Combine("bin", "gurobi_cl.exe"),
                Path.Combine("bin", "gurobi_cl")
            ],
            ["gurobi_cl.exe", "gurobi_cl"],
            EnumerateCommonWindowsCandidates());
    }

    private static IEnumerable<string>
        EnumerateCommonWindowsCandidates()
    {
        if (!OperatingSystem.IsWindows())
        {
            yield break;
        }

        string systemDrive =
            Path.GetPathRoot(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.System)) ??
            @"C:\";

        IEnumerable<string> roots;

        try
        {
            roots =
                Directory
                    .EnumerateDirectories(
                        systemDrive,
                        "gurobi*",
                        SearchOption.TopDirectoryOnly)
                    .OrderByDescending(
                        static path =>
                            Path.GetFileName(path),
                        StringComparer.OrdinalIgnoreCase)
                    .ToArray();
        }
        catch
        {
            yield break;
        }

        foreach (string root in roots)
        {
            yield return
                Path.Combine(
                    root,
                    "win64",
                    "bin",
                    "gurobi_cl.exe");

            yield return
                Path.Combine(
                    root,
                    "bin",
                    "gurobi_cl.exe");
        }
    }

    private static bool LooksLikeLicenseFailure(
        string text)
    {
        string[] fragments =
        [
            "no license",
            "license not found",
            "unable to open license",
            "unable to retrieve license",
            "license expired",
            "error 10009",
            "not licensed"
        ];

        return fragments.Any(
            fragment =>
                text.Contains(
                    fragment,
                    StringComparison.OrdinalIgnoreCase));
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
        @"(?:Gurobi(?:\s+Optimizer)?\s+version\s+|Gurobi\s+)(\d+(?:\.\d+){1,3})",
        RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant)]
    private static partial Regex VersionRegex();
}
