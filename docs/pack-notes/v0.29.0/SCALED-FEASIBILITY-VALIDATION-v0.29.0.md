# v0.29.0 — scaled feasibility validation

The real CBC end-to-end qualification exposed a numerical interoperability
issue in the independent portable-model checker.

CBC 2.10.13 returned an optimal root LP point with objective
`374.86358036`, but the text solution round-trip produced an absolute row
residual of approximately `4e-7`. With the historical absolute feasibility
tolerance `1e-7`, the checker rejected the otherwise valid LP point.

A temporary `1e-6` diagnostic run confirmed that all six solver-backed
strategies then solved to the independently known ULS optimum `680`.

The permanent fix does not simply loosen the global tolerance. Constraint
validation now uses a mixed absolute/relative rule:

```text
normalized violation =
    absolute violation /
    max(1, |rhs|, sum_i |a_i x_i|)
```

The configured `FeasibilityTolerance` remains `1e-7`.

Consequences:

- small/zero-scale rows retain the original absolute protection;
- larger rows tolerate harmless text/native solver rounding proportionally;
- material scaled violations are still rejected;
- the reported `MaximumConstraintViolation` remains the absolute violation,
  preserving the existing public result contract.

The CBC qualification smoke is restored to the default tolerance; no
test-specific relaxation remains.
