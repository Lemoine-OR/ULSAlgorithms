using System.Diagnostics;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.CuttingPlanes.Internal;
using ULSAlgorithms.CuttingPlanes.Separation;
using ULSAlgorithms.Exact.Formulations.Internal;
using ULSAlgorithms.Formulations;
using ULSAlgorithms.Formulations.Aggregate;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization;
using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Optimization.Modeling;
using ULSAlgorithms.Results;
using ULSAlgorithms.Validation;

namespace ULSAlgorithms.Exact.CuttingPlanes;

/// <summary>
/// Base implementation of an exact ULS cut-and-solve algorithm using classical
/// (l,S) inequalities at the root LP relaxation followed by an exact MILP solve.
/// </summary>
public abstract class LsCuttingPlaneSolverBase :
    IUlsSolver,
    IAsyncUlsSolver
{
    private readonly ILsCutSeparator _separator;
    private readonly LinearModelSolver _modelSolver;
    private readonly LinearModelSolveOptions _executionOptions;
    private readonly LsCuttingPlaneOptions _cuttingPlaneOptions;

    /// <summary>Initializes a cutting-plane strategy.</summary>
    protected LsCuttingPlaneSolverBase(
        string name,
        ILsCutSeparator separator,
        LinearModelSolver? modelSolver = null,
        LinearModelSolveOptions? executionOptions = null,
        LsCuttingPlaneOptions? cuttingPlaneOptions = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A solver name is required.",
                nameof(name));
        }

        Name = name.Trim();

        _separator =
            separator ??
            throw new ArgumentNullException(
                nameof(separator));

        _modelSolver =
            modelSolver ??
            new LinearModelSolver();

        _executionOptions =
            CloneExecutionOptions(
                executionOptions ??
                new LinearModelSolveOptions());

        _cuttingPlaneOptions =
            CloneCuttingPlaneOptions(
                cuttingPlaneOptions ??
                new LsCuttingPlaneOptions());
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public UlsSolverKind Kind =>
        UlsSolverKind.Exact;

    /// <summary>Gets the separation method.</summary>
    public CutSeparationMethod SeparationMethod =>
        _separator.Method;

    /// <summary>Tests separator applicability.</summary>
    public bool IsApplicable(
        UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);
        return _separator.IsApplicable(problem);
    }

    /// <inheritdoc />
    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.Run(
                async () =>
                    await SolveAsync(
                        problem,
                        cancellationToken)
                    .ConfigureAwait(false),
                CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    /// <inheritdoc />
    public async ValueTask<UlsSolveResult> SolveAsync(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsApplicable(problem))
        {
            throw new NotSupportedException(
                $"{Name} is not applicable to the supplied ULS cost structure.");
        }

        var formulationBuilder =
            new AggregateInventoryFormulationBuilder();

        UlsFormulation aggregate =
            formulationBuilder.Build(
                problem);

        LinearModel currentLp =
            LsCutModelBuilder.CreateLpRelaxation(
                aggregate.Model);

        var addedCuts =
            new List<(string Name, LsCutDefinition Cut)>();

        var knownCuts =
            new HashSet<string>(
                StringComparer.Ordinal);

        var iterationReports =
            new List<CutIterationReport>();

        int sequence = 0;

        SolverExecutionInfo? selectedSolver =
            null;

        LinearModelSolveResult? lastLpExecution =
            null;

        for (int iteration = 0;
             iteration < _cuttingPlaneOptions.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            LinearModelSolveOptions iterationOptions =
                selectedSolver is null
                    ? CloneExecutionOptions(
                        _executionOptions)
                    : CreatePinnedExecutionOptions(
                        _executionOptions,
                        selectedSolver.SelectedSolver);

            lastLpExecution =
                await _modelSolver.SolveAsync(
                    currentLp,
                    iterationOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            if (lastLpExecution.Solver is not null &&
                selectedSolver is null)
            {
                selectedSolver =
                    lastLpExecution.Solver;
            }

            if (!lastLpExecution.HasFeasibleSolution)
            {
                return BuildEarlyFailure(
                    aggregate,
                    lastLpExecution,
                    selectedSolver,
                    iterationReports,
                    "Root LP relaxation could not be solved to a valid " +
                    "feasible point.");
            }

            var stopwatch =
                Stopwatch.StartNew();

            IReadOnlyList<LsSeparatedCut> candidates =
                _separator.Separate(
                    problem,
                    aggregate,
                    lastLpExecution.VariableValues);

            stopwatch.Stop();

            var records =
                new List<CutRecord>(
                    candidates.Count);

            var newlyAdded =
                new List<(string Name, LsCutDefinition Cut)>();

            foreach (LsSeparatedCut candidate in candidates)
            {
                string key =
                    LsCutKey.Create(
                        candidate.Definition);

                CutDisposition disposition;
                string reason;
                string rowName = string.Empty;

                if (candidate.Violation <=
                    _cuttingPlaneOptions.ViolationTolerance)
                {
                    disposition =
                        CutDisposition.BelowTolerance;

                    reason =
                        $"Violation {candidate.Violation:G17} <= tolerance " +
                        $"{_cuttingPlaneOptions.ViolationTolerance:G17}.";
                }
                else if (!knownCuts.Add(key))
                {
                    disposition =
                        CutDisposition.Duplicate;

                    reason =
                        "An equivalent (l,S) cut was already added.";
                }
                else
                {
                    disposition =
                        CutDisposition.Added;

                    rowName =
                        $"ls_{_separator.Method}_{iteration}_{sequence}";

                    reason =
                        "Violated unique cut added to the portable LP model.";

                    newlyAdded.Add(
                        (rowName, candidate.Definition));

                    addedCuts.Add(
                        (rowName, candidate.Definition));
                }

                records.Add(
                    new CutRecord(
                        sequence++,
                        iteration,
                        _separator.Method,
                        candidate.Definition,
                        candidate.Violation,
                        candidate.Efficacy,
                        disposition,
                        rowName,
                        reason));
            }

            iterationReports.Add(
                new CutIterationReport(
                    iteration,
                    records,
                    stopwatch.Elapsed));

            if (newlyAdded.Count == 0)
            {
                break;
            }

            currentLp =
                LsCutModelBuilder.AddCuts(
                    currentLp,
                    newlyAdded);
        }

        if (selectedSolver is null)
        {
            return new UlsSolveResult(
                Name,
                UlsSolveStatus.NotSolved,
                message:
                    "No optimization solver was selected during root separation.");
        }

        LinearModel strengthenedMip =
            LsCutModelBuilder.AddCuts(
                aggregate.Model,
                addedCuts);

        LinearModelSolveOptions finalOptions =
            CreatePinnedExecutionOptions(
                _executionOptions,
                selectedSolver.SelectedSolver);

        LinearModelSolveResult finalExecution =
            await _modelSolver.SolveAsync(
                strengthenedMip,
                finalOptions,
                cancellationToken)
            .ConfigureAwait(false);

        var cutReport =
            new CutGenerationReport(
                iterationReports);

        var executionReport =
            new CuttingPlaneExecutionReport(
                selectedSolver,
                cutReport);

        if (!finalExecution.HasFeasibleSolution)
        {
            return new CuttingPlaneUlsSolveResult(
                Name,
                MapStatusWithoutSolution(
                    finalExecution.Status),
                _separator.Method,
                executionReport,
                finalExecution,
                solution: null,
                message:
                    BuildMessage(
                        finalExecution,
                        cutReport));
        }

        try
        {
            UlsSolution solution =
                UlsFormulationSolutionMapper.Map(
                    problem,
                    aggregate,
                    finalExecution.VariableValues,
                    finalOptions.ZeroTolerance,
                    finalOptions.FeasibilityTolerance);

            UlsSolutionValidationResult validation =
                UlsSolutionValidator.Validate(
                    problem,
                    solution,
                    finalOptions.FeasibilityTolerance);

            if (!validation.IsFeasible)
            {
                return new CuttingPlaneUlsSolveResult(
                    Name,
                    UlsSolveStatus.Failed,
                    _separator.Method,
                    executionReport,
                    finalExecution,
                    solution: null,
                    message:
                        "ULS-domain validation rejected the reconstructed " +
                        "cutting-plane solution: " +
                        string.Join(
                            " | ",
                            validation.Diagnostics));
            }

            if (finalExecution.ObjectiveValue.HasValue &&
                !ObjectivesAgree(
                    finalExecution.ObjectiveValue.Value,
                    solution.TotalCost,
                    finalOptions.FeasibilityTolerance))
            {
                return new CuttingPlaneUlsSolveResult(
                    Name,
                    UlsSolveStatus.Failed,
                    _separator.Method,
                    executionReport,
                    finalExecution,
                    solution: null,
                    message:
                        $"Portable-model objective " +
                        $"{finalExecution.ObjectiveValue.Value:G17} differs " +
                        $"from reconstructed ULS objective " +
                        $"{solution.TotalCost:G17}.");
            }

            UlsSolveStatus finalStatus =
                finalExecution.Status ==
                LinearModelSolveStatus.Optimal
                    ? UlsSolveStatus.Optimal
                    : UlsSolveStatus.Feasible;

            return new CuttingPlaneUlsSolveResult(
                Name,
                finalStatus,
                _separator.Method,
                executionReport,
                finalExecution,
                solution,
                BuildMessage(
                    finalExecution,
                    cutReport));
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException)
        {
            return new CuttingPlaneUlsSolveResult(
                Name,
                UlsSolveStatus.Failed,
                _separator.Method,
                executionReport,
                finalExecution,
                solution: null,
                message:
                    $"ULS solution reconstruction failed: " +
                    $"{exception.Message}");
        }
    }

    private UlsSolveResult BuildEarlyFailure(
        UlsFormulation aggregate,
        LinearModelSolveResult execution,
        SolverExecutionInfo? selectedSolver,
        IReadOnlyList<CutIterationReport> iterationReports,
        string prefix)
    {
        if (selectedSolver is null)
        {
            return new UlsSolveResult(
                Name,
                MapStatusWithoutSolution(
                    execution.Status),
                message:
                    prefix +
                    " " +
                    string.Join(
                        " | ",
                        execution.Diagnostics));
        }

        var cutReport =
            new CutGenerationReport(
                iterationReports);

        return new CuttingPlaneUlsSolveResult(
            Name,
            MapStatusWithoutSolution(
                execution.Status),
            _separator.Method,
            new CuttingPlaneExecutionReport(
                selectedSolver,
                cutReport),
            execution,
            solution: null,
            message:
                prefix +
                " " +
                BuildMessage(
                    execution,
                    cutReport));
    }

    private static UlsSolveStatus MapStatusWithoutSolution(
        LinearModelSolveStatus status)
    {
        return status switch
        {
            LinearModelSolveStatus.Infeasible =>
                UlsSolveStatus.Infeasible,

            LinearModelSolveStatus.Cancelled or
            LinearModelSolveStatus.SolverUnavailable or
            LinearModelSolveStatus.Unknown =>
                UlsSolveStatus.NotSolved,

            _ =>
                UlsSolveStatus.Failed
        };
    }

    private static bool ObjectivesAgree(
        double modelObjective,
        double ulsObjective,
        double tolerance)
    {
        double scale =
            Math.Max(
                1.0,
                Math.Max(
                    Math.Abs(modelObjective),
                    Math.Abs(ulsObjective)));

        return Math.Abs(
                   modelObjective -
                   ulsObjective) <=
               tolerance * scale;
    }

    private static string BuildMessage(
        LinearModelSolveResult execution,
        CutGenerationReport cuts)
    {
        var parts =
            new List<string>
            {
                $"Cuts generated: {cuts.CutsGenerated}",
                $"cuts added: {cuts.CutsAdded}",
                $"separation iterations: {cuts.IterationCount}"
            };

        if (execution.Solver is not null)
        {
            parts.Add(
                $"Optimization engine: " +
                $"{execution.Solver.SolverName} " +
                $"{execution.Solver.SolverVersion}".Trim());
        }

        if (!string.IsNullOrWhiteSpace(
                execution.NativeStatus))
        {
            parts.Add(
                $"Native status: {execution.NativeStatus}");
        }

        parts.AddRange(
            execution.Diagnostics);

        return string.Join(
            " | ",
            parts.Where(
                static part =>
                    !string.IsNullOrWhiteSpace(part)));
    }

    private static LinearModelSolveOptions CreatePinnedExecutionOptions(
        LinearModelSolveOptions source,
        SolverKind solverKind)
    {
        LinearModelSolveOptions result =
            CloneExecutionOptions(
                source);

        result.Solver =
            solverKind;

        result.AllowFallbackWhenExplicit =
            false;

        return result;
    }

    private static LinearModelSolveOptions CloneExecutionOptions(
        LinearModelSolveOptions source)
    {
        source.EnsureValid();

        return new LinearModelSolveOptions
        {
            Solver = source.Solver,
            AllowFallbackWhenExplicit =
                source.AllowFallbackWhenExplicit,
            FeasibilityTolerance =
                source.FeasibilityTolerance,
            ZeroTolerance =
                source.ZeroTolerance,
            IntegralityTolerance =
                source.IntegralityTolerance,
            NearIntegerTolerance =
                source.NearIntegerTolerance,
            ExportModelPath =
                source.ExportModelPath,
            KeepTemporaryFiles =
                source.KeepTemporaryFiles,
            TemporaryRootPath =
                source.TemporaryRootPath
        };
    }

    private static LsCuttingPlaneOptions CloneCuttingPlaneOptions(
        LsCuttingPlaneOptions source)
    {
        source.EnsureValid();

        return new LsCuttingPlaneOptions
        {
            MaximumIterations =
                source.MaximumIterations,
            ViolationTolerance =
                source.ViolationTolerance
        };
    }
}
