using ULSAlgorithms.Models;

namespace ULSAlgorithms.Selection;

/// <summary>
/// Describes inexpensive structural and cost characteristics used to select
/// an exact ULS solution strategy.
/// </summary>
/// <remarks>
/// The analysis is deliberately independent of solver execution. It can be
/// reused by benchmarking, orchestration and future learned/empirical selection
/// policies without changing the common <c>IUlsSolver</c> contract.
/// </remarks>
public readonly record struct UlsProblemCharacteristics(
    int Horizon,
    double TotalDemand,
    int PositiveDemandPeriods,
    bool HasNoSpeculativeMotiveCosts,
    bool HasConstantSetupCosts,
    bool HasConstantUnitProductionCosts,
    bool HasConstantHoldingCosts)
{
    /// <summary>
    /// Gets the fraction of periods carrying strictly positive demand.
    /// </summary>
    public double DemandDensity =>
        Horizon == 0 ? 0.0 : (double)PositiveDemandPeriods / Horizon;
}

/// <summary>
/// Computes solver-selection characteristics for a validated ULS problem.
/// </summary>
public static class UlsProblemAnalyzer
{
    /// <summary>
    /// Analyzes the problem in one linear pass.
    /// </summary>
    /// <param name="problem">The validated ULS problem.</param>
    /// <returns>A compact immutable characteristic vector.</returns>
    /// <remarks>
    /// <para>
    /// The no-speculative-motive condition tested here is
    /// <c>p[t] + h[t] &gt;= p[t+1]</c>. When it holds for every adjacent pair,
    /// the linear-time Wagner-Whitin specialization implemented by
    /// <c>WagnerWhitinSolver</c> is applicable.
    /// </para>
    /// <para>
    /// Algorithmic basis: A. Wagelmans, S. van Hoesel and A. Kolen,
    /// "Economic Lot Sizing: An O(n log n) Algorithm That Runs in Linear Time
    /// in the Wagner-Whitin Case", Operations Research 40(S1), S145-S156,
    /// 1992, DOI: 10.1287/opre.40.1.S145.
    /// </para>
    /// </remarks>
    public static UlsProblemCharacteristics Analyze(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var horizon = problem.Horizon;
        var demands = problem.Demands;
        var setupCosts = problem.SetupCosts;
        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        var positiveDemandPeriods = 0;
        var noSpeculativeMotive = true;
        var constantSetup = true;
        var constantProduction = true;
        var constantHolding = true;

        var firstSetup = setupCosts[0];
        var firstProduction = productionCosts[0];
        var firstHolding = holdingCosts[0];

        for (var period = 0; period < horizon; period++)
        {
            if (demands[period] > 0.0)
            {
                positiveDemandPeriods++;
            }

            if (setupCosts[period] != firstSetup)
            {
                constantSetup = false;
            }

            if (productionCosts[period] != firstProduction)
            {
                constantProduction = false;
            }

            if (holdingCosts[period] != firstHolding)
            {
                constantHolding = false;
            }

            if (period < horizon - 1)
            {
                var deliveredNextPeriod =
                    productionCosts[period] + holdingCosts[period];

                if (!double.IsFinite(deliveredNextPeriod) ||
                    deliveredNextPeriod < productionCosts[period + 1])
                {
                    noSpeculativeMotive = false;
                }
            }
        }

        return new UlsProblemCharacteristics(
            horizon,
            problem.TotalDemand,
            positiveDemandPeriods,
            noSpeculativeMotive,
            constantSetup,
            constantProduction,
            constantHolding);
    }
}
