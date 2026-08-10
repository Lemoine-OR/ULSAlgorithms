namespace ULSAlgorithms.Formulations;

/// <summary>
/// Identifies a mathematical-programming formulation of classical ULS.
/// </summary>
/// <remarks>
/// The taxonomy follows the four classical formulations summarized by
/// Brahimi, Dauzère-Pérès, Najid and Nordli (2006):
/// aggregate, disaggregate/facility-location, shortest-path, and a formulation
/// obtained by eliminating inventory variables.
/// </remarks>
public enum UlsFormulationKind
{
    /// <summary>Classical aggregate inventory-balance big-M formulation.</summary>
    AggregateInventory = 0,

    /// <summary>Disaggregated facility-location formulation.</summary>
    FacilityLocation = 1,

    /// <summary>Regeneration-interval shortest-path formulation.</summary>
    ShortestPath = 2,

    /// <summary>Aggregate formulation with inventory variables eliminated.</summary>
    InventoryEliminated = 3
}
