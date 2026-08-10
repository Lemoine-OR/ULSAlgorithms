\page getting_started Getting Started

# Getting Started

## 1. Create a ULS problem

`UlsProblem` stores four vectors of equal length:

- demand `d[t]`;
- setup cost `f[t]`;
- unit production cost `p[t]`;
- end-of-period unit holding cost `h[t]`.

```csharp
using ULSAlgorithms.Models;

var problem = new UlsProblem(
    demands:             [20.0, 30.0, 25.0, 40.0],
    setupCosts:          [100.0, 100.0, 100.0, 100.0],
    unitProductionCosts: [  0.0,   0.0,   0.0,   0.0],
    holdingCosts:        [  2.0,   2.0,   2.0,   0.0]);
```

Periods are zero-based in the API.

## 2. Select an algorithm

Every public solver implements `IUlsSolver`.

```csharp
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;

IUlsSolver solver = new WagnerWhitinSolver();
```

Use @ref algorithm_selection and @ref complexity_applicability before selecting a restricted method.

## 3. Solve

```csharp
var result = solver.Solve(problem);

Console.WriteLine(result.Status);
Console.WriteLine(result.ObjectiveValue);
```

Exact methods return `Optimal` after exact completion. Heuristics return `Feasible` and never claim optimality.

## 4. Exchange strategies without changing caller code

```csharp
IUlsSolver[] solvers =
[
    new WagnerWhitinSolver(),
    new WagelmansGeneralSolver(),
    new SilverMealSolver()
];

foreach (var candidate in solvers)
{
    var candidateResult = candidate.Solve(problem);
    Console.WriteLine(
        $"{candidate.Name}: {candidateResult.ObjectiveValue}");
}
```

## Next steps

- @ref problem_and_notation
- @ref algorithm_catalog
- @ref validation_benchmarks
