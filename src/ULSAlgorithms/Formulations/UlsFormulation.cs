using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Formulations;

/// <summary>
/// Solver-independent ULS mathematical formulation plus semantic variable map.
/// </summary>
public sealed class UlsFormulation
{
    /// <summary>Initializes a formulation result.</summary>
    public UlsFormulation(
        UlsFormulationKind kind,
        string source,
        LinearModel model,
        UlsFormulationVariableMap variables)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException(
                "A scientific/source description is required.",
                nameof(source));
        }

        Kind = kind;
        Source = source.Trim();
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Variables =
            variables ??
            throw new ArgumentNullException(nameof(variables));
    }

    /// <summary>Gets the formulation kind.</summary>
    public UlsFormulationKind Kind { get; }

    /// <summary>Gets the scientific/source description.</summary>
    public string Source { get; }

    /// <summary>Gets the portable model.</summary>
    public LinearModel Model { get; }

    /// <summary>Gets semantic variable mappings.</summary>
    public UlsFormulationVariableMap Variables { get; }
}
