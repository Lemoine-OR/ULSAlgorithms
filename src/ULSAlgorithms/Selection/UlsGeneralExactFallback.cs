namespace ULSAlgorithms.Selection;

/// <summary>
/// Selects the general exact algorithm used when the linear-time
/// Wagner-Whitin specialization is not applicable.
/// </summary>
public enum UlsGeneralExactFallback
{
    /// <summary>
    /// Uses the backward geometric Wagelmans-van Hoesel-Kolen algorithm.
    /// </summary>
    WagelmansGeneral = 0,

    /// <summary>
    /// Uses the forward Federgruen-Tzur algorithm.
    /// </summary>
    FedergruenTzurGeneral = 1
}
