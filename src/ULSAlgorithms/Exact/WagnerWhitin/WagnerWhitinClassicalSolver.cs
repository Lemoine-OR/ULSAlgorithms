using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.WagnerWhitin;

/// <summary>
/// Implements the classical Wagner-Whitin shortest-path dynamic program.
/// </summary>
/// <remarks>
/// <para>
/// This implementation deliberately materializes the complete triangular matrix
/// of regeneration-interval costs before running the dynamic program. It is
/// therefore useful as a transparent classical implementation and as a
/// benchmarking baseline.
/// </para>
/// <para>
/// Time complexity: <c>O(n^2)</c>.
/// Space complexity: <c>O(n^2)</c>.
/// </para>
/// <para>
/// Reference:
/// H. M. Wagner and T. M. Whitin,
/// "Dynamic Version of the Economic Lot Size Model",
/// Management Science 5(1), 89-96, 1958.
/// DOI: 10.1287/mnsc.5.1.89.
/// </para>
/// </remarks>
public sealed class WagnerWhitinClassicalSolver : IUlsSolver
{
    /// <inheritdoc />
    public string Name => "Wagner-Whitin classical";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <inheritdoc />
    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        var horizon = problem.Horizon;
        var arcCosts = new double[checked(horizon * horizon)];

        BuildArcCostMatrix(problem, arcCosts, cancellationToken);

        var value = new double[horizon + 1];
        var predecessor = new int[horizon + 1];

        Array.Fill(value, double.PositiveInfinity);
        Array.Fill(predecessor, -1);
        value[0] = 0.0;

        for (var end = 1; end <= horizon; end++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var best = double.PositiveInfinity;
            var bestStart = -1;

            for (var start = 0; start < end; start++)
            {
                var arcCost = arcCosts[(start * horizon) + (end - 1)];
                var candidate = value[start] + arcCost;

                if (candidate < best)
                {
                    best = candidate;
                    bestStart = start;
                }
            }

            if (!double.IsFinite(best) || bestStart < 0)
            {
                throw new ArithmeticException(
                    $"No finite Wagner-Whitin value was obtained for horizon prefix {end}.");
            }

            value[end] = best;
            predecessor[end] = bestStart;
        }

        return ZeroInventoryOrderSolutionBuilder.Build(
            problem,
            predecessor,
            Name,
            cancellationToken);
    }

    private static void BuildArcCostMatrix(
        UlsProblem problem,
        Span<double> arcCosts,
        CancellationToken cancellationToken)
    {
        var horizon = problem.Horizon;
        var demands = problem.Demands;
        var setupCosts = problem.SetupCosts;
        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        for (var start = 0; start < horizon; start++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var deliveredUnitCost = productionCosts[start];
            var batchCost = 0.0;
            var cumulativeDemand = 0.0;

            for (var end = start; end < horizon; end++)
            {
                cumulativeDemand += demands[end];

                batchCost += demands[end] * deliveredUnitCost;

                if (!double.IsFinite(batchCost))
                {
                    throw new ArithmeticException(
                        "Numerical overflow while computing a Wagner-Whitin arc cost.");
                }

                arcCosts[(start * horizon) + end] =
                    cumulativeDemand > 0.0
                        ? setupCosts[start] + batchCost
                        : 0.0;

                if (!double.IsFinite(arcCosts[(start * horizon) + end]))
                {
                    throw new ArithmeticException(
                        "Numerical overflow while computing a Wagner-Whitin regeneration interval.");
                }

                if (end < horizon - 1)
                {
                    deliveredUnitCost += holdingCosts[end];

                    if (!double.IsFinite(deliveredUnitCost))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow while accumulating delivered unit cost.");
                    }
                }
            }
        }
    }
}
