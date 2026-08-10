using ULSAlgorithms.Abstractions;

namespace ULSAlgorithms.Catalog;

/// <summary>
/// Creates public ULS strategies from stable catalog identifiers.
/// </summary>
public static class UlsSolverFactory
{
    /// <summary>
    /// Creates a new solver using its stable catalog identifier and default
    /// constructor policy.
    /// </summary>
    /// <param name="id">Stable lower-kebab-case strategy identifier.</param>
    /// <returns>A fresh solver instance.</returns>
    public static IUlsSolver Create(string id) =>
        UlsSolverCatalog.Get(id).Create();

    /// <summary>
    /// Creates a new solver using its stable catalog identifier and explicit
    /// constructor-level configuration.
    /// </summary>
    /// <param name="id">Stable strategy identifier.</param>
    /// <param name="options">Composed factory options.</param>
    /// <returns>A fresh configured solver instance.</returns>
    public static IUlsSolver Create(
        string id,
        UlsSolverCreationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return UlsSolverCatalog
            .Get(id)
            .Create(options);
    }

    /// <summary>
    /// Attempts to create a solver using its stable catalog identifier.
    /// </summary>
    /// <param name="id">Stable identifier.</param>
    /// <param name="solver">Created solver, or null when the identifier is unknown.</param>
    /// <returns>True when a solver was created.</returns>
    public static bool TryCreate(
        string? id,
        out IUlsSolver? solver)
    {
        if (!UlsSolverCatalog.TryGet(id, out var descriptor))
        {
            solver = null;
            return false;
        }

        solver = descriptor.Create();
        return true;
    }

    /// <summary>
    /// Attempts to create a configured solver using its stable catalog
    /// identifier.
    /// </summary>
    /// <param name="id">Stable identifier.</param>
    /// <param name="options">Composed factory options.</param>
    /// <param name="solver">Created solver, or null when the identifier is unknown.</param>
    /// <returns>True when a solver was created.</returns>
    /// <remarks>
    /// Unknown identifiers return false. Invalid or unsupported options for a
    /// known identifier are programming/configuration errors and therefore
    /// still throw.
    /// </remarks>
    public static bool TryCreate(
        string? id,
        UlsSolverCreationOptions options,
        out IUlsSolver? solver)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!UlsSolverCatalog.TryGet(id, out var descriptor))
        {
            solver = null;
            return false;
        }

        solver = descriptor.Create(options);
        return true;
    }
}
