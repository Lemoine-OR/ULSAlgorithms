\page cut_generation_traceability Cut Generation Traceability

# Cut Generation Traceability

Cutting-plane methods must expose the constraints they generate and the subset
that is actually inserted into the solver model.

## Solver-independent cut record

Every generated `(l,S)` inequality is represented by `LsCutDefinition`.

The definition records:

- `l`;
- the zero-based set `S`;
- every nonzero coefficient;
- the linear-constraint sense;
- the right-hand side.

The definition is independent of CPLEX, Gurobi, Xpress or CBC.

## Per-cut trace

`CutRecord` additionally records:

- stable sequence number;
- cutting-plane iteration;
- separator (`WagnerWhitin` or `General`);
- violation at generation time;
- efficacy;
- final disposition;
- whether the row was added;
- solver row name;
- reason for rejection/non-insertion.

Supported dispositions distinguish:

- added;
- duplicate;
- below tolerance;
- invalid;
- solver rejected.

Generated-but-not-added cuts therefore remain visible in the report.

## Iteration and solve summaries

`CutIterationReport` provides per-iteration counts, maximum violation and
separation time.

`CutGenerationReport` aggregates the complete solve and exposes:

- number of iterations;
- cuts generated;
- cuts added;
- duplicates;
- below-tolerance cuts;
- solver-rejected cuts;
- maximum violation;
- total separation time;
- the complete ordered list of cuts.

`CuttingPlaneExecutionReport` combines the cut report with
`SolverExecutionInfo`, providing a complete reproducibility record of both the
solver selected and the inequalities generated.
