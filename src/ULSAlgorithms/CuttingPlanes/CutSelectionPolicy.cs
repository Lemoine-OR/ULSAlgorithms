namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Selects which eligible violated cuts are inserted at one root-separation
/// iteration.
/// </summary>
public enum CutSelectionPolicy
{
    /// <summary>Add every unique violated cut.</summary>
    AllViolated = 0,

    /// <summary>
    /// Add only the most violated candidate for each value of l.
    /// </summary>
    MostViolatedPerL = 1,

    /// <summary>
    /// Add the globally most violated candidates, limited by
    /// MaximumCutsPerIteration.
    /// </summary>
    TopByViolation = 2,

    /// <summary>
    /// Add the globally highest-efficacy candidates, limited by
    /// MaximumCutsPerIteration.
    /// </summary>
    TopByEfficacy = 3
}
