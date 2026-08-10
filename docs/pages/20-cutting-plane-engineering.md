\page cutting_plane_engineering Cutting-Plane Engineering and Convergence

# Cutting-Plane Engineering and Convergence

ULSAlgorithms v0.21.0 adds cut-pool management, root convergence statistics and
pure separator benchmarks without changing the exactness guarantee introduced
in v0.20.0.

## Cut-selection policies

`LsCuttingPlaneOptions.SelectionPolicy` supports:

```text
AllViolated
MostViolatedPerL
TopByViolation
TopByEfficacy
```

`AllViolated` is the v0.20.0 behavior and remains the default.

For the two top-K policies, use:

```csharp
new LsCuttingPlaneOptions
{
    SelectionPolicy =
        CutSelectionPolicy.TopByViolation,
    MaximumCutsPerIteration = 20
};
```

A minimum efficacy can also be imposed:

```csharp
MinimumEfficacy = 1e-4
```

Eligible violated cuts that are deliberately not inserted are retained in the
trace with:

```text
CutDisposition.NotSelected
```

Therefore cut-pool management never destroys observability.

## Convergence statistics

Every root iteration now records:

```text
LP objective
LP solve time
separation time
generated candidates
eligible candidates
selected cuts
added cuts
cumulative cuts
maximum violation
mean positive violation
maximum efficacy
```

The complete solve exposes:

```text
InitialLpObjective
FinalRootLpObjective
FinalMipObjective
RootBoundImprovement
RootGapClosedFraction
TotalLpSolveTime
TotalSeparationTime
```

For a minimization problem, root-gap closure is:

\f[
\frac{z_{LP}^{final}-z_{LP}^{initial}}
     {z_{MIP}-z_{LP}^{initial}}.
\f]

The metric is omitted when the initial LP already matches the final MILP
objective numerically.

## Exactness

Cut selection changes only root strengthening.

The algorithm still solves the final strengthened model with binary setup
variables and the same optimization engine selected for the first LP. Therefore
all selection policies retain the exact final MILP guarantee.

## Benchmarks

`LsSeparationBenchmarks` measures the two separation engines independently of
CPLEX, Gurobi, Xpress or CBC.

Benchmark horizons:

```text
50
100
250
500
```

The benchmark reports both time and allocations through BenchmarkDotNet's
`MemoryDiagnoser`.

Keeping solver time outside this benchmark makes it possible to measure whether
future separator/data-structure changes actually improve the ULSAlgorithms code
rather than merely observing optimization-engine variability.
