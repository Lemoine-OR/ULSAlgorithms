\page solver_adapters Concrete Solver Adapters

# Concrete Solver Adapters

ULSAlgorithms ships four optional machine-discovery adapters for future
solver-backed formulations and cutting-plane methods.

## Automatic priority

The default order is deliberately identical to LotSizingDataModel:

1. **IBM ILOG CPLEX**
2. **Gurobi Optimizer**
3. **FICO Xpress MP**
4. **COIN-OR CBC**

Use:

```csharp
SolverSelectionResult selection =
    await OptimizationSolverDiscovery.SelectAsync(
        SolverKind.Automatic,
        options,
        cancellationToken);
```

No manual registry construction is required.

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

## Scope of this release

These adapters provide **real machine availability detection and provenance**.

They intentionally do not yet define a generic mathematical-model execution API.
The next solver-backed ULS formulations will consume the selected adapter and
their own execution backend while preserving the same automatic selection and
diagnostic contract.
