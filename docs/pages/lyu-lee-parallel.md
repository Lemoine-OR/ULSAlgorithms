\page lyu_lee_parallel Lyu-Lee parallel dynamic lot-sizing

# Lyu-Lee parallel dynamic lot-sizing

Public class: `LyuLeeParallelSolver`.

Reference:

J.-J. Lyu and M.-C. Lee (2001),
*A parallel algorithm for the dynamic lot-sizing problem*,
Computers & Industrial Engineering 41(2), 127-134.
DOI: https://doi.org/10.1016/S0360-8352(01)00047-X

## Published contribution

The paper develops an exact parallel DLS algorithm. The publisher abstract
reports complexity proportional to O(n²/p) for `p` processors and empirical
speedup approaching linearity as problem size grows.

Descriptions of the method characterize it as a reorganization of the
Wagner-Whitin recursion into a lower-triangular matrix whose cells are
calculated in parallel and whose optimal policy is selected by the master
process.

## Implementation transparency

The complete original program listing was not available in the sources used
for this implementation. `LyuLeeParallelSolver` is therefore explicitly a
modern shared-memory reconstruction of that published parallel architecture,
not a claim of source-code transliteration.

At endpoint `t`, every candidate predecessor `j<t` depends only on already
finalized `F(j)`. Candidate cells can therefore be partitioned between
workers. Each worker computes a local minimum and the calling thread performs
the deterministic global reduction.

## O(1) arc evaluation

General time-varying setup, production and holding costs are supported.

Cumulative arrays allow each regeneration interval to be evaluated in O(1)
without materializing the complete lower-triangular matrix.

## Parallelism

`LyuLeeParallelSolver()` uses the runtime processor count and switches to
parallel evaluation only when at least 128 predecessor cells are present.

The configurable constructor exposes:

- `maxDegreeOfParallelism`;
- `parallelThreshold`.

Using one worker provides a deterministic sequential version of the same
matrix recurrence for testing.

## Complexity

- total arithmetic work: O(T²);
- ideal predecessor-cell parallel work: approximately O(T²/p);
- synchronization: one reduction/barrier per endpoint;
- auxiliary memory: O(T).

The actual speedup depends on horizon length, processor count, runtime
scheduling and memory hierarchy.

## Validation

2,000 deterministic random general-cost instances are cross-validated against:

- the independent quadratic oracle;
- `WagelmansGeneralSolver`.

The test suite also compares one-worker and multi-worker execution directly.
