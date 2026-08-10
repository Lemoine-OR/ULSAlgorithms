namespace ULSAlgorithms.Optimization;

/// <summary>
/// Describes whether a solver can currently be used on the current computer.
/// </summary>
public enum SolverAvailabilityStatus
{
    /// <summary>Availability has not been checked.</summary>
    Unknown = 0,

    /// <summary>The solver is not installed or cannot be located.</summary>
    NotInstalled = 1,

    /// <summary>Required managed or native libraries are missing.</summary>
    LibrariesMissing = 2,

    /// <summary>Solver libraries were found but could not be loaded.</summary>
    LoadFailure = 3,

    /// <summary>The solver is installed but no usable license is available.</summary>
    LicenseUnavailable = 4,

    /// <summary>The solver is usable with functional or licensing limitations.</summary>
    AvailableWithLimitations = 5,

    /// <summary>The solver is installed, loadable and usable.</summary>
    Available = 6
}
