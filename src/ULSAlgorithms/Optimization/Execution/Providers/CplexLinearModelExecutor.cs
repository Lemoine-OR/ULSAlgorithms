using System.Diagnostics;
using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Optimization.Execution.Providers;

/// <summary>
/// Executes portable LP/MILP models through the stand-alone CPLEX optimizer.
/// </summary>
/// <remarks>
/// Availability is still validated by the dynamic CPLEX runtime adapter. The
/// execution backend then invokes the cplex command-line program from the same
/// runtime directory and parses its XML .sol file.
/// </remarks>
public sealed class CplexLinearModelExecutor :
    ILinearModelSolverExecutor
{
    private readonly ExternalSolverProcessRunner _runner =
        new();

    /// <inheritdoc />
    public SolverKind SolverKind =>
        SolverKind.Cplex;

    /// <inheritdoc />
    public async ValueTask<LinearModelSolveResult> SolveAsync(
        LinearModel model,
        SolverSelectionResult selection,
        LinearModelSolveOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(options);

        if (selection.SelectedSolver != SolverKind.Cplex ||
            selection.Availability is null)
        {
            throw new ArgumentException(
                "The supplied selection does not target CPLEX.",
                nameof(selection));
        }

        string executable =
            ResolveCplexExecutable(
                selection.Availability);

        if (string.IsNullOrWhiteSpace(executable))
        {
            return new LinearModelSolveResult(
                model.Name,
                LinearModelSolveStatus.Failed,
                new SolverExecutionInfo(selection),
                null,
                null,
                TimeSpan.Zero,
                "cplex executable unavailable",
                [
                    "CPLEX runtime discovery succeeded, but the stand-alone " +
                    "cplex executable was not found in the selected runtime " +
                    "directory."
                ]);
        }

        string artifacts =
            SolverExecutionUtilities.CreateArtifactDirectory(
                SolverKind,
                options);

        var stopwatch =
            Stopwatch.StartNew();

        try
        {
            string modelPath =
                Path.Combine(
                    artifacts,
                    "model.lp");

            string solutionPath =
                Path.Combine(
                    artifacts,
                    "solution.sol");

            new PortableLpModelWriter().Write(
                model,
                modelPath);

            SolverExecutionUtilities.ExportModelIfRequested(
                modelPath,
                options);

            string commands =
                string.Join(
                    Environment.NewLine,
                    [
                        $"read {Quote(modelPath)}",
                        "optimize",
                        $"write {Quote(solutionPath)}",
                        "quit",
                        string.Empty
                    ]);

            ExternalSolverProcessResult process =
                await _runner.RunAsync(
                    executable,
                    [],
                    artifacts,
                    commands,
                    cancellationToken);

            stopwatch.Stop();

            string output =
                process.CombinedOutput;

            CplexXmlSolution solution =
                CplexXmlSolutionParser.Parse(
                    solutionPath);

            bool solutionExists =
                File.Exists(solutionPath);

            LinearModelSolveStatus status =
                MapStatus(
                    solution.Status,
                    output,
                    solutionExists,
                    process.ExitCode);

            return SolverExecutionUtilities.BuildSolutionResult(
                model,
                selection,
                options,
                status,
                solution.Values,
                solutionExists,
                stopwatch.Elapsed,
                !string.IsNullOrWhiteSpace(solution.Status)
                    ? solution.Status
                    : SolverExecutionUtilities.LastMeaningfulLine(output),
                [
                    $"cplex exit code: {process.ExitCode}.",
                    !string.IsNullOrWhiteSpace(solution.Status)
                        ? $"CPLEX solution status: {solution.Status}."
                        : SolverExecutionUtilities.LastMeaningfulLine(output)
                ],
                artifacts);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            return new LinearModelSolveResult(
                model.Name,
                LinearModelSolveStatus.Cancelled,
                new SolverExecutionInfo(selection),
                null,
                null,
                stopwatch.Elapsed,
                "Cancelled",
                ["CPLEX execution was cancelled by the caller."],
                options.KeepTemporaryFiles
                    ? artifacts
                    : string.Empty);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            return new LinearModelSolveResult(
                model.Name,
                LinearModelSolveStatus.Failed,
                new SolverExecutionInfo(selection),
                null,
                null,
                stopwatch.Elapsed,
                exception.GetType().Name,
                [exception.Message],
                options.KeepTemporaryFiles
                    ? artifacts
                    : string.Empty);
        }
        finally
        {
            SolverExecutionUtilities.DeleteArtifactsUnlessRetained(
                artifacts,
                options);
        }
    }

    private static string ResolveCplexExecutable(
        SolverAvailabilityInfo availability)
    {
        string directory =
            availability.NativeLibraryPath;

        if (string.IsNullOrWhiteSpace(directory))
        {
            directory =
                Path.GetDirectoryName(
                    availability.ManagedAssemblyPath) ??
                string.Empty;
        }

        if (string.IsNullOrWhiteSpace(directory))
        {
            return string.Empty;
        }

        string[] candidates =
            OperatingSystem.IsWindows()
                ? ["cplex.exe", "cplex"]
                : ["cplex", "cplex.bin"];

        foreach (string name in candidates)
        {
            string path =
                Path.Combine(
                    directory,
                    name);

            if (File.Exists(path))
            {
                return Path.GetFullPath(path);
            }
        }

        return string.Empty;
    }

    private static string Quote(
        string path) =>
        "\"" +
        path.Replace(
            "\"",
            "\"\"",
            StringComparison.Ordinal) +
        "\"";

    private static LinearModelSolveStatus MapStatus(
        string nativeStatus,
        string output,
        bool solutionExists,
        int exitCode)
    {
        // The status stored in the CPLEX XML solution is authoritative.
        // Console logs can legitimately contain words such as "infeasible"
        // while reporting presolve statistics, MIP starts, node information,
        // or intermediate incumbents.  Searching the complete console output
        // before the native status can therefore misclassify a valid solution.
        if (!string.IsNullOrWhiteSpace(nativeStatus))
        {
            string native =
                nativeStatus.Trim();

            if (native.Contains(
                    "infeasible or unbounded",
                    StringComparison.OrdinalIgnoreCase))
            {
                return LinearModelSolveStatus.InfeasibleOrUnbounded;
            }

            // CPXMIP_OPTIMAL_TOL / "integer optimal, tolerance":
            // feasible incumbent accepted within MIP gap tolerances, but
            // global optimality is not proved.
            if (native.Contains(
                    "optimal, tolerance",
                    StringComparison.OrdinalIgnoreCase) ||
                native.Contains(
                    "optimal within tolerance",
                    StringComparison.OrdinalIgnoreCase))
            {
                return LinearModelSolveStatus.Feasible;
            }

            if (native.Contains(
                    "optimal",
                    StringComparison.OrdinalIgnoreCase))
            {
                return LinearModelSolveStatus.Optimal;
            }

            if (native.Contains(
                    "infeasible",
                    StringComparison.OrdinalIgnoreCase))
            {
                return LinearModelSolveStatus.Infeasible;
            }

            if (native.Contains(
                    "unbounded",
                    StringComparison.OrdinalIgnoreCase))
            {
                return LinearModelSolveStatus.Unbounded;
            }
        }

        // Fallback only when the XML/native status is absent or unrecognized.
        if (output.Contains(
                "infeasible or unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.InfeasibleOrUnbounded;
        }

        if (output.Contains(
                "optimal, tolerance",
                StringComparison.OrdinalIgnoreCase) ||
            output.Contains(
                "optimal within tolerance",
                StringComparison.OrdinalIgnoreCase))
        {
            return solutionExists
                ? LinearModelSolveStatus.Feasible
                : LinearModelSolveStatus.Unknown;
        }

        if (output.Contains(
                "optimal",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Optimal;
        }

        if (output.Contains(
                "infeasible",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Infeasible;
        }

        if (output.Contains(
                "unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Unbounded;
        }

        if (solutionExists)
        {
            return LinearModelSolveStatus.Feasible;
        }

        return exitCode == 0
            ? LinearModelSolveStatus.Unknown
            : LinearModelSolveStatus.Failed;
    }
}
