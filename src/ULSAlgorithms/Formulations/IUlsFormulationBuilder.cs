using ULSAlgorithms.Models;

namespace ULSAlgorithms.Formulations;

/// <summary>
/// Builds a solver-independent mathematical-programming formulation of ULS.
/// </summary>
public interface IUlsFormulationBuilder
{
    /// <summary>Gets the stable formulation name.</summary>
    string Name { get; }

    /// <summary>Gets the formulation kind.</summary>
    UlsFormulationKind Kind { get; }

    /// <summary>Tests whether the formulation is valid for the supplied costs.</summary>
    bool IsApplicable(UlsProblem problem);

    /// <summary>Builds the portable formulation.</summary>
    UlsFormulation Build(UlsProblem problem);
}
