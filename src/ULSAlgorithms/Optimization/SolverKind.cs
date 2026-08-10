namespace ULSAlgorithms.Optimization;

/// <summary>
/// Identifies a mathematical optimization solver that can be used by
/// solver-backed ULS algorithms.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Automatic"/> follows the same default priority as
/// LotSizingDataModel:
/// CPLEX, Gurobi, FICO Xpress, then COIN-OR CBC.
/// </para>
/// </remarks>
public enum SolverKind
{
    /// <summary>No solver has been specified.</summary>
    Unknown = 0,

    /// <summary>Select the first usable solver according to the configured priority.</summary>
    Automatic = 1,

    /// <summary>IBM ILOG CPLEX Optimization Studio.</summary>
    Cplex = 2,

    /// <summary>Gurobi Optimizer.</summary>
    Gurobi = 3,

    /// <summary>FICO Xpress Optimizer.</summary>
    Xpress = 4,

    /// <summary>COIN-OR CBC mixed-integer programming solver.</summary>
    CoinOrCbc = 5
}
