using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Selection;

namespace ULSAlgorithms.Catalog;

/// <summary>
/// Composes the existing strategy-specific constructor options used by
/// <see cref="UlsSolverFactory"/>.
/// </summary>
/// <remarks>
/// <para>
/// Every property is optional. An empty instance is equivalent to the
/// historical parameterless factory path.
/// </para>
/// <para>
/// Configuration is strict: supplying a setting that is not supported by the
/// selected strategy throws <see cref="ArgumentException"/> rather than being
/// silently ignored.
/// </para>
/// <para>
/// Existing option objects are reused directly. Solver-backed strategies clone
/// their execution/cutting-plane options during construction, preserving their
/// existing ownership semantics.
/// </para>
/// </remarks>
public sealed class UlsSolverCreationOptions
{
    /// <summary>
    /// Gets or sets the general exact fallback used by
    /// <c>adaptive-exact</c>. Null preserves the default Wagelmans fallback.
    /// </summary>
    public UlsGeneralExactFallback? AdaptiveGeneralFallback { get; set; }

    /// <summary>
    /// Gets or sets the maximum worker count for a strategy exposing the
    /// parallelism capability. Null preserves that strategy's default.
    /// </summary>
    public int? MaxDegreeOfParallelism { get; set; }

    /// <summary>
    /// Gets or sets the minimum candidate count before parallel evaluation is
    /// attempted. Null preserves the strategy default.
    /// </summary>
    public int? ParallelThreshold { get; set; }

    /// <summary>
    /// Gets or sets solver-backed execution options, including the requested
    /// optimization engine and numerical/file-management settings.
    /// </summary>
    public LinearModelSolveOptions? OptimizationExecution { get; set; }

    /// <summary>
    /// Gets or sets root (l,S) cutting-plane engineering options.
    /// </summary>
    public LsCuttingPlaneOptions? CuttingPlane { get; set; }

    internal bool IsEmpty =>
        AdaptiveGeneralFallback is null &&
        MaxDegreeOfParallelism is null &&
        ParallelThreshold is null &&
        OptimizationExecution is null &&
        CuttingPlane is null;

    internal void EnsureValidFor(UlsSolverDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var capabilities = descriptor.ConfigurationCapabilities;

        if (AdaptiveGeneralFallback.HasValue)
        {
            RequireCapability(
                descriptor,
                capabilities,
                UlsSolverConfigurationCapabilities.AdaptiveGeneralFallback,
                nameof(AdaptiveGeneralFallback));

            if (!Enum.IsDefined(
                    typeof(UlsGeneralExactFallback),
                    AdaptiveGeneralFallback.Value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(AdaptiveGeneralFallback),
                    AdaptiveGeneralFallback.Value,
                    "Unknown adaptive general exact fallback.");
            }
        }

        if (MaxDegreeOfParallelism.HasValue ||
            ParallelThreshold.HasValue)
        {
            RequireCapability(
                descriptor,
                capabilities,
                UlsSolverConfigurationCapabilities.Parallelism,
                "parallelism");

            if (MaxDegreeOfParallelism.HasValue &&
                (MaxDegreeOfParallelism.Value == 0 ||
                 MaxDegreeOfParallelism.Value < -1))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(MaxDegreeOfParallelism),
                    MaxDegreeOfParallelism.Value,
                    "MaxDegreeOfParallelism must be -1 or strictly positive.");
            }

            if (ParallelThreshold.HasValue &&
                ParallelThreshold.Value < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(ParallelThreshold),
                    ParallelThreshold.Value,
                    "ParallelThreshold must be strictly positive.");
            }
        }

        if (OptimizationExecution is not null)
        {
            RequireCapability(
                descriptor,
                capabilities,
                UlsSolverConfigurationCapabilities.OptimizationExecution,
                nameof(OptimizationExecution));

            OptimizationExecution.EnsureValid();
        }

        if (CuttingPlane is not null)
        {
            RequireCapability(
                descriptor,
                capabilities,
                UlsSolverConfigurationCapabilities.CuttingPlane,
                nameof(CuttingPlane));

            CuttingPlane.EnsureValid();
        }
    }

    private static void RequireCapability(
        UlsSolverDescriptor descriptor,
        UlsSolverConfigurationCapabilities actual,
        UlsSolverConfigurationCapabilities required,
        string optionName)
    {
        if ((actual & required) == required)
        {
            return;
        }

        throw new ArgumentException(
            $"Configuration option '{optionName}' is not supported by " +
            $"solver '{descriptor.Id}'.",
            optionName);
    }
}
