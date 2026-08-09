\page patterson_laforge_ippa Patterson-LaForge Incremental Part-Period Algorithm

# Patterson-LaForge Incremental Part-Period Algorithm

Public class: `PattersonLaForgeIncrementalPartPeriodSolver`.

Reference:

J. W. Patterson and R. L. LaForge (1985),
*The Incremental Part-Period Algorithm: An Alternative to EOQ*,
Journal of Purchasing and Materials Management 21(2), 28-33.

DOI: https://doi.org/10.1111/j.1745-493X.1985.tb00132.x

## Rule

For a lot beginning in `s`, IPPA accumulates the holding cost created by
successive future requirements and extends the lot while

\f[
h\sum_{t=s+1}^{j}(t-s)d_t \le A.
\f]

The first candidate that would make cumulative incremental holding cost exceed
the setup cost starts the next search cycle.

## Relationship to PPB

PPB selects the cumulative part-period quantity **closest** to the economic
part-period target. IPPA instead uses a one-sided incremental stopping rule.
That difference is preserved explicitly in the public strategies.
