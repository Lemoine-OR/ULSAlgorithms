using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.Formulations.Internal;
using ULSAlgorithms.Formulations;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Results;
using ULSAlgorithms.Validation;

namespace ULSAlgorithms.Exact.Formulations;

/// <summary>
/// Base class for exact ULS strategies that build a mathematical formulation
/// and solve it through the portable optimization execution layer.
/// </summary>
public abstract class SolverBackedUlsFormulationSolverBase :
    IUlsSolver,
    IAsyncUlsSolver
{
    private readonly IUlsFormulationBuilder _formulationBuilder;
    private readonly LinearModelSolver _modelSolver;
    private readonly LinearModelSolveOptions _executionOptions;

    /// <summary>Initializes one solver-backed formulation strategy.</summary>
    protected SolverBackedUlsFormulationSolverBase(
        string name,
        IUlsFormulationBuilder formulationBuilder,
        LinearModelSolver? modelSolver = null,
        LinearModelSolveOptions? executionOptions = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A solver name is required.",
                nameof(name));
        }

        Name = name.Trim();

        _formulationBuilder =
            formulationBuilder ??
            throw new ArgumentNullException(
                nameof(formulationBuilder));

        _modelSolver =
            modelSolver ??
            new LinearModelSolver();

        _executionOptions =
            CloneOptions(
                executionOptions ??
                new LinearModelSolveOptions());
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public UlsSolverKind Kind =>
        UlsSolverKind.Exact;

    /// <summary>Gets the formulation implemented by this strategy.</summary>
    public UlsFormulationKind FormulationKind =>
        _formulationBuilder.Kind;

    /// <summary>Tests formulation applicability to one ULS problem.</summary>
    public bool IsApplicable(
        UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        return _formulationBuilder.IsApplicable(
            problem);
    }

    /// <inheritdoc />
    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        // The common historical IUlsSolver API is synchronous. Run the async
        // solver-backed path on the thread pool so a UI SynchronizationContext
        // cannot deadlock the provider's asynchronous continuations.
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

        UlsFormulation formulation =
            _formulationBuilder.Build(
                problem);

        LinearModelSolveOptions options =
            CloneOptions(
                _executionOptions);

        LinearModelSolveResult execution =
            await _modelSolver.SolveAsync(
                formulation.Model,
                options,
                cancellationToken)
            .ConfigureAwait(false);

        if (!execution.HasFeasibleSolution)
        {
            return new SolverBackedUlsSolveResult(
                Name,
                MapStatusWithoutSolution(
                    execution.Status),
                formulation.Kind,
                execution,
                solution: null,
                message:
                    BuildMessage(
                        execution));
        }

        try
        {
            UlsSolution solution =
                UlsFormulationSolutionMapper.Map(
                    problem,
                    formulation,
                    execution.VariableValues,
                    options.ZeroTolerance,
                    options.FeasibilityTolerance);

            UlsSolutionValidationResult validation =
                UlsSolutionValidator.Validate(
                    problem,
                    solution,
                    options.FeasibilityTolerance);

            if (!validation.IsFeasible)
            {
                return new SolverBackedUlsSolveResult(
                    Name,
                    UlsSolveStatus.Failed,
                    formulation.Kind,
                    execution,
                    solution: null,
                    message:
                        "The mathematical model returned a valid native " +
                        "solution, but ULS-domain reconstruction failed: " +
                        string.Join(
                            " | ",
                            validation.Diagnostics));
            }

            if (execution.ObjectiveValue.HasValue &&
                !ObjectivesAgree(
                    execution.ObjectiveValue.Value,
                    solution.TotalCost,
                    options.FeasibilityTolerance))
            {
                return new SolverBackedUlsSolveResult(
                    Name,
                    UlsSolveStatus.Failed,
                    formulation.Kind,
                    execution,
                    solution: null,
                    message:
                        $"Portable-model objective " +
                        $"{execution.ObjectiveValue.Value:G17} differs from " +
                        $"reconstructed ULS objective " +
                        $"{solution.TotalCost:G17}.");
            }

            UlsSolveStatus status =
                execution.Status ==
                LinearModelSolveStatus.Optimal
                    ? UlsSolveStatus.Optimal
                    : UlsSolveStatus.Feasible;

            return new SolverBackedUlsSolveResult(
                Name,
                status,
                formulation.Kind,
                execution,
                solution,
                BuildMessage(
                    execution));
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException)
        {
            return new SolverBackedUlsSolveResult(
                Name,
                UlsSolveStatus.Failed,
                formulation.Kind,
                execution,
                solution: null,
                message:
                    $"ULS solution reconstruction failed: {exception.Message}");
        }
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

            LinearModelSolveStatus.Unbounded or
            LinearModelSolveStatus.InfeasibleOrUnbounded or
            LinearModelSolveStatus.Failed =>
                UlsSolveStatus.Failed,

            LinearModelSolveStatus.Optimal or
            LinearModelSolveStatus.Feasible =>
                UlsSolveStatus.Failed,

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
        LinearModelSolveResult execution)
    {
        var parts =
            new List<string>();

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

        if (execution.Diagnostics.Count > 0)
        {
            parts.AddRange(
                execution.Diagnostics);
        }

        return string.Join(
            " | ",
            parts.Where(
                static part =>
                    !string.IsNullOrWhiteSpace(part)));
    }

    private static LinearModelSolveOptions CloneOptions(
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
}
