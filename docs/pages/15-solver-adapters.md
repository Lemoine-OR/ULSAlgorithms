\page solver_adapters Concrete Solver Adapters

# Concrete Solver Adapters

ULSAlgorithms ships four optional machine-discovery adapters used by the
current portable optimization-execution layer and by all six public
solver-backed ULS strategies:

- four mathematical-formulation strategies;
- two `(l,S)` cutting-plane strategies.

The adapters are responsible for locating a usable optimization engine,
checking the required runtime/capability path and returning reproducible
selection diagnostics. Provider-specific model execution is then delegated to
the corresponding `ILinearModelSolverExecutor`.

No external optimizer is required by direct exact algorithms or heuristics.

## Automatic priority

Automatic selection uses the repository-wide order:

1. **IBM ILOG CPLEX**
2. **Gurobi Optimizer**
3. **FICO Xpress MP**
4. **COIN-OR CBC**

At the generic execution level, callers normally use `LinearModelSolver`,
which performs capability-aware selection automatically.

The lower-level discovery API remains available when only engine discovery or
diagnostics are required:

```csharp
SolverSelectionResult selection =
    await OptimizationSolverDiscovery.SelectAsync(
        SolverKind.Automatic,
        options,
        cancellationToken);
```

No manual registry construction is required for the built-in engines.

## CPLEX

The CPLEX adapter has no compile-time dependency on IBM assemblies.

Discovery checks, in order:

- `ULSALGORITHMS_CPLEX_HOME`;
- standard `CPLEX_STUDIO_DIR*` variables;
- `Program Files\IBM\ILOG\CPLEX_Studio*` on Windows.

A candidate must contain:

```text
cplex/bin/x64_win64/ILOG.Concert.dll
cplex/bin/x64_win64/ILOG.CPLEX.dll
```

The adapter dynamically loads the assemblies, instantiates
`ILOG.CPLEX.Cplex`, reads its `Version`, then calls `End`.
Failure to create the CPLEX environment is reported as a load or licensing
failure rather than silently accepting the installation directory.

The execution backend submits the portable LP model through the selected CPLEX
installation and parses the generated solution artifact back into portable
variable IDs.

## Gurobi

Gurobi is probed through the official `gurobi_cl` executable.

Discovery checks:

- `ULSALGORITHMS_GUROBI_EXECUTABLE`;
- compatibility variable `LOTSIZING_GUROBI_EXECUTABLE`;
- `GUROBI_HOME`;
- common Windows `C:\gurobi*\win64\bin` installations;
- `PATH`.

The adapter executes:

```text
gurobi_cl --version
gurobi_cl --license
```

and records version and license diagnostics.

The execution backend writes the portable model, requests a result file and
maps the returned portable `v_<id>` variable names to the model.

## FICO Xpress

Xpress remains optional and is loaded by reflection from `Optimizer.dll`.

Discovery checks:

- `ULSALGORITHMS_XPRESS_OPTIMIZER_ASSEMBLY`;
- compatibility variable `LOTSIZING_XPRESS_OPTIMIZER_ASSEMBLY`;
- `XPRESSDIR`;
- normal .NET assembly probing.

The adapter invokes `Optimizer.XPRS.Init` and `Optimizer.XPRS.Free`. Successful
initialization is the availability criterion because it exercises the managed
assembly, native runtime and license initialization path.

The execution backend reuses that optional runtime through reflection and maps
the returned solution to the portable model.

## COIN-OR CBC

CBC is probed through the stand-alone `cbc` executable.

Discovery checks:

- `ULSALGORITHMS_CBC_EXECUTABLE`;
- compatibility variable `LOTSIZING_CBC_EXECUTABLE`;
- `CBC_HOME`;
- `COINOR_HOME`;
- application/current-directory conventional `cbc` locations;
- `PATH`.

The adapter runs:

```text
cbc -quit
```

and parses the CBC version banner. CBC requires no commercial runtime license.

The execution backend invokes CBC on the portable LP model, parses the text
solution and returns the same normalized `LinearModelSolveResult` contract as
the commercial engines.

## Current end-to-end use

The adapters are no longer discovery-only infrastructure. The production path
is:

```text
UlsProblem
  -> formulation or cutting-plane strategy
  -> portable LinearModel
  -> LinearModelSolver
  -> automatic/explicit solver selection
  -> provider executor
  -> normalized native solution
  -> independent LinearModel validation
  -> reconstructed UlsSolution
  -> ULS feasibility/objective validation
```

The four formulation strategies are:

```text
aggregate-inventory-formulation
facility-location-formulation
shortest-path-formulation
inventory-eliminated-formulation
```

The two cutting-plane strategies are:

```text
general-ls-cutting-plane
wagner-whitin-ls-cutting-plane
```

All six are normal public exact strategies in the runtime catalog and can use
CPLEX, Gurobi, Xpress or CBC through this common execution layer.
