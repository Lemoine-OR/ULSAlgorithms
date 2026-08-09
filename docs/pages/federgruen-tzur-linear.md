\page federgruen_tzur_linear Federgruen-Tzur linear-time specializations

# Federgruen-Tzur linear-time specializations

Federgruen and Tzur (1991) derive two distinct O(n) versions of their general
forward O(n log n) algorithm. ULSAlgorithms exposes both as separate public
strategies instead of hiding them behind the general solver.

## 1. No speculative inventory motive

Public class:

`FedergruenTzurNoSpeculativeMotiveSolver`

The transformed variable costs are

\f[
C_t = p_t - H_{t-1},
\f]

where `H` is cumulative holding cost before period `t`.

No speculative motive is equivalent to

\f[
C_0 \ge C_1 \ge \cdots \ge C_{T-1},
\f]

or, in adjacent original-cost form,

\f[
p_t + h_t \ge p_{t+1}.
\f]

Federgruen and Tzur's Corollary 4 shows that every new candidate is inserted
at the end of the Minimal Optimal Predecessor list. Their simplified Step 1
therefore performs only:

- deletions from the front;
- deletions from the back;
- insertion at the back.

The resulting algorithm is O(n) time and O(n) space.

This condition is the same economic condition required by the linear-time
Wagelmans Wagner-Whitin specialization, but the implementation is a distinct
**forward Federgruen-Tzur algorithm** and is benchmarked separately.

## 2. Nondecreasing setup costs

Public class:

`FedergruenTzurNondecreasingSetupSolver`

Applicability:

\f[
f_0 \le f_1 \le \cdots \le f_{T-1}.
\f]

Variable production and holding costs may otherwise be general within the
non-negative `UlsProblem` contract, including speculative inventory motives.

Federgruen and Tzur's Corollary 2 shows that a new period either:

1. cannot belong to the updated Minimal Optimal Predecessor list; or
2. is inserted directly at the end.

Therefore no binary-tree search is required. Deletions occur only at the two
ends of the list, giving O(n) total time.

The 1991 paper specifically notes that this is an important special case and
that, at the time, no alternative linear-time method appeared to exist for
the prevalent nondecreasing-setup-cost setting.

## Data structure

Both implementations use
`FedergruenTzurLinearCandidateDeque`.

It is an array-backed monotone lower-envelope deque with:

- pooled slope array;
- pooled intercept array;
- pooled activation-threshold array;
- pooled period-index array;
- O(1) amortized front deletion;
- O(1) amortized back deletion/insertion;
- no candidate objects;
- no LINQ in the hot path.

Every candidate can enter and leave the deque at most once.

## Relationship to the general Federgruen-Tzur solver

| Solver | Condition | Time | Core structure |
|---|---|---:|---|
| `FedergruenTzurSolver` | general | O(n log n) | array-backed AVL tree |
| `FedergruenTzurNoSpeculativeMotiveSolver` | no speculation | O(n) | monotone deque |
| `FedergruenTzurNondecreasingSetupSolver` | setup costs nondecreasing | O(n) | monotone deque |

These are retained as three separate public algorithms for scientific
traceability and benchmarking.

## Validation

The test suite adds:

- 2,000 random no-speculative-motive instances cross-validated against:
  - the O(n²) independent oracle,
  - Federgruen-Tzur general,
  - Wagelmans general,
  - Wagelmans linear;
- 2,000 random nondecreasing-setup-cost instances with arbitrary variable
  production costs, cross-validated against:
  - the O(n²) independent oracle,
  - Federgruen-Tzur general,
  - Wagelmans general,
  - Evans 1985;
- explicit applicability and cancellation tests.

## Reference

A. Federgruen and M. Tzur (1991).

*A Simple Forward Algorithm to Solve General Dynamic Lot Sizing Models with n
Periods in O(n log n) or O(n) Time.*

Management Science, 37(8), 909-925.

DOI: https://doi.org/10.1287/mnsc.37.8.909

The nondecreasing-setup-cost algorithm is developed in Section 3 and the
no-speculative-motive algorithm in Section 4.
