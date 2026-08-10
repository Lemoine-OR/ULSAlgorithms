\page api_reference_guide API Reference Guide

# API Reference Guide

## Common strategy contract

All public solvers implement `IUlsSolver`.

```csharp
public interface IUlsSolver
{
    string Name { get; }
    UlsSolverKind Kind { get; }

    UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default);
}
```

This is the primary extension point for algorithm selection.

## Input model

`UlsProblem` validates and stores:

- `Demands`;
- `SetupCosts`;
- `UnitProductionCosts`;
- `HoldingCosts`;
- `Horizon`;
- `TotalDemand`.

The constructor copies the input vectors once. Solvers then access contiguous read-only spans.

## Result model

`UlsSolveResult` separates the solve status from the returned solution.

Use the status to distinguish:

- exact optimal solutions;
- heuristic feasible solutions;
- other explicit result states defined by the API.

## Navigating generated API docs

Use the left tree for namespaces and classes, or the search box for a specific solver class.

Useful namespaces include:

- `ULSAlgorithms.Abstractions`;
- `ULSAlgorithms.Models`;
- `ULSAlgorithms.Results`;
- `ULSAlgorithms.Exact`;
- `ULSAlgorithms.Heuristics`.

Return to @ref overview at any time for conceptual navigation.
