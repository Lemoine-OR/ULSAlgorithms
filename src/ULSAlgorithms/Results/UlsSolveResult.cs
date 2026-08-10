namespace ULSAlgorithms.Results;

/// <summary>
/// Represents the outcome returned by a ULS solution strategy.
/// </summary>
/// <remarks>
/// The class is intentionally extensible so solver-backed strategies can expose
/// additional execution provenance while remaining assignable to the common
/// <see cref="UlsSolveResult"/> contract.
/// </remarks>
public class UlsSolveResult
{
    /// <summary>
    /// Initializes a solve result.
    /// </summary>
    /// <param name="solverName">Stable human-readable solver name.</param>
    /// <param name="status">Mathematical solve status.</param>
    /// <param name="solution">Optional feasible solution.</param>
    /// <param name="message">Optional diagnostic message.</param>
    public UlsSolveResult(
        string solverName,
        UlsSolveStatus status,
        UlsSolution? solution = null,
        string? message = null)
    {
        if (string.IsNullOrWhiteSpace(solverName))
        {
            throw new ArgumentException(
                "A solve result must identify its solver.",
                nameof(solverName));
        }

        ValidateStatusAndSolution(
            status,
            solution);

        SolverName = solverName;
        Status = status;
        Solution = solution;
        Message = message;
    }

    /// <summary>Gets the solver that produced the result.</summary>
    public string SolverName { get; }

    /// <summary>Gets the mathematical solve status.</summary>
    public UlsSolveStatus Status { get; }

    /// <summary>Gets the feasible solution, when one is available.</summary>
    public UlsSolution? Solution { get; }

    /// <summary>Gets an optional diagnostic message.</summary>
    public string? Message { get; }

    /// <summary>Gets whether the result contains a feasible solution.</summary>
    public bool HasSolution => Solution is not null;

    /// <summary>Gets the objective value when a solution is available.</summary>
    public double? ObjectiveValue => Solution?.TotalCost;

    private static void ValidateStatusAndSolution(
        UlsSolveStatus status,
        UlsSolution? solution)
    {
        if ((status is UlsSolveStatus.Optimal or
             UlsSolveStatus.Feasible) &&
            solution is null)
        {
            throw new ArgumentException(
                $"Status '{status}' requires a feasible solution.",
                nameof(solution));
        }

        if ((status is UlsSolveStatus.Infeasible or
             UlsSolveStatus.NotSolved or
             UlsSolveStatus.Failed) &&
            solution is not null)
        {
            throw new ArgumentException(
                $"Status '{status}' cannot contain a feasible solution.",
                nameof(solution));
        }
    }
}
