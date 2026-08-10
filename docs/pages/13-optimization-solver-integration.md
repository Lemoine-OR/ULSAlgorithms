\page optimization_solver_integration Optimization Solver Integration

# Optimization Solver Integration

Solver-backed ULS methods must not hard-code one mathematical optimizer.

## Automatic selection order

`SolverKind.Automatic` uses the same default priority as LotSizingDataModel:

1. **IBM ILOG CPLEX**
2. **Gurobi**
3. **FICO Xpress**
4. **COIN-OR CBC**

Starting with v0.16.0, the four concrete discovery adapters are included in the
main package and can be used without manually constructing a registry:

```csharp
SolverSelectionResult selection =
    await OptimizationSolverDiscovery.SelectAsync(
        SolverKind.Automatic,
        options,
        cancellationToken);
```

The first adapter that both:

- supports all capabilities required by the algorithm; and
- reports a usable installation on the current computer

is selected.

A solver that is installed but cannot load its libraries, lacks a usable
license, or does not expose a required capability is skipped with a diagnostic.

## Complete discovery

To inspect the complete machine state:

```csharp
SolverDiscoveryReport report =
    await OptimizationSolverDiscovery.DiscoverAllAsync(
        cancellationToken);
```

The report always follows the standard CPLEX → Gurobi → Xpress → CBC order and
contains both usable and unavailable solver diagnostics.

## Explicit solver selection

A caller may request a concrete solver. With
`SolverSelectionOptions.RequireExactSolverKind = true`, failure of that solver
does not trigger fallback. With the default `false`, the requested solver is
tried first, followed by the standard priority.

## Reproducibility

A solver-backed algorithm should attach `SolverExecutionInfo` to its detailed
execution result. The snapshot records:

- requested solver;
- selected solver;
- adapter id/name/version;
- detected solver name/version;
- availability status;
- installation path;
- license information;
- selection diagnostics.

This allows benchmark and computational-study results to identify the engine
that actually executed the mathematical model.
