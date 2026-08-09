\page silver_meal Silver-Meal

# Silver-Meal

Public class: `SilverMealSolver`.

Reference:

E. A. Silver and H. C. Meal (1973),
*A heuristic for selecting lot size quantities for the case of a deterministic
time-varying demand rate and discrete opportunities for replenishment*,
Production and Inventory Management 14(2), 64-74.

For a lot beginning at period `s`, the method extends the covered horizon while

\f[
\frac{A+\text{holding}(s,t)}{t-s+1}
\f]

does not increase.

This implementation uses the classical stationary setup/production/holding-cost
domain and incremental O(T) scanning across generated cycles.
