using ULSAlgorithms.Optimization.Execution.Providers;

namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// Creates the built-in CPLEX, Gurobi, Xpress and CBC model executors.
/// </summary>
public static class DefaultLinearModelExecutorRegistry
{
    /// <summary>Creates all built-in execution backends.</summary>
    public static LinearModelExecutorRegistry Create()
    {
        var registry =
            new LinearModelExecutorRegistry();

        registry.Register(
            new CplexLinearModelExecutor());

        registry.Register(
            new GurobiLinearModelExecutor());

        registry.Register(
            new XpressLinearModelExecutor());

        registry.Register(
            new CoinOrCbcLinearModelExecutor());

        return registry;
    }
}
