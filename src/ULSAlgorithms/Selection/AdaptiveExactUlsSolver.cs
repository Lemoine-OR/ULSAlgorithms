using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.FedergruenTzur;
using ULSAlgorithms.Exact.Wagelmans;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Selection;

/// <summary>
/// Selects and executes an efficient exact ULS algorithm from problem
/// characteristics while preserving the common <see cref="IUlsSolver"/> contract.
/// </summary>
/// <remarks>
/// <para>
/// When <c>p[t] + h[t] &gt;= p[t+1]</c> for every adjacent period, the selector
/// uses <see cref="WagnerWhitinSolver"/>, i.e. the linear-time specialization of
/// Wagelmans, van Hoesel and Kolen (1992). Otherwise it uses a configurable
/// general <c>O(n log n)</c> exact algorithm.
/// </para>
/// <para>
/// The no-speculative-motive condition is cached by the immutable
/// <see cref="UlsProblem"/> during construction. Adaptive selection therefore
/// adds no extra <c>O(n)</c> applicability scan before invoking the selected
/// exact solver.
/// </para>
/// <para>
/// The default general fallback is <see cref="WagelmansGeneralSolver"/>. The
/// alternative <see cref="FedergruenTzurSolver"/> remains selectable explicitly
/// for reproducible research and benchmarking. The v0.24 calibration campaign
/// recorded in the v0.25 engineering notes found no practical crossover on the
/// measured horizons, so no empirical threshold is introduced.
/// </para>
/// <para>
/// References: A. Wagelmans, S. van Hoesel and A. Kolen, Operations Research
/// 40(S1), S145-S156, 1992, DOI: 10.1287/opre.40.1.S145; A. Federgruen and
/// M. Tzur, Management Science 37(8), 909-925, 1991,
/// DOI: 10.1287/mnsc.37.8.909.
/// </para>
/// </remarks>
public sealed class AdaptiveExactUlsSolver : IUlsSolver
{
    private readonly WagnerWhitinSolver _linearSolver = new();
    private readonly IUlsSolver _generalSolver;

    /// <summary>
    /// Initializes a selector using the Wagelmans general algorithm as fallback.
    /// </summary>
    public AdaptiveExactUlsSolver()
        : this(UlsGeneralExactFallback.WagelmansGeneral)
    {
    }

    /// <summary>
    /// Initializes a selector with an explicit general exact fallback.
    /// </summary>
    /// <param name="fallback">General exact algorithm to use outside the NSM case.</param>
    public AdaptiveExactUlsSolver(UlsGeneralExactFallback fallback)
    {
        Fallback = fallback;
        _generalSolver = fallback switch
        {
            UlsGeneralExactFallback.WagelmansGeneral =>
                new WagelmansGeneralSolver(),
            UlsGeneralExactFallback.FedergruenTzurGeneral =>
                new FedergruenTzurSolver(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(fallback),
                fallback,
                "Unknown general exact fallback.")
        };
    }

    /// <inheritdoc />
    public string Name => "Adaptive exact ULS solver";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <summary>
    /// Gets the general exact fallback configured for this selector.
    /// </summary>
    public UlsGeneralExactFallback Fallback { get; }

    /// <summary>
    /// Selects the exact solver to use for the supplied problem.
    /// </summary>
    /// <param name="problem">The validated ULS problem.</param>
    /// <returns>The selected exact strategy.</returns>
    /// <remarks>
    /// The applicability decision reuses the immutable profile cached by
    /// <see cref="UlsProblem"/> and therefore does not rescan the horizon.
    /// </remarks>
    public IUlsSolver SelectSolver(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        return problem.HasNoSpeculativeMotiveCosts
            ? _linearSolver
            : _generalSolver;
    }

    /// <summary>
    /// Selects a solver from already-computed problem characteristics.
    /// </summary>
    /// <param name="problem">The validated ULS problem.</param>
    /// <param name="characteristics">Characteristics computed for this problem.</param>
    /// <returns>The selected exact strategy.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the supplied characteristics are inconsistent with the
    /// problem horizon or total demand.
    /// </exception>
    public IUlsSolver SelectSolver(
        UlsProblem problem,
        in UlsProblemCharacteristics characteristics)
    {
        ArgumentNullException.ThrowIfNull(problem);

        if (characteristics.Horizon != problem.Horizon ||
            characteristics.TotalDemand != problem.TotalDemand)
        {
            throw new ArgumentException(
                "The supplied characteristics do not describe the supplied problem.",
                nameof(characteristics));
        }

        return characteristics.HasNoSpeculativeMotiveCosts
            ? _linearSolver
            : _generalSolver;
    }

    /// <inheritdoc />
    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        var selected = SelectSolver(problem);
        return selected.Solve(problem, cancellationToken);
    }
}
