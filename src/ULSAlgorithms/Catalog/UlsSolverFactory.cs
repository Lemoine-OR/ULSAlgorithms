using ULSAlgorithms.Abstractions;

namespace ULSAlgorithms.Catalog;

/// <summary>
/// Creates public ULS strategies from stable catalog identifiers.
/// </summary>
public static class UlsSolverFactory
{
    /// <summary>
    /// Creates a new solver using its stable catalog identifier.
    /// </summary>
    /// <param name="id">Stable lower-kebab-case strategy identifier.</param>
    /// <returns>A fresh solver instance.</returns>
    public static IUlsSolver Create(string id) =>
        UlsSolverCatalog.Get(id).Create();

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

        if (descriptor is null)
        {
            solver = null;
            return false;
        }

        solver = descriptor.Create();
        return true;
    }
}
