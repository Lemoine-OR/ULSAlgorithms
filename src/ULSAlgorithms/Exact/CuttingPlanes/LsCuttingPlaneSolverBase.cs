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
            throw new ArgumentNullException(nameof(separator));
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

    public string Name { get; }

    public UlsSolverKind Kind =>
        UlsSolverKind.Exact;

    public CutSeparationMethod SeparationMethod =>
        _separator.Method;

    public bool IsApplicable(
        UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);
        return _separator.IsApplicable(problem);
    }

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
            formulationBuilder.Build(problem);

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

        var convergenceIterations =
            new List<CuttingPlaneIterationStatistics>();

        int sequence = 0;

        SolverExecutionInfo? selectedSolver =
            null;

        for (int iteration = 0;
             iteration < _cuttingPlaneOptions.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            LinearModelSolveOptions iterationOptions =
                selectedSolver is null
                    ? CloneExecutionOptions(_executionOptions)
                    : CreatePinnedExecutionOptions(
                        _executionOptions,
                        selectedSolver.SelectedSolver);

            LinearModelSolveResult lpExecution =
                await _modelSolver.SolveAsync(
                    currentLp,
                    iterationOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            if (lpExecution.Solver is not null &&
                selectedSolver is null)
            {
                selectedSolver =
                    lpExecution.Solver;
            }

            if (!lpExecution.HasFeasibleSolution ||
                !lpExecution.ObjectiveValue.HasValue)
            {
                return BuildEarlyFailure(
                    lpExecution,
                    selectedSolver,
                    iterationReports,
                    convergenceIterations,
                    "Root LP relaxation could not be solved to a valid " +
                    "feasible point.");
            }

            var stopwatch =
                Stopwatch.StartNew();

            IReadOnlyList<LsSeparatedCut> candidates =
                _separator.Separate(
                    problem,
                    aggregate,
                    lpExecution.VariableValues);

            stopwatch.Stop();

            var records =
                new List<CutRecord>(
                    candidates.Count);

            var eligibleIndices =
                new List<int>();

            var preDisposition =
                new Dictionary<int, (CutDisposition Disposition, string Reason)>();

            var keys =
                new string[candidates.Count];

            var seenThisIteration =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (int index = 0;
                 index < candidates.Count;
                 index++)
            {
                LsSeparatedCut candidate =
                    candidates[index];

                string key =
                    LsCutKey.Create(
                        candidate.Definition);

                keys[index] = key;

                if (candidate.Violation <=
                    _cuttingPlaneOptions.ViolationTolerance)
                {
                    preDisposition[index] =
                        (
                            CutDisposition.BelowTolerance,
                            $"Violation {candidate.Violation:G17} <= " +
                            $"tolerance " +
                            $"{_cuttingPlaneOptions.ViolationTolerance:G17}."
                        );

                    continue;
                }

                if (candidate.Efficacy <
                    _cuttingPlaneOptions.MinimumEfficacy)
                {
                    preDisposition[index] =
                        (
                            CutDisposition.NotSelected,
                            $"Efficacy {candidate.Efficacy:G17} < minimum " +
                            $"{_cuttingPlaneOptions.MinimumEfficacy:G17}."
                        );

                    continue;
                }

                if (knownCuts.Contains(key) ||
                    !seenThisIteration.Add(key))
                {
                    preDisposition[index] =
                        (
                            CutDisposition.Duplicate,
                            "An equivalent (l,S) cut is already present."
                        );

                    continue;
                }

                eligibleIndices.Add(index);
            }

            HashSet<int> selectedIndices =
                LsCutSelector.Select(
                    candidates,
                    eligibleIndices,
                    _cuttingPlaneOptions);

            var newlyAdded =
                new List<(string Name, LsCutDefinition Cut)>();

            int selectedCount = 0;

            for (int index = 0;
                 index < candidates.Count;
                 index++)
            {
                LsSeparatedCut candidate =
                    candidates[index];

                CutDisposition disposition;
                string reason;
                string rowName =
                    string.Empty;

                if (preDisposition.TryGetValue(
                        index,
                        out var prior))
                {
                    disposition =
                        prior.Disposition;

                    reason =
                        prior.Reason;
                }
                else if (!selectedIndices.Contains(index))
                {
                    disposition =
                        CutDisposition.NotSelected;

                    reason =
                        $"Eligible violated cut not selected by policy " +
                        $"'{_cuttingPlaneOptions.SelectionPolicy}'.";
                }
                else
                {
                    disposition =
                        CutDisposition.Added;

                    rowName =
                        $"ls_{_separator.Method}_{iteration}_{sequence}";

                    reason =
                        $"Selected by '{_cuttingPlaneOptions.SelectionPolicy}' " +
                        "and added to the portable LP model.";

                    selectedCount++;

                    knownCuts.Add(
                        keys[index]);

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

            var iterationReport =
                new CutIterationReport(
                    iteration,
                    records,
                    stopwatch.Elapsed);

            iterationReports.Add(
                iterationReport);

            double[] positiveViolations =
                candidates
                    .Where(
                        candidate =>
                            candidate.Violation > 0.0)
                    .Select(
                        candidate =>
                            candidate.Violation)
                    .ToArray();

            convergenceIterations.Add(
                new CuttingPlaneIterationStatistics(
                    iteration,
                    lpExecution.ObjectiveValue.Value,
                    lpExecution.SolveDuration,
                    stopwatch.Elapsed,
                    generatedCandidates:
                        candidates.Count,
                    eligibleCandidates:
                        eligibleIndices.Count,
                    selectedCuts:
                        selectedCount,
                    cutsAdded:
                        newlyAdded.Count,
                    cumulativeCutsAdded:
                        addedCuts.Count,
                    maximumViolation:
                        candidates.Count == 0
                            ? 0.0
                            : candidates.Max(
                                candidate =>
                                    candidate.Violation),
                    meanPositiveViolation:
                        positiveViolations.Length == 0
                            ? 0.0
                            : positiveViolations.Average(),
                    maximumEfficacy:
                        candidates.Count == 0
                            ? 0.0
                            : candidates.Max(
                                candidate =>
                                    candidate.Efficacy)));

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

        var convergence =
            new CuttingPlaneConvergenceReport(
                convergenceIterations,
                finalExecution.ObjectiveValue);

        var executionReport =
            new CuttingPlaneExecutionReport(
                selectedSolver,
                cutReport,
                convergence);

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
                        cutReport,
                        convergence));
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
                    cutReport,
                    convergence));
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
        LinearModelSolveResult execution,
        SolverExecutionInfo? selectedSolver,
        IReadOnlyList<CutIterationReport> iterationReports,
        IReadOnlyList<CuttingPlaneIterationStatistics> convergenceIterations,
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

        var convergence =
            new CuttingPlaneConvergenceReport(
                convergenceIterations,
                finalMipObjective: null);

        return new CuttingPlaneUlsSolveResult(
            Name,
            MapStatusWithoutSolution(
                execution.Status),
            _separator.Method,
            new CuttingPlaneExecutionReport(
                selectedSolver,
                cutReport,
                convergence),
            execution,
            solution: null,
            message:
                prefix +
                " " +
                BuildMessage(
                    execution,
                    cutReport,
                    convergence));
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
        CutGenerationReport cuts,
        CuttingPlaneConvergenceReport convergence)
    {
        var parts =
            new List<string>
            {
                $"Cuts generated: {cuts.CutsGenerated}",
                $"cuts added: {cuts.CutsAdded}",
                $"not selected: {cuts.NotSelected}",
                $"separation iterations: {cuts.IterationCount}"
            };

        if (convergence.RootBoundImprovement.HasValue)
        {
            parts.Add(
                $"root bound improvement: " +
                $"{convergence.RootBoundImprovement.Value:G17}");
        }

        if (convergence.RootGapClosedFraction.HasValue)
        {
            parts.Add(
                $"root gap closed: " +
                $"{convergence.RootGapClosedFraction.Value:P2}");
        }

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
            CloneExecutionOptions(source);

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
                source.ViolationTolerance,
            MinimumEfficacy =
                source.MinimumEfficacy,
            SelectionPolicy =
                source.SelectionPolicy,
            MaximumCutsPerIteration =
                source.MaximumCutsPerIteration
        };
    }
}
