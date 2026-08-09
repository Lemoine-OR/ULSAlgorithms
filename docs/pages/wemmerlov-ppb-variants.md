\page wemmerlov_ppb_variants Wemmerlöv PPB variants

# Wemmerlöv PPB variants

Version 0.13.0 implements three variants explicitly analyzed in:

U. Wemmerlöv (1983),
*The Part-Period Balancing Algorithm and Its Look Ahead-Look Back Feature:
A Theoretical and Experimental Analysis of a Single Stage Lot-Sizing
Procedure*,
Journal of Operations Management 4(1), 23-39.

DOI: https://doi.org/10.1016/0272-6963(83)90023-2

The public strategies are:

- `WemmerlovModifiedPartPeriodBalancingSolver`;
- `WemmerlovPpbLookAheadLookBackSolver`;
- `WemmerlovModifiedPpbLookAheadLookBackSolver`.

## Corrected PPB

Wemmerlöv rewrites the ordinary PPB balance as

\f[
\left|
A-h\sum_{j=1}^{T}(j-1+\nu)d_{k+j-1}
\right|.
\f]

The paper derives a positive correction factor and reports that the practical
limiting value

\f[
\nu=0.5
\f]

can be used with only a small empirical penalty relative to item-specific
values.

`WemmerlovModifiedPartPeriodBalancingSolver` therefore exposes this
recommended fixed correction explicitly rather than silently changing the
existing classical `PartPeriodBalancingSolver`.

## Look-Ahead / Look-Back

The modified LALB procedure in Figure 4 locally adjusts a tentative PPB lot.

Suppose a replenishment in `k` tentatively covers `T` periods and the next
replenishment is therefore planned for `k+T`.

### Look-Ahead gate

Before moving the next replenishment one period forward, the algorithm checks

\f[
h T d_{k+T} \le A.
\f]

### Look-Ahead comparison

The two-period local cost comparison is

\f[
h\{\nu d_{k+T}+(1+\nu)d_{k+T+1}\}
\f]

versus

\f[
h\{(T+\nu)d_{k+T}+\nu d_{k+T+1}\}.
\f]

When moving the next replenishment forward is cheaper, the tentative current
lot is enlarged by one period.

### Look-Back comparison

If Look-Ahead does not move the replenishment, the final requirement already
inside the tentative lot is tested against moving it into the next batch:

\f[
h\{(T-1+\nu)d_{k+T-1}+\nu d_{k+T}\}
\f]

versus

\f[
h\{\nu d_{k+T-1}+(1+\nu)d_{k+T}\}.
\f]

If the shifted pattern is cheaper, the tentative lot is shortened by one
period.

## Conservative applicability

The original analysis is for stationary ordering and holding costs. These
strategies therefore use the library's stationary-cost guard.

For LALB, ULSAlgorithms additionally requires strictly positive period demand.
The paper's local formulas contain explicitly identified adjacent
requirements; rather than inventing a zero-demand preprocessing convention,
the current implementation rejects such instances.

The corrected PPB strategy without LALB continues to support zero-demand
periods.

## Empirical results in the paper

Wemmerlöv evaluates four variants separately:

1. PPB;
2. corrected PPB;
3. PPB/LALB;
4. corrected PPB/LALB.

The study reports statistically significant average improvements from both
the correction factor and LALB, while also warning that Look-Back can perform
poorly in constant or near-constant demand environments. The library therefore
keeps all variants public instead of assuming one universally dominates the
others.
