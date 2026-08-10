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

```csharp
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;

IUlsSolver solver = new WagnerWhitinSolver();
```

Every public algorithm implements the same `IUlsSolver` contract. Changing method therefore changes only the instantiated class.

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
- Need to understand inputs/results: see @ref simple_api.
- Need complexity or assumptions: see @ref complexity_applicability.
- Need every class/member: use the generated API tabs.
