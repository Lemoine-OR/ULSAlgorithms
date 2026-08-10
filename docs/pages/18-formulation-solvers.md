\page formulation_solvers Solver-Backed ULS Formulation Strategies

# Solver-Backed ULS Formulation Strategies

ULSAlgorithms v0.19.0 promotes the four mathematical formulations introduced
in v0.17.0 to normal exact ULS Strategy implementations.

The new public classes are:

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

First, v0.18.0 validates the raw optimization result against `LinearModel`:

- bounds;
- integrality;
- every mathematical constraint;
- portable-model objective.

Second, v0.19.0 validates the reconstructed `UlsSolution` against the original
`UlsProblem`:

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

## Numerical normalization

The v0.18.0 normalization policy remains in force before reconstruction:

```text
zero tolerance          1e-8
integrality tolerance   1e-7
near-integer tolerance  1e-8
```

Small floating-point residues are cleaned; material errors are not hidden.

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

The formulation strategy does not replace the native dynamic-programming or
geometric implementations. All remain separate public strategies so users can
benchmark, cite and compare the mathematical formulation against the direct
algorithmic methods.
