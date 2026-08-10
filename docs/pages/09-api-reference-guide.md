\page simple_api Simple API

# Simple API

Most users need only four public types.

## `UlsProblem`

Input data for one deterministic ULS instance.

```csharp
var problem = new UlsProblem(
    demands,
    setupCosts,
    unitProductionCosts,
    holdingCosts);
```

All vectors have length `Horizon`.

## `IUlsSolver`

The common strategy interface.

```csharp
IUlsSolver solver = new WagnerWhitinSolver();
var result = solver.Solve(problem);
```

Every algorithm exposes:

| Member | Meaning |
|---|---|
| `Name` | readable algorithm name |
| `Kind` | exact or heuristic |
| `Solve(problem, cancellationToken)` | solve the instance |

## `UlsSolveResult`

The common result wrapper.

| Member | Meaning |
|---|---|
| `Status` | optimal, feasible, infeasible, failed, … |
| `ObjectiveValue` | total cost when a solution exists |
| `Solution` | detailed production plan |
| `Message` | optional diagnostic information |

## `UlsSolution`

The detailed plan contains production quantities, ending inventories, setup decisions and cost components.

## Advanced APIs

You only need the advanced layer when using solver selection, mathematical formulations, cutting-plane reports or custom execution backends. Those types remain fully documented in the generated class/namespace reference but are intentionally kept out of the basic workflow.

This split is deliberate: **simple API first, implementation API second**.
