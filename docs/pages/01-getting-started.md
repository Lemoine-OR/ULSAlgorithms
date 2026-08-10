\page getting_started Getting Started

# Getting Started

ULSAlgorithms is designed around one simple workflow:

```text
1. Create a UlsProblem
2. Choose an IUlsSolver
3. Call Solve
4. Read UlsSolveResult
```

## 1. Create the problem

```csharp
using ULSAlgorithms.Models;

var problem = new UlsProblem(
    demands:             [20.0, 30.0, 25.0, 40.0],
    setupCosts:          [200.0, 200.0, 200.0, 200.0],
    unitProductionCosts: [0.0, 0.0, 0.0, 0.0],
    holdingCosts:        [4.0, 4.0, 4.0, 0.0]);
```

All four arrays have one value per planning period:

| Parameter | Meaning |
|---|---|
| `demands` | demand to satisfy in each period |
| `setupCosts` | fixed cost paid when production starts in a period |
| `unitProductionCosts` | variable production cost per unit |
| `holdingCosts` | cost of carrying one unit of end-of-period inventory |

Periods are zero-based. Backlogging is not allowed and initial inventory is zero.

## 2. Choose an algorithm

You can instantiate a concrete strategy directly:

```csharp
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;

IUlsSolver solver = new WagnerWhitinSolver();
```

Or use the stable runtime catalog/factory introduced in v0.26.0:

```csharp
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Catalog;

IUlsSolver solver =
    UlsSolverFactory.Create("adaptive-exact");
```

The factory is useful for configuration files, command-line tools, experiment
campaigns and user interfaces because client code does not need a compile-time
switch over concrete strategy classes.

Every public algorithm still implements the same `IUlsSolver` contract.

For constructor-level configuration, use the overload introduced in v0.27.0:

```csharp
var solver =
    UlsSolverFactory.Create(
        "lyu-lee-parallel",
        new UlsSolverCreationOptions
        {
            MaxDegreeOfParallelism = 4,
            ParallelThreshold = 256
        });
```

See @ref solver_catalog_factory for adaptive fallback, external optimization
engine and cutting-plane examples.

## 3. Solve

```csharp
var result = solver.Solve(problem);
```

For long-running or solver-backed methods, an optional cancellation token can be passed.

## 4. Read the result

```csharp
Console.WriteLine(result.Status);
Console.WriteLine(result.ObjectiveValue);

if (result.Solution is not null)
{
    Console.WriteLine(string.Join(", ", result.Solution.ProductionQuantities.ToArray()));
}
```

Exact methods may return `Optimal`. Heuristics return `Feasible` because they do not claim an optimality proof.

## What should I read next?

- Need an algorithm: use the card-based documentation home page.
- Need runtime discovery/factory creation: see @ref solver_catalog_factory.
- Need to understand inputs/results: see @ref simple_api.
- Need complexity or assumptions: see @ref complexity_applicability.
- Need every class/member: use the generated API tabs.
