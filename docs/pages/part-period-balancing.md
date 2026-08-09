\page part_period_balancing Part-Period Balancing

# Part-Period Balancing

Public class: `PartPeriodBalancingSolver`.

Early primary reference:

J. J. DeMatteis (1968),
*An economic lot-sizing technique I: The part-period algorithm*,
IBM Systems Journal 7(1).

The classical Economic Part Period is

\f[
EPP=A/h.
\f]

The method chooses the cycle whose accumulated part-periods are closest to
`EPP`. It is also widely described as the Least Total Cost / Part-Period
Balancing family.

`GetEconomicPartPeriod(problem)` exposes the threshold used by the solver.
