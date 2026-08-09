namespace ULSAlgorithms.Models;

/// <summary>
/// Represents a validated classical uncapacitated lot-sizing problem.
/// </summary>
/// <remarks>
/// <para>
/// Periods are zero-based in the API: <c>0, ..., Horizon - 1</c>.
/// Demand must be satisfied without backlogging, capacity is unlimited, and
/// initial inventory is zero.
/// </para>
/// <para>
/// <see cref="HoldingCosts"/> contains the cost of holding one unit in
/// end-of-period inventory. A standard zero-ending-inventory solution therefore
/// does not use the last holding-cost entry.
/// </para>
/// <para>
/// Input vectors are copied once at construction. Solvers then access contiguous
/// read-only spans without per-period allocations.
/// </para>
/// </remarks>
public sealed class UlsProblem
{
    private readonly double[] _demands;
    private readonly double[] _setupCosts;
    private readonly double[] _unitProductionCosts;
    private readonly double[] _holdingCosts;

    /// <summary>
    /// Initializes a new validated ULS problem.
    /// </summary>
    /// <param name="demands">Demand in each period.</param>
    /// <param name="setupCosts">Fixed setup cost in each period.</param>
    /// <param name="unitProductionCosts">Unit production cost in each period.</param>
    /// <param name="holdingCosts">
    /// Unit cost of holding one unit of end-of-period inventory in each period.
    /// </param>
    public UlsProblem(
        ReadOnlySpan<double> demands,
        ReadOnlySpan<double> setupCosts,
        ReadOnlySpan<double> unitProductionCosts,
        ReadOnlySpan<double> holdingCosts)
    {
        UlsProblemValidator.Validate(
            demands,
            setupCosts,
            unitProductionCosts,
            holdingCosts);

        _demands = demands.ToArray();
        _setupCosts = setupCosts.ToArray();
        _unitProductionCosts = unitProductionCosts.ToArray();
        _holdingCosts = holdingCosts.ToArray();

        var totalDemand = 0.0;
        for (var period = 0; period < _demands.Length; period++)
        {
            totalDemand += _demands[period];
        }

        TotalDemand = totalDemand;
    }

    /// <summary>
    /// Gets the number of planning periods.
    /// </summary>
    public int Horizon => _demands.Length;

    /// <summary>
    /// Gets the total demand over the complete planning horizon.
    /// </summary>
    public double TotalDemand { get; }

    /// <summary>
    /// Gets demand by period.
    /// </summary>
    public ReadOnlySpan<double> Demands => _demands;

    /// <summary>
    /// Gets fixed setup costs by period.
    /// </summary>
    public ReadOnlySpan<double> SetupCosts => _setupCosts;

    /// <summary>
    /// Gets unit production costs by period.
    /// </summary>
    public ReadOnlySpan<double> UnitProductionCosts => _unitProductionCosts;

    /// <summary>
    /// Gets end-of-period unit holding costs by period.
    /// </summary>
    public ReadOnlySpan<double> HoldingCosts => _holdingCosts;
}
