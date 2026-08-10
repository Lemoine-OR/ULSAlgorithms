\page ls_cutting_planes Classical (l,S) Cutting-Plane Algorithms

# Classical (l,S) Cutting-Plane Algorithms

ULSAlgorithms v0.20.0 adds two public exact cut-and-solve strategies:

```text
GeneralLsCuttingPlaneSolver
WagnerWhitinLsCuttingPlaneSolver
```

They share the normal `IUlsSolver` and `IAsyncUlsSolver` contracts.

## Classical (l,S) inequalities

Let:

\f[
d_{j,l}=\sum_{t=j}^{l}d_t,
\qquad
L=\{0,\ldots,l\}.
\f]

For every \f$S\subseteq L\f$, the classical inequality is

\f[
\sum_{j\in S}x_j+
\sum_{j\in L\setminus S}d_{j,l}y_j
\ge d_{0,l}.
\f]

These inequalities give the classical convex-hull description of ULS.

Primary references:

- I. Barany, T.J. Van Roy, L.A. Wolsey,
  *Uncapacitated lot-sizing: the convex hull of solutions*,
  Mathematical Programming Study 22 (1984), 32-43,
  DOI `10.1007/BFb0121006`.
- I. Barany, T.J. Van Roy, L.A. Wolsey,
  *Strong Formulations for Multi-Item Capacitated Lot Sizing*,
  Management Science 30(10) (1984), 1255-1261,
  DOI `10.1287/mnsc.30.10.1255`.

## General exact separator

For fixed \f$l\f$, each period contributes either:

\f[
x_j
\f]

or

\f[
d_{j,l}y_j.
\f]

Therefore the minimum left-hand side over all subsets \f$S\f$ is

\f[
\sum_{j=0}^{l}
\min\{x_j,d_{j,l}y_j\}.
\f]

The exact most-violated subset is thus

\f[
S_l^*=
\{j\le l:x_j\le d_{j,l}y_j\}.
\f]

`GeneralLsCutSeparator` generates one exact separation candidate for every
\f$l\f$.

Complexity:

```text
time   O(T²)
space  O(T) working memory
```

This separator is valid for the general cost structure represented by
`UlsProblem`.

## Wagner-Whitin separator

Under the no-speculative-motive condition

\f[
p_t+h_t\ge p_{t+1},
\f]

Pochet and Wolsey show that the ULS polyhedral structure simplifies
substantially.

The Wagner-Whitin specialization used here considers prefix sets

\f[
S=\{0,\ldots,k-1\}.
\f]

Using the inventory-balance equations, the canonical `(l,S)` inequality is
equivalent to

\f[
I_{k-1}+
\sum_{j=k}^{l}d_{j,l}y_j
\ge d_{k,l}.
\f]

`WagnerWhitinLsCutSeparator` evaluates every pair \f$(k,l)\f$ using prefix
production sums and backward weighted-setup sums.

Complexity:

```text
time   O(T²)
space  O(T)
```

Primary reference:

Y. Pochet, L.A. Wolsey,
*Polyhedra for lot-sizing with Wagner-Whitin costs*,
Mathematical Programming 67 (1994), 297-323,
DOI `10.1007/BF01582225`.

## Cut-and-solve architecture

Both public strategies use the same root-loop architecture:

```text
aggregate formulation
        ↓
relax binary setups to [0,1]
        ↓
solve root LP
        ↓
separate (l,S)
        ↓
record every candidate
        ↓
add every unique violated cut
        ↓
repeat until no new violated cut
        ↓
restore binary setups
        ↓
solve strengthened MILP exactly
        ↓
reconstruct UlsSolution
        ↓
ULS independent checker
```

The final MILP is solved with the **same optimization engine** selected during
the first LP iteration. Automatic priority remains:

```text
CPLEX -> Gurobi -> Xpress -> CBC
```

The final MILP guarantees exactness even if `MaximumIterations` stops the
root-separation loop before complete closure.

## Traceability

Every separator candidate creates a `CutRecord`.

For each cut the result exposes:

```text
sequence number
iteration
separator
l
S
all coefficients
sense
right-hand side
violation
efficacy
disposition
solver row name
disposition reason
```

Typical dispositions are:

```text
Added
Duplicate
BelowTolerance
```

`CuttingPlaneUlsSolveResult` exposes:

```text
SeparationMethod
CuttingPlaneExecution
FinalModelExecution
Solution
```

and the complete report is available through:

```csharp
var r = (CuttingPlaneUlsSolveResult)result;

foreach (CutRecord cut in
         r.CuttingPlaneExecution.Cuts.Cuts)
{
    Console.WriteLine(
        $"{cut.Iteration}: {cut.Definition} -> {cut.Disposition}");
}
```

This directly answers which constraints were generated and which were actually
added.
