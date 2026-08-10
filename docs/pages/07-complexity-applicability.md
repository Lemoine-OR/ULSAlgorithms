\page complexity_applicability Complexity and Applicability

# Complexity and Applicability

The complete machine-readable algorithm inventory is maintained in
`docs/algorithm-catalog.json` and rendered automatically as @ref algorithm_catalog.

## Reading the complexity table

- \(T\) is the number of planning periods.
- \(p\) is the number of worker threads/processors in the parallel complexity statement.
- "Worst case" means data-dependent pruning may be much faster on favorable instances.
- Memory figures refer to algorithmic working memory, excluding the immutable input and returned solution arrays.

## Restricted methods

Several fast algorithms require one or more of:

- no speculative motive;
- stationary holding costs;
- constant unit production costs;
- strictly positive demand;
- nondecreasing setup costs.

The library does not silently coerce a general instance into a restricted model.

## Numerical representation

Costs and quantities use `double`. Implementations check input validity and many accumulation paths explicitly guard against non-finite intermediate results.

For algorithm-by-algorithm details, open @ref algorithm_catalog and then the corresponding class or scientific page.
