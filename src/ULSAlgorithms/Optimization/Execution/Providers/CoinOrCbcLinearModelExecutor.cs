using System.Diagnostics;
using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Optimization.Execution.Providers;

/// <summary>
/// Executes portable LP/MILP models through the stand-alone COIN-OR CBC
/// executable.
/// </summary>
public sealed class CoinOrCbcLinearModelExecutor :
    ILinearModelSolverExecutor
{
    private readonly ExternalSolverProcessRunner _runner =
        new();

    /// <inheritdoc />
    public SolverKind SolverKind =>
        SolverKind.CoinOrCbc;

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

        if (selection.SelectedSolver != SolverKind.CoinOrCbc ||
            selection.Availability is null)
        {
            throw new ArgumentException(
                "The supplied selection does not target COIN-OR CBC.",
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
                "The selected CBC executable is no longer available.");
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
                        modelPath,
                        "-solve",
                        "-solu",
                        solutionPath,
                        "-quit"
                    ],
                    artifacts,
                    standardInput: null,
                    cancellationToken);

            stopwatch.Stop();

            string output =
                process.CombinedOutput;

            string solutionHeader =
                ReadFirstLine(solutionPath);

            bool solutionExists =
                File.Exists(solutionPath) &&
                !ContainsNoSolution(
                    solutionHeader);

            IReadOnlyDictionary<int, double> values =
                NamedSolutionValueParser.ParseFile(
                    solutionPath);

            LinearModelSolveStatus status =
                MapStatus(
                    output,
                    solutionHeader,
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
                !string.IsNullOrWhiteSpace(solutionHeader)
                    ? solutionHeader
                    : SolverExecutionUtilities.LastMeaningfulLine(output),
                [
                    $"cbc exit code: {process.ExitCode}.",
                    !string.IsNullOrWhiteSpace(solutionHeader)
                        ? solutionHeader
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
                ["CBC execution was cancelled by the caller."],
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

    /// <summary>
    /// Normalizes the status reported by CBC.
    /// </summary>
    /// <remarks>
    /// CBC can emit transient simplex/presolve diagnostics containing words
    /// such as "infeasible" before subsequently producing an optimal solution.
    /// The solution-file header is therefore authoritative when it contains a
    /// terminal status. A valid candidate solution also takes precedence over
    /// generic diagnostic text from earlier phases of the solve.
    /// </remarks>
    internal static LinearModelSolveStatus MapStatus(
        string output,
        string solutionHeader,
        bool solutionExists,
        int exitCode)
    {
        if (solutionHeader.StartsWith(
                "Optimal",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Optimal;
        }

        if (solutionHeader.Contains(
                "infeasible or unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.InfeasibleOrUnbounded;
        }

        if (solutionHeader.Contains(
                "infeasible",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Infeasible;
        }

        if (solutionHeader.Contains(
                "unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Unbounded;
        }

        if (solutionExists)
        {
            if (output.Contains(
                    "optimal solution found",
                    StringComparison.OrdinalIgnoreCase))
            {
                return LinearModelSolveStatus.Optimal;
            }

            return LinearModelSolveStatus.Feasible;
        }

        if (output.Contains(
                "infeasible or unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.InfeasibleOrUnbounded;
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

        if (output.Contains(
                "optimal solution found",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Optimal;
        }

        return exitCode == 0
            ? LinearModelSolveStatus.Unknown
            : LinearModelSolveStatus.Failed;
    }

    private static bool ContainsNoSolution(
        string header)
    {
        return header.Contains(
                   "no integer solution",
                   StringComparison.OrdinalIgnoreCase) ||
               header.Contains(
                   "infeasible",
                   StringComparison.OrdinalIgnoreCase) ||
               header.Contains(
                   "unbounded",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadFirstLine(
        string path)
    {
        if (!File.Exists(path))
        {
            return string.Empty;
        }

        using var reader =
            new StreamReader(path);

        return reader.ReadLine() ??
               string.Empty;
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
