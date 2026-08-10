\page formulation_solvers Solver-Backed ULS Formulation Strategies

# Solver-Backed ULS Formulation Strategies

ULSAlgorithms v0.19.0 promoted the four mathematical formulations introduced
in v0.17.0 to normal exact ULS Strategy implementations.

The public classes are:

```text
AggregateInventoryFormulationSolver
FacilityLocationFormulationSolver
ShortestPathFormulationSolver
InventoryEliminatedFormulationSolver
```

Each class implements:

- `IUlsSolver`;
- `IAsyncUlsSolver`;
- `UlsSolverKind.Exact`.

Therefore existing synchronous Strategy-pattern code keeps the same contract:

```csharp
IUlsSolver solver =
    new AggregateInventoryFormulationSolver();

UlsSolveResult result =
    solver.Solve(problem);
```

Solver-backed callers can avoid blocking with:

```csharp
IAsyncUlsSolver solver =
    new AggregateInventoryFormulationSolver();

UlsSolveResult result =
    await solver.SolveAsync(
        problem,
        cancellationToken);
```

## Automatic optimization-engine selection

Unless an explicit solver is requested, each strategy delegates to
`LinearModelSolver`, which uses:

```text
CPLEX -> Gurobi -> Xpress -> COIN-OR CBC
```

The strategy does not contain provider-specific model code.

## Reconstruction by formulation

### Aggregate inventory

`x[t]`, `I[t]`, and `y[t]` map directly to:

- production quantity;
- ending inventory;
- setup decision.

### Facility location

Production is reconstructed by:

\f[
x_t = \sum_{k=t}^{T} q_{tk}.
\f]

Inventory is then reconstructed from material balance.

### Shortest path

The mapper extracts one positive-flow source-to-sink path from the optimal
network support. This remains robust if an LP provider returns a fractional
convex combination of alternative optimal paths.

Each replenishment arc `(t,j+1)` becomes one production lot in period `t`
covering demand `t..j`. Zero-demand skip arcs do not create setups.

### Inventory eliminated

Production and setup decisions map directly. End-of-period inventory is
reconstructed from cumulative material balance because the mathematical model
contains no explicit inventory variables.

## Two independent validation levels

A solver-backed strategy is accepted only after two checks.

First, the generic optimization layer validates the returned optimization
candidate against `LinearModel`:

- bounds;
- integrality;
- every mathematical constraint;
- portable-model objective.

Second, the formulation strategy validates the reconstructed `UlsSolution`
against the original `UlsProblem`:

- inventory balance;
- final zero inventory;
- production/setup consistency;
- setup cost;
- production cost;
- holding cost;
- total objective.

The independently reconstructed ULS objective must also agree with the
portable-model objective.

A mismatch returns `UlsSolveStatus.Failed`, never `Optimal`.

## Numerical normalization and row feasibility

The execution-layer normalization policy remains in force before
reconstruction:

```text
zero tolerance          1e-8
feasibility tolerance   1e-7
integrality tolerance   1e-7
near-integer tolerance  1e-8
```

Bounds and integrality are checked with their absolute tolerances. Constraint
feasibility uses the execution layer's mixed absolute/relative row scale:

\f[
\max\left(1,\lvert b\rvert,\sum_i\lvert a_i x_i\rvert\right).
\f]

This keeps small rows protected by the absolute tolerance while preventing
harmless solver-output residuals on larger rows from being mistaken for
material infeasibility.

## Provenance

Solver-backed strategies return a `SolverBackedUlsSolveResult`, which remains
assignable to `UlsSolveResult` but additionally exposes:

```text
FormulationKind
ModelExecution
OptimizationSolver
```

`OptimizationSolver` identifies the concrete engine, version, adapter and
selection diagnostics.

## Scientific identity

The formulation strategies do not replace the native dynamic-programming or
geometric implementations. All remain separate public strategies so users can
benchmark, cite and compare mathematical formulations against the direct
algorithmic methods.
