\page solver_execution Solver Execution Layer

# Solver Execution Layer

ULSAlgorithms v0.18.0 can execute the portable `LinearModel` introduced in
v0.17.0 with the first usable optimization engine selected by the existing
priority:

1. IBM ILOG CPLEX
2. Gurobi
3. FICO Xpress
4. COIN-OR CBC

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
values are normalized using the same policy as LotSizingDataModel.

Default tolerances are:

```text
zero tolerance             = 1e-8
integrality tolerance      = 1e-7
continuous near-integer    = 1e-8
```

Examples:

```text
-4.999947122996673E-09  -> 0
180.00000000000006      -> 180
0.99999995 (binary)     -> 1
-1E-06 (continuous)     -> preserved
0.75 (binary)           -> rejected
```

The normalized values are then used consistently by both the independent
feasibility checker and objective reconstruction. Material numerical errors are
therefore never hidden by unconditional rounding.

## Independent checker

A returned native solution is never trusted solely because a solver reports
"optimal".

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

## Architecture boundary

v0.18.0 executes generic mathematical models. It intentionally does not yet
turn each ULS formulation into an `IUlsSolver`.

That next layer will:

1. build the selected `UlsFormulation`;
2. call `LinearModelSolver`;
3. reconstruct `UlsSolution` from the formulation variable map;
4. run the existing ULS feasibility/objective checks;
5. expose the formulation itself as a normal exact Strategy implementation.

The `(l,S)` cutting-plane implementation will then reuse this same execution
layer and the cut traceability introduced in v0.15.0.
