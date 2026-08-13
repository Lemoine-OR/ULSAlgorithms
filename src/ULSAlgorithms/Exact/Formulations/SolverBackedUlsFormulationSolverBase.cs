using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.Formulations.Internal;
using ULSAlgorithms.Formulations;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Optimization.Modeling;
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

        execution =
            await TryRecoverRejectedCandidateAsync(
                formulation.Model,
                execution,
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

    private async ValueTask<LinearModelSolveResult>
        TryRecoverRejectedCandidateAsync(
            LinearModel originalModel,
            LinearModelSolveResult execution,
            LinearModelSolveOptions options,
            CancellationToken cancellationToken)
    {
        if (!options.EnableFixedIntegerPolishing ||
            !ShouldAttemptFixedIntegerPolishing(
                originalModel,
                execution,
                options))
        {
            return execution;
        }

        LinearModel fixedIntegerModel;

        try
        {
            fixedIntegerModel =
                BuildFixedIntegerPolishingModel(
                    originalModel,
                    execution.VariableValues,
                    options.IntegralityTolerance);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException)
        {
            return AppendDiagnostic(
                execution,
                "Fixed-integer polishing could not build the recovery model: " +
                exception.Message);
        }

        LinearModelSolveOptions polishOptions =
            CloneOptions(options);

        // Never overwrite an explicitly requested export of the original MIP.
        polishOptions.ExportModelPath =
            string.Empty;

        LinearModelSolveResult polished;

        try
        {
            polished =
                await _modelSolver.SolveAsync(
                    fixedIntegerModel,
                    polishOptions,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return AppendDiagnostic(
                execution,
                "Fixed-integer polishing failed during continuous re-optimization: " +
                exception.Message);
        }

        if (!polished.HasFeasibleSolution)
        {
            return AppendDiagnostic(
                execution,
                "Fixed-integer polishing did not recover an independently " +
                $"feasible solution (status {polished.Status}; native " +
                $"'{polished.NativeStatus}').");
        }

        LinearModelSolveStatus recoveredStatus =
            execution.SolverReportedStatus ==
            LinearModelSolveStatus.Optimal
                ? LinearModelSolveStatus.Optimal
                : LinearModelSolveStatus.Feasible;

        var diagnostics =
            execution.Diagnostics
                .Concat(
                    [
                        "Fixed-integer polishing recovered the candidate by " +
                        "fixing normalized integer decisions and re-optimizing " +
                        "the remaining continuous model."
                    ])
                .Concat(polished.Diagnostics)
                .ToArray();

        string nativeStatus =
            string.IsNullOrWhiteSpace(execution.NativeStatus)
                ? "fixed-integer polish: " + polished.NativeStatus
                : execution.NativeStatus +
                  " | fixed-integer polish: " +
                  polished.NativeStatus;

        return new LinearModelSolveResult(
            originalModel.Name,
            recoveredStatus,
            polished.Solver ??
            execution.Solver,
            polished.VariableValues,
            polished.Validation,
            execution.SolveDuration +
            polished.SolveDuration,
            nativeStatus,
            diagnostics,
            polished.ArtifactDirectory)
        {
            // Critical scientific rule: a CPXMIP_OPTIMAL_TOL incumbent stays
            // non-proven even when its fixed-integer LP is polished optimally.
            SolverReportedStatus =
                execution.SolverReportedStatus
        };
    }

    private static bool ShouldAttemptFixedIntegerPolishing(
        LinearModel model,
        LinearModelSolveResult execution,
        LinearModelSolveOptions options)
    {
        if (!model.IsMixedInteger ||
            execution.VariableValues.Count == 0 ||
            execution.Validation is null ||
            execution.Validation.IsFeasible ||
            execution.SolverReportedStatus is not (
                LinearModelSolveStatus.Optimal or
                LinearModelSolveStatus.Feasible))
        {
            return false;
        }

        foreach (LinearVariable variable in model.Variables)
        {
            if (variable.Type == LinearVariableType.Continuous)
            {
                continue;
            }

            if (!execution.VariableValues.TryGetValue(
                    variable.Id,
                    out double value) ||
                !double.IsFinite(value))
            {
                return false;
            }

            double rounded =
                Math.Round(
                    value,
                    MidpointRounding.AwayFromZero);

            if (Math.Abs(value - rounded) >
                options.IntegralityTolerance)
            {
                return false;
            }
        }

        return true;
    }

    private static LinearModel BuildFixedIntegerPolishingModel(
        LinearModel original,
        IReadOnlyDictionary<int, double> candidateValues,
        double integralityTolerance)
    {
        var variables =
            new LinearVariable[original.VariableCount];

        for (int index = 0;
             index < original.VariableCount;
             index++)
        {
            LinearVariable variable =
                original.Variables[index];

            if (variable.Type ==
                LinearVariableType.Continuous)
            {
                variables[index] =
                    variable;

                continue;
            }

            if (!candidateValues.TryGetValue(
                    variable.Id,
                    out double candidate) ||
                !double.IsFinite(candidate))
            {
                throw new InvalidOperationException(
                    $"No finite candidate value exists for integer variable " +
                    $"'{variable.Name}'.");
            }

            double fixedValue =
                Math.Round(
                    candidate,
                    MidpointRounding.AwayFromZero);

            if (Math.Abs(candidate - fixedValue) >
                integralityTolerance)
            {
                throw new InvalidOperationException(
                    $"Integer variable '{variable.Name}' is too fractional " +
                    "for fixed-integer polishing.");
            }

            if (fixedValue <
                    variable.LowerBound -
                    integralityTolerance ||
                fixedValue >
                    variable.UpperBound +
                    integralityTolerance)
            {
                throw new InvalidOperationException(
                    $"Rounded integer value {fixedValue:G17} for " +
                    $"'{variable.Name}' lies outside its original bounds.");
            }

            // Convert the fixed integer decision to a continuous fixed
            // variable. The recovery solve is therefore an LP, not another MIP.
            variables[index] =
                new LinearVariable(
                    variable.Id,
                    variable.Name,
                    LinearVariableType.Continuous,
                    fixedValue,
                    fixedValue);
        }

        return new LinearModel(
            original.Name + "-FixedIntegerPolish",
            variables,
            original.Constraints,
            original.Objective);
    }

    private static LinearModelSolveResult AppendDiagnostic(
        LinearModelSolveResult execution,
        string message)
    {
        return new LinearModelSolveResult(
            execution.ModelName,
            execution.Status,
            execution.Solver,
            execution.VariableValues,
            execution.Validation,
            execution.SolveDuration,
            execution.NativeStatus,
            execution.Diagnostics.Concat([message]),
            execution.ArtifactDirectory)
        {
            SolverReportedStatus =
                execution.SolverReportedStatus
        };
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
            EnableFixedIntegerPolishing =
                source.EnableFixedIntegerPolishing,
            ExportModelPath =
                source.ExportModelPath,
            KeepTemporaryFiles =
                source.KeepTemporaryFiles,
            TemporaryRootPath =
                source.TemporaryRootPath
        };
    }
}

