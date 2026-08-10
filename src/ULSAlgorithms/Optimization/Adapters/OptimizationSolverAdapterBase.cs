namespace ULSAlgorithms.Optimization.Adapters;

/// <summary>
/// Base class for concrete optimization-solver availability adapters.
/// </summary>
public abstract class OptimizationSolverAdapterBase :
    IOptimizationSolverAdapter
{
    private readonly SolverCapability[] _capabilities;

    /// <summary>
    /// Initializes an adapter with its supported capabilities.
    /// </summary>
    protected OptimizationSolverAdapterBase(
        params SolverCapability[] capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);

        if (capabilities.Any(
                static capability =>
                    capability == SolverCapability.Unknown))
        {
            throw new ArgumentException(
                "Adapter capabilities cannot contain Unknown.",
                nameof(capabilities));
        }

        if (capabilities.Distinct().Count() != capabilities.Length)
        {
            throw new ArgumentException(
                "Adapter capabilities cannot contain duplicates.",
                nameof(capabilities));
        }

        _capabilities = capabilities.ToArray();
    }

    /// <inheritdoc />
    public abstract string AdapterId { get; }

    /// <inheritdoc />
    public abstract string AdapterName { get; }

    /// <inheritdoc />
    public virtual string AdapterVersion => "1.0.0";

    /// <inheritdoc />
    public abstract SolverKind SolverKind { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<SolverCapability> Capabilities =>
        _capabilities;

    /// <inheritdoc />
    public bool SupportsCapability(
        SolverCapability capability) =>
        _capabilities.Contains(capability);

    /// <inheritdoc />
    public abstract ValueTask<SolverAvailabilityInfo>
        CheckAvailabilityAsync(
            CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps an exception to a stable availability state.
    /// </summary>
    protected static SolverAvailabilityStatus ClassifyFailure(
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        string message =
            exception.ToString();

        if (message.Contains(
                "license",
                StringComparison.OrdinalIgnoreCase) ||
            message.Contains(
                "licence",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverAvailabilityStatus.LicenseUnavailable;
        }

        if (exception is FileNotFoundException or
            DirectoryNotFoundException)
        {
            return SolverAvailabilityStatus.LibrariesMissing;
        }

        return SolverAvailabilityStatus.LoadFailure;
    }

    /// <summary>
    /// Unwraps reflection invocation exceptions.
    /// </summary>
    protected static Exception Unwrap(
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is
                System.Reflection.TargetInvocationException invocation &&
               invocation.InnerException is Exception inner
            ? inner
            : exception;
    }
}
