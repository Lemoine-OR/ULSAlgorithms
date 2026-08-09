\page chowdhury_baki_azab Chowdhury-Baki-Azab O(T) algorithm

# Chowdhury-Baki-Azab O(T) algorithm

Public class: `ChowdhuryBakiAzabSolver`.

## Identity

This is a separate exact linear-time algorithm for the Wagner-Whitin model. It
does not call the Wagelmans or Federgruen-Tzur implementations.

Reference:

N. T. Chowdhury, M. F. Baki and A. Azab (2018),
*Dynamic Economic Lot-Sizing Problem: A new O(T) Algorithm for the
Wagner-Whitin Model*,
Computers & Industrial Engineering 117, 6-18.
DOI: https://doi.org/10.1016/j.cie.2018.01.010

The implementation follows Algorithm 1 in Chapter 2 of Chowdhury's doctoral
dissertation, where the paper's definitions, lemmas, theorems and pseudocode
are reproduced in full.

## Core data structures

The paper introduces triangular advantage matrices A and B for exposition, but
Algorithm 1 never materializes them. It keeps:

- `a(k)` and `b(k)` line parameters;
- an active doubly linked set of diagonals through predecessor/successor arrays;
- scheduled lists `L(k)`;
- compressed stack summaries;
- the current best diagonal `i*`.

ULSAlgorithms represents all of these with pooled primitive arrays.

Scheduled list entries use a compact singly-linked event pool. The proof in the
paper bounds the total list cardinality by `2T-4` for `T >= 3`, so the storage
is O(T).

## Published arithmetic

For each backward period `k`:

\f[
a(k)=G(k+1)-G(k+2)-h d_{k+1},
\qquad
b(k)=h d_{k+1}.
\f]

The deletion event is scheduled using

\f[
u=\max\{\min\{\lceil a(k)/b(k)\rceil,T\},0\}.
\f]

The implementation also reproduces the stack-compression update of Algorithm
1 and computes `G(k)` from the selected successor in O(1) using cumulative
demand and cumulative weighted demand.

## Applicability

The paper's ELSP specialization uses stationary holding cost and time-varying
setup costs. Constant production cost is policy-independent and is therefore
permitted.

For numerical faithfulness to Algorithm 1, this implementation currently
requires:

- strictly positive demands;
- strictly positive stationary relevant holding cost;
- constant unit production cost;
- arbitrary nonnegative time-varying setup costs.

Zero-demand periods are rejected rather than silently modifying the published
`b(k)=h d(k+1)` divisions.

## Complexity

The paper proves:

- total scheduled list cardinality O(T);
- every deleted diagonal is deleted once;
- total execution time O(T).

ULSAlgorithms additionally uses O(T) pooled memory and no per-period managed
candidate allocations.

## Validation

The suite cross-validates 5,000 deterministic random instances against:

- the independent quadratic oracle;
- the independent Wagelmans O(T) solver;
- the independent Federgruen-Tzur O(T) solver.
