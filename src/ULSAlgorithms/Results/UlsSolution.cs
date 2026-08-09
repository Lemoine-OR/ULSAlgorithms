namespace ULSAlgorithms.Results;

/// <summary>
/// Represents a feasible production plan for a ULS problem.
/// </summary>
public sealed class UlsSolution
{
    private readonly double[] _productionQuantities;
    private readonly double[] _endingInventories;
    private readonly bool[] _setupDecisions;

    /// <summary>
    /// Initializes a solution by defensively copying all supplied decision vectors.
    /// </summary>
    /// <param name="productionQuantities">Production quantity in each period.</param>
    /// <param name="endingInventories">Inventory remaining at the end of each period.</param>
    /// <param name="setupDecisions">Whether production is set up in each period.</param>
    /// <param name="setupCost">Total fixed setup cost.</param>
    /// <param name="productionCost">Total variable production cost.</param>
    /// <param name="holdingCost">Total inventory holding cost.</param>
    public UlsSolution(
        ReadOnlySpan<double> productionQuantities,
        ReadOnlySpan<double> endingInventories,
        ReadOnlySpan<bool> setupDecisions,
        double setupCost,
        double productionCost,
        double holdingCost)
        : this(
            productionQuantities.ToArray(),
            endingInventories.ToArray(),
            setupDecisions.ToArray(),
            setupCost,
            productionCost,
            holdingCost,
            takeOwnership: true)
    {
    }

    private UlsSolution(
        double[] productionQuantities,
        double[] endingInventories,
        bool[] setupDecisions,
        double setupCost,
        double productionCost,
        double holdingCost,
        bool takeOwnership)
    {
        ArgumentNullException.ThrowIfNull(productionQuantities);
        ArgumentNullException.ThrowIfNull(endingInventories);
        ArgumentNullException.ThrowIfNull(setupDecisions);

        if (productionQuantities.Length == 0)
        {
            throw new ArgumentException(
                "A ULS solution must contain at least one period.",
                nameof(productionQuantities));
        }

        if (endingInventories.Length != productionQuantities.Length)
        {
            throw new ArgumentException(
                "Ending-inventory and production vectors must have the same length.",
                nameof(endingInventories));
        }

        if (setupDecisions.Length != productionQuantities.Length)
        {
            throw new ArgumentException(
                "Setup-decision and production vectors must have the same length.",
                nameof(setupDecisions));
        }

        ValidateNonNegativeFiniteVector(productionQuantities, nameof(productionQuantities));
        ValidateNonNegativeFiniteVector(endingInventories, nameof(endingInventories));
        ValidateCost(setupCost, nameof(setupCost));
        ValidateCost(productionCost, nameof(productionCost));
        ValidateCost(holdingCost, nameof(holdingCost));

        _productionQuantities = takeOwnership
            ? productionQuantities
            : (double[])productionQuantities.Clone();

        _endingInventories = takeOwnership
            ? endingInventories
            : (double[])endingInventories.Clone();

        _setupDecisions = takeOwnership
            ? setupDecisions
            : (bool[])setupDecisions.Clone();

        SetupCost = setupCost;
        ProductionCost = productionCost;
        HoldingCost = holdingCost;
        TotalCost = setupCost + productionCost + holdingCost;
    }

    /// <summary>
    /// Gets the number of periods represented by the solution.
    /// </summary>
    public int Horizon => _productionQuantities.Length;

    /// <summary>
    /// Gets production quantities by period.
    /// </summary>
    public ReadOnlySpan<double> ProductionQuantities => _productionQuantities;

    /// <summary>
    /// Gets end-of-period inventories by period.
    /// </summary>
    public ReadOnlySpan<double> EndingInventories => _endingInventories;

    /// <summary>
    /// Gets setup decisions by period.
    /// </summary>
    public ReadOnlySpan<bool> SetupDecisions => _setupDecisions;

    /// <summary>
    /// Gets the total fixed setup cost.
    /// </summary>
    public double SetupCost { get; }

    /// <summary>
    /// Gets the total variable production cost.
    /// </summary>
    public double ProductionCost { get; }

    /// <summary>
    /// Gets the total holding cost.
    /// </summary>
    public double HoldingCost { get; }

    /// <summary>
    /// Gets the complete objective value.
    /// </summary>
    public double TotalCost { get; }

    /// <summary>
    /// Creates a solution while transferring ownership of already allocated solver buffers.
    /// </summary>
    /// <remarks>
    /// This internal fast path prevents an unnecessary second copy when an algorithm has
    /// already produced dedicated output arrays.
    /// </remarks>
    internal static UlsSolution FromOwnedBuffers(
        double[] productionQuantities,
        double[] endingInventories,
        bool[] setupDecisions,
        double setupCost,
        double productionCost,
        double holdingCost)
    {
        return new UlsSolution(
            productionQuantities,
            endingInventories,
            setupDecisions,
            setupCost,
            productionCost,
            holdingCost,
            takeOwnership: true);
    }

    private static void ValidateNonNegativeFiniteVector(
        ReadOnlySpan<double> values,
        string parameterName)
    {
        for (var period = 0; period < values.Length; period++)
        {
            var value = values[period];

            if (!double.IsFinite(value) || value < 0.0)
            {
                throw new ArgumentException(
                    $"Vector '{parameterName}' contains an invalid value at period {period}: {value}.",
                    parameterName);
            }
        }
    }

    private static void ValidateCost(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Solution cost components must be finite and non-negative.");
        }
    }
}
