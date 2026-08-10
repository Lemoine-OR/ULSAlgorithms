using ULSAlgorithms.Formulations.Aggregate;
using ULSAlgorithms.Formulations.FacilityLocation;
using ULSAlgorithms.Formulations.InventoryEliminated;
using ULSAlgorithms.Formulations.ShortestPath;

namespace ULSAlgorithms.Formulations;

/// <summary>
/// Creates the built-in classical ULS mathematical formulations.
/// </summary>
public static class UlsFormulationCatalog
{
    /// <summary>
    /// Creates all four classical formulation builders in taxonomy order.
    /// </summary>
    public static IReadOnlyList<IUlsFormulationBuilder>
        CreateAll()
    {
        return
        [
            new AggregateInventoryFormulationBuilder(),
            new FacilityLocationFormulationBuilder(),
            new ShortestPathFormulationBuilder(),
            new InventoryEliminatedFormulationBuilder()
        ];
    }
}
