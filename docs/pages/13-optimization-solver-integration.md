\page optimization_solver_integration Optimization Solver Integration

# Optimization Solver Integration

Solver-backed ULS methods must not hard-code one mathematical optimizer.

## Automatic selection order

`SolverKind.Automatic` uses the same default priority as LotSizingDataModel:

1. **IBM ILOG CPLEX**
2. **Gurobi**
3. **FICO Xpress**
4. **COIN-OR CBC**

The first adapter that both:

- supports all capabilities required by the algorithm; and
- reports a usable installation on the current computer

is selected.

A solver that is installed but cannot load its libraries, lacks a usable
license, or does not expose a required capability is skipped with a diagnostic.

## Explicit solver selection

A caller may request a concrete solver. With
`SolverSelectionOptions.RequireExactSolverKind = true`, failure of that solver
does not trigger fallback. With the default `false`, the requested solver is
tried first, followed by the standard priority.

## Adapter responsibility

Concrete adapters perform the real machine check:

- installation discovery;
- managed/native library validation;
- load smoke test;
- license validation where relevant;
- solver-version reporting;
- capability reporting.

The generic selection layer does not infer usability from an installation
folder alone.

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
