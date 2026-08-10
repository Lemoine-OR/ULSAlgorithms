\page solver_execution Solver Execution Layer

# Solver Execution Layer

ULSAlgorithms executes its portable `LinearModel` with the first usable engine
selected from the repository-wide priority:

1. IBM ILOG CPLEX
2. Gurobi
3. FICO Xpress
4. COIN-OR CBC

This execution layer is used directly by the four solver-backed formulation
strategies and by both `(l,S)` cutting-plane strategies.

## High-level API

```csharp
var solver = new LinearModelSolver();

LinearModelSolveResult result =
    await solver.SolveAsync(
        formulation.Model,
        new LinearModelSolveOptions
        {
            Solver = SolverKind.Automatic
        },
        cancellationToken);
```

For a mixed-integer model the orchestration layer automatically requires
`MixedIntegerLinearProgramming`; for a continuous model it requires
`LinearProgramming`.

An explicitly requested solver is strict by default. Set
`AllowFallbackWhenExplicit = true` only when fallback is desired.

## Provider execution backends

### CPLEX

The discovery layer first validates the CPLEX runtime. Execution then invokes
the stand-alone `cplex` program from the selected CPLEX runtime directory,
submits the portable LP file and parses the XML `.sol` file.

### Gurobi

The backend invokes `gurobi_cl` with a `ResultFile` solution target and parses
portable `v_<id>` variable names.

### FICO Xpress

The backend reuses the optional `Optimizer.dll` runtime and invokes
`XPRSprob.ReadProb`, `Optimize` and `GetSolution` through reflection. Variable
values are mapped by portable column name where the loaded Xpress API exposes
`GetIndex`, with a column-order fallback only when vector length is exact.

### COIN-OR CBC

The backend invokes the stand-alone `cbc` executable with `-solve` and `-solu`
and parses the generated text solution.

The v0.29.0 qualification pipeline exercises CBC end to end against all six
public solver-backed strategies with fallback disabled.

## Stable portable names

The LP writer uses:

```text
v_0
v_1
v_2
...
```

instead of semantic names such as `x[0]` or `q[2,7]`. This makes solution
parsing independent of punctuation and solver-specific name normalization.

The original semantic mapping remains available in `UlsFormulation.Variables`.

## Numerical normalization

Before the independent checker sees a solver solution, raw floating-point
values are normalized.

Repository defaults are:

```text
zero tolerance                 = 1e-8
feasibility tolerance          = 1e-7
integrality tolerance          = 1e-7
continuous near-integer        = 1e-8
```

Examples:

```text
-4.999947122996673E-09  -> 0
180.00000000000006      -> 180
0.99999995 (binary)     -> 1
-1E-06 (continuous)     -> preserved
0.75 (binary)           -> rejected
```

Normalization and feasibility checking deliberately solve different numerical
problems:

- zero/integer cleanup uses the absolute normalization tolerances above;
- variable bounds use the absolute feasibility tolerance;
- integrality uses the absolute integrality tolerance;
- linear constraints use a mixed absolute/relative row-feasibility test.

For a constraint with right-hand side `b`, coefficients `a_i` and returned
values `x_i`, the checker uses the row scale

\f[
s = \max\left(1,\lvert b\rvert,\sum_i \lvert a_i x_i\rvert\right).
\f]

A constraint is accepted when

\f[
\frac{\text{absolute violation}}{s}
\le \text{FeasibilityTolerance}.
\f]

The scale floor of one preserves the original absolute protection for small
rows, while larger rows are not falsely rejected because of harmless
floating-point/text-solution residuals.

`MaximumConstraintViolation` remains the raw absolute diagnostic value; the
scaled value is used internally for the feasibility decision.

## Independent checker

A returned native solution is never trusted solely because an optimization
engine reports "optimal".

`LinearModelSolutionValidator` independently checks:

- every variable value is finite;
- lower and upper bounds;
- binary/integer integrality;
- every linear constraint;
- objective reconstruction, including the objective constant omitted from the
  portable LP file.

If a native solver reports an optimal or feasible solution that fails this
independent check, the normalized result is `Failed`, not `Optimal`.

The validation report exposes:

```text
IsFeasible
ObjectiveValue
MaximumBoundViolation
MaximumIntegralityViolation
MaximumConstraintViolation
Diagnostics
```

## Reproducibility

`LinearModelSolveResult` records:

- normalized status;
- complete variable vector;
- independently recomputed objective;
- selected solver and version through `SolverExecutionInfo`;
- native status summary;
- elapsed solve time;
- diagnostics.

Set `KeepTemporaryFiles = true` to retain the exact LP, solution and provider
artifacts. `ExportModelPath` can also save the exact submitted LP model without
retaining the complete temporary directory.

## Current ULS integration

The generic execution layer is fully connected to the public ULS Strategy
surface.

For the four formulation strategies, the path is:

```text
UlsProblem
  -> UlsFormulation
  -> LinearModelSolver
  -> independent portable-model validation
  -> formulation-specific reconstruction
  -> independent ULS validation
  -> SolverBackedUlsSolveResult
```

The current formulation strategies are:

```text
AggregateInventoryFormulationSolver
FacilityLocationFormulationSolver
ShortestPathFormulationSolver
InventoryEliminatedFormulationSolver
```

Each implements both `IUlsSolver` and `IAsyncUlsSolver`.

The two `(l,S)` cutting-plane strategies reuse the same execution layer for
root LP solves and the final strengthened MILP. Their final result retains
solver provenance together with cutting-plane trace/convergence information.

Therefore solver discovery, model execution, numerical validation,
formulation reconstruction and cutting-plane execution now form one completed
end-to-end architecture rather than separate future layers.
