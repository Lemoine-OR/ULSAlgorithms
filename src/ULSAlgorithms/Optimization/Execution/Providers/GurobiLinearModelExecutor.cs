using System.Diagnostics;
using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Optimization.Execution.Providers;

/// <summary>
/// Executes portable LP/MILP models through the official gurobi_cl executable.
/// </summary>
public sealed class GurobiLinearModelExecutor :
    ILinearModelSolverExecutor
{
    private readonly ExternalSolverProcessRunner _runner =
        new();

    /// <inheritdoc />
    public SolverKind SolverKind =>
        SolverKind.Gurobi;

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

        if (selection.SelectedSolver != SolverKind.Gurobi ||
            selection.Availability is null)
        {
            throw new ArgumentException(
                "The supplied selection does not target Gurobi.",
                nameof(selection));
        }

        string executable =
            selection.Availability.NativeLibraryPath;

        if (string.IsNullOrWhiteSpace(executable) ||
            !File.Exists(executable))
        {
            return Failure(
                model,
                selection,
                "The selected Gurobi executable is no longer available.");
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

            ExternalSolverProcessResult process =
                await _runner.RunAsync(
                    executable,
                    [
                        $"ResultFile={solutionPath}",
                        modelPath
                    ],
                    artifacts,
                    standardInput: null,
                    cancellationToken);

            stopwatch.Stop();

            string output =
                process.CombinedOutput;

            bool solutionExists =
                File.Exists(solutionPath);

            IReadOnlyDictionary<int, double> values =
                NamedSolutionValueParser.ParseFile(
                    solutionPath);

            LinearModelSolveStatus status =
                MapStatus(
                    output,
                    solutionExists,
                    process.ExitCode);

            return SolverExecutionUtilities.BuildSolutionResult(
                model,
                selection,
                options,
                status,
                values,
                solutionExists,
                stopwatch.Elapsed,
                SolverExecutionUtilities.LastMeaningfulLine(output),
                [
                    $"gurobi_cl exit code: {process.ExitCode}.",
                    SolverExecutionUtilities.LastMeaningfulLine(output)
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
                ["Gurobi execution was cancelled by the caller."],
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

    private static LinearModelSolveStatus MapStatus(
        string output,
        bool solutionExists,
        int exitCode)
    {
        if (output.Contains(
                "infeasible or unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.InfeasibleOrUnbounded;
        }

        if (output.Contains(
                "model is infeasible",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Infeasible;
        }

        if (output.Contains(
                "model is unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Unbounded;
        }

        if (output.Contains(
                "optimal solution found",
                StringComparison.OrdinalIgnoreCase) ||
            output.Contains(
                "optimal objective",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Optimal;
        }

        if (solutionExists)
        {
            return LinearModelSolveStatus.Feasible;
        }

        return exitCode == 0
            ? LinearModelSolveStatus.Unknown
            : LinearModelSolveStatus.Failed;
    }

    private static LinearModelSolveResult Failure(
        LinearModel model,
        SolverSelectionResult selection,
        string diagnostic)
    {
        return new LinearModelSolveResult(
            model.Name,
            LinearModelSolveStatus.Failed,
            new SolverExecutionInfo(selection),
            null,
            null,
            TimeSpan.Zero,
            "Executable unavailable",
            [diagnostic]);
    }
}
