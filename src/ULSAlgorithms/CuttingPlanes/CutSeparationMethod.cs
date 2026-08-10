namespace ULSAlgorithms.CuttingPlanes;

/// <summary>Identifies the separation procedure that generated a cut.</summary>
public enum CutSeparationMethod
{
    /// <summary>No separation procedure has been specified.</summary>
    Unknown = 0,

    /// <summary>Wagner-Whitin-structure separation for (l,S) inequalities.</summary>
    WagnerWhitin = 1,

    /// <summary>General separation for (l,S) inequalities.</summary>
    General = 2
}
