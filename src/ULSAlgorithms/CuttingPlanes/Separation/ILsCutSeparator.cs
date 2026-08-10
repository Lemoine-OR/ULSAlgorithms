using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.Formulations;
using ULSAlgorithms.Models;

namespace ULSAlgorithms.CuttingPlanes.Separation;

/// <summary>
/// Separates classical ULS (l,S) inequalities from a fractional aggregate
/// lot-sizing solution.
/// </summary>
public interface ILsCutSeparator
{
    /// <summary>Gets the stable separator name.</summary>
    string Name { get; }

    /// <summary>Gets the traceability method identifier.</summary>
    CutSeparationMethod Method { get; }

    /// <summary>Tests whether the separator is applicable to the problem.</summary>
    bool IsApplicable(
        UlsProblem problem);

    /// <summary>
    /// Generates the separator's candidate inequalities at one LP point.
    /// </summary>
    IReadOnlyList<LsSeparatedCut> Separate(
        UlsProblem problem,
        UlsFormulation formulation,
        IReadOnlyDictionary<int, double> variableValues);
}
