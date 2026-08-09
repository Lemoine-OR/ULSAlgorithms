\page periodic_order_quantity Periodic Order Quantity

# Periodic Order Quantity

Public class: `PeriodicOrderQuantitySolver`.

POQ converts an EOQ-style quantity into a number of demand periods. With
average per-period demand `dBar`:

\f[
P \approx \sqrt{\frac{2A}{h\,\bar d}}.
\f]

The implementation rounds `P` to the nearest positive integer number of
calendar periods and each replenishment covers that many periods.

This is a classical MRP baseline rather than an optimal dynamic lot-sizing
algorithm. `GetOrderInterval(problem)` exposes the selected interval.
