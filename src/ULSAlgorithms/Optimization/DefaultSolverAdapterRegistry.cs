using ULSAlgorithms.Optimization.Adapters.CoinOrCbc;
using ULSAlgorithms.Optimization.Adapters.Cplex;
using ULSAlgorithms.Optimization.Adapters.Gurobi;
using ULSAlgorithms.Optimization.Adapters.Xpress;

namespace ULSAlgorithms.Optimization;

/// <summary>
/// Creates the built-in concrete solver-adapter registry.
/// </summary>
public static class DefaultSolverAdapterRegistry
{
    /// <summary>
    /// Creates a registry containing CPLEX, Gurobi, Xpress and CBC adapters.
    /// </summary>
    /// <remarks>
    /// Registration order deliberately matches the default automatic solver
    /// priority used by LotSizingDataModel.
    /// </remarks>
    public static SolverAdapterRegistry Create()
    {
        var registry =
            new SolverAdapterRegistry();

        registry.Register(
            new CplexSolverAdapter());

        registry.Register(
            new GurobiSolverAdapter());

        registry.Register(
            new XpressSolverAdapter());

        registry.Register(
            new CoinOrCbcSolverAdapter());

        return registry;
    }
}
