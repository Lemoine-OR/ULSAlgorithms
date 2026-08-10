using ULSAlgorithms.Abstractions;

namespace ULSAlgorithms.Catalog;

/// <summary>
/// Immutable metadata and construction entry for one public ULS strategy.
/// </summary>
public sealed class UlsSolverDescriptor
{
    private readonly Func<IUlsSolver> _factory;
    private readonly Func<UlsSolverCreationOptions, IUlsSolver>? _configuredFactory;

    internal UlsSolverDescriptor(
        string id,
        string name,
        UlsSolverCategory category,
        string family,
        string timeComplexity,
        string spaceComplexity,
        string applicability,
        string scientificReference,
        string doi,
        string implementation,
        string sourcePath,
        Type implementationType,
        Func<IUlsSolver> factory,
        UlsSolverConfigurationCapabilities configurationCapabilities =
            UlsSolverConfigurationCapabilities.None,
        Func<UlsSolverCreationOptions, IUlsSolver>? configuredFactory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(family);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeComplexity);
        ArgumentException.ThrowIfNullOrWhiteSpace(spaceComplexity);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicability);
        ArgumentException.ThrowIfNullOrWhiteSpace(scientificReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(implementation);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentNullException.ThrowIfNull(implementationType);
        ArgumentNullException.ThrowIfNull(factory);

        ValidateStableId(id);
        ValidateConfigurationFactory(
            configurationCapabilities,
            configuredFactory);

        if (!typeof(IUlsSolver).IsAssignableFrom(implementationType))
        {
            throw new ArgumentException(
                $"Type '{implementationType.FullName}' does not implement IUlsSolver.",
                nameof(implementationType));
        }

        Id = id;
        Name = name;
        Category = category;
        Kind = category == UlsSolverCategory.Heuristic
            ? UlsSolverKind.Heuristic
            : UlsSolverKind.Exact;
        Family = family;
        TimeComplexity = timeComplexity;
        SpaceComplexity = spaceComplexity;
        Applicability = applicability;
        ScientificReference = scientificReference;
        Doi = doi ?? string.Empty;
        Implementation = implementation;
        SourcePath = sourcePath;
        ImplementationType = implementationType;
        RequiresExternalSolver =
            category is UlsSolverCategory.OptimizationFormulation
                or UlsSolverCategory.CuttingPlane;
        ConfigurationCapabilities = configurationCapabilities;
        _factory = factory;
        _configuredFactory = configuredFactory;
    }

    /// <summary>
    /// Gets the stable lower-kebab-case identifier used by
    /// <see cref="UlsSolverFactory"/>.
    /// </summary>
    public string Id { get; }

    /// <summary>Gets the human-readable strategy name.</summary>
    public string Name { get; }

    /// <summary>Gets the exact/heuristic strategy kind.</summary>
    public UlsSolverKind Kind { get; }

    /// <summary>Gets the operational strategy category.</summary>
    public UlsSolverCategory Category { get; }

    /// <summary>Gets the literature/implementation family.</summary>
    public string Family { get; }

    /// <summary>Gets the documented asymptotic time complexity.</summary>
    public string TimeComplexity { get; }

    /// <summary>Gets the documented auxiliary/model memory complexity.</summary>
    public string SpaceComplexity { get; }

    /// <summary>Gets the documented applicability conditions.</summary>
    public string Applicability { get; }

    /// <summary>
    /// Gets whether solving requires an external mathematical-programming engine.
    /// </summary>
    public bool RequiresExternalSolver { get; }

    /// <summary>Gets the primary scientific or historical reference.</summary>
    public string ScientificReference { get; }

    /// <summary>Gets the primary DOI when one is recorded, otherwise an empty string.</summary>
    public string Doi { get; }

    /// <summary>Gets the implementation note exposed by the public catalog.</summary>
    public string Implementation { get; }

    /// <summary>Gets the repository-relative implementation source path.</summary>
    public string SourcePath { get; }

    /// <summary>Gets the concrete public strategy type.</summary>
    public Type ImplementationType { get; }

    /// <summary>
    /// Gets the constructor-level settings supported by the configurable
    /// factory for this strategy.
    /// </summary>
    public UlsSolverConfigurationCapabilities ConfigurationCapabilities { get; }

    /// <summary>
    /// Gets whether this strategy exposes at least one configurable factory
    /// setting.
    /// </summary>
    public bool SupportsConfiguration =>
        ConfigurationCapabilities !=
        UlsSolverConfigurationCapabilities.None;

    /// <summary>
    /// Creates a new solver instance using the strategy's default constructor
    /// policy.
    /// </summary>
    /// <returns>A fresh public solver instance.</returns>
    public IUlsSolver Create() =>
        ValidateCreatedSolver(
            _factory());

    /// <summary>
    /// Creates a new solver instance using explicit constructor-level
    /// configuration.
    /// </summary>
    /// <param name="options">Composed factory options.</param>
    /// <returns>A fresh configured solver instance.</returns>
    /// <remarks>
    /// An empty option set is equivalent to <see cref="Create()"/>.
    /// Unsupported non-empty options are rejected.
    /// </remarks>
    public IUlsSolver Create(UlsSolverCreationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.IsEmpty)
        {
            return Create();
        }

        options.EnsureValidFor(this);

        if (_configuredFactory is null)
        {
            throw new InvalidOperationException(
                $"Solver '{Id}' does not expose configurable construction.");
        }

        return ValidateCreatedSolver(
            _configuredFactory(options));
    }

    private IUlsSolver ValidateCreatedSolver(IUlsSolver solver)
    {
        ArgumentNullException.ThrowIfNull(solver);

        if (solver.GetType() != ImplementationType)
        {
            throw new InvalidOperationException(
                $"Catalog factory for '{Id}' returned '{solver.GetType().FullName}' " +
                $"instead of '{ImplementationType.FullName}'.");
        }

        if (solver.Kind != Kind)
        {
            throw new InvalidOperationException(
                $"Catalog kind mismatch for '{Id}'. Expected {Kind}, got {solver.Kind}.");
        }

        return solver;
    }

    private static void ValidateConfigurationFactory(
        UlsSolverConfigurationCapabilities capabilities,
        Func<UlsSolverCreationOptions, IUlsSolver>? configuredFactory)
    {
        if (capabilities == UlsSolverConfigurationCapabilities.None &&
            configuredFactory is not null)
        {
            throw new ArgumentException(
                "A configured factory requires at least one configuration capability.",
                nameof(configuredFactory));
        }

        if (capabilities != UlsSolverConfigurationCapabilities.None &&
            configuredFactory is null)
        {
            throw new ArgumentException(
                "Configuration capabilities require a configured factory.",
                nameof(configuredFactory));
        }
    }

    private static void ValidateStableId(string id)
    {
        if (id[0] == '-' || id[^1] == '-')
        {
            throw new ArgumentException(
                "A solver identifier cannot start or end with '-'.",
                nameof(id));
        }

        var previousHyphen = false;

        foreach (var character in id)
        {
            var valid =
                character is >= 'a' and <= 'z' ||
                character is >= '0' and <= '9' ||
                character == '-';

            if (!valid || (character == '-' && previousHyphen))
            {
                throw new ArgumentException(
                    "Solver identifiers must use normalized lower-kebab-case ASCII.",
                    nameof(id));
            }

            previousHyphen = character == '-';
        }
    }
}
