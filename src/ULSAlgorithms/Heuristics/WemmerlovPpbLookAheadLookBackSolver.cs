using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements standard PPB followed by Wemmerlöv's modified
/// Look-Ahead/Look-Back tests.
/// </summary>
/// <remarks>
/// Reference:
/// U. Wemmerlöv,
/// "The Part-Period Balancing Algorithm and Its Look Ahead-Look Back Feature:
/// A Theoretical and Experimental Analysis of a Single Stage Lot-Sizing
/// Procedure",
/// Journal of Operations Management 4(1), 23-39, 1983.
/// DOI: 10.1016/0272-6963(83)90023-2.
/// </remarks>
public sealed class WemmerlovPpbLookAheadLookBackSolver : IUlsSolver
{
    public string Name =>
        "Wemmerlov PPB with Look-Ahead/Look-Back";

    public UlsSolverKind Kind => UlsSolverKind.Heuristic;

    public static bool IsApplicable(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        if (!ClassicHeuristicGuard.HasStationaryRelevantCosts(problem))
        {
            return false;
        }

        var demands = problem.Demands;

        for (var period = 0;
             period < demands.Length;
             period++)
        {
            if (!(demands[period] > 0.0))
            {
                return false;
            }
        }

        return true;
    }

    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        return WemmerlovPpbCore.Solve(
            problem,
            Name,
            correctionFactor: 0.0,
            useLookAheadLookBack: true,
            cancellationToken);
    }
}
