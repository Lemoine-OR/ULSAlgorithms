\page method_families Method Families

# Method Families

The public strategy catalog is organized by what the user actually needs to
choose, not by historical release packs.

## Exact algorithms

Direct algorithms that solve ULS without delegating the mathematical problem
to an external MILP solver. The current library includes dynamic programming,
geometric acceleration, planning-horizon methods, network methods,
branch-and-bound and parallel variants.

Use these when you want a self-contained algorithm implementation with a proven
optimum.

## Mathematical optimization

Exact formulations translated to a portable linear/MILP model and solved by an
external optimization engine. Automatic discovery follows:

```text
CPLEX -> Gurobi -> Xpress -> COIN-OR CBC
```

Use these when formulation choice, solver comparison or mathematical-model
integration matters.

## Cutting planes

Exact solver-backed methods that strengthen the root model with classical
`(l,S)` inequalities before the final exact MILP solve. Generated, rejected and
added cuts remain traceable.

Use these when you want polyhedral strengthening and convergence information.

## Heuristic strategies

Fast construction rules that return a feasible plan but do not claim
optimality. Families include baseline rules, average-cost rules, part-period
methods, marginal-cost rules and look-ahead/look-back variants.

Use these when speed, warm starts or comparison against classical planning
rules matters.

## Not currently present

The documentation does not display empty categories. Metaheuristics or other
future method families will appear only when the repository contains actual
public implementations.
