using System.Reflection;
using System.Runtime.Loader;

namespace ULSAlgorithms.Optimization.Adapters.Cplex;

/// <summary>
/// Detects and validates an optional IBM ILOG CPLEX installation.
/// </summary>
/// <remarks>
/// The managed CPLEX assemblies are loaded dynamically so ULSAlgorithms keeps
/// no compile-time dependency on IBM binaries.
/// </remarks>
public sealed class CplexSolverAdapter :
    OptimizationSolverAdapterBase
{
    private static readonly object RuntimeGate = new();

    /// <summary>Initializes the CPLEX adapter.</summary>
    public CplexSolverAdapter()
        : base(
            SolverCapability.LinearProgramming,
            SolverCapability.MixedIntegerLinearProgramming,
            SolverCapability.Interruption,
            SolverCapability.LpExport,
            SolverCapability.MpsExport,
            SolverCapability.OptimalityGapReporting,
            SolverCapability.SearchStatistics)
    {
    }

    /// <inheritdoc />
    public override string AdapterId =>
        "ULSAlgorithms.Solver.Cplex";

    /// <inheritdoc />
    public override string AdapterName =>
        "ULSAlgorithms CPLEX Adapter";

    /// <inheritdoc />
    public override SolverKind SolverKind =>
        SolverKind.Cplex;

    /// <inheritdoc />
    public override ValueTask<SolverAvailabilityInfo>
        CheckAvailabilityAsync(
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        CplexInstallationDiscoveryResult discovery =
            CplexInstallationLocator.Discover();

        if (discovery.Installation is null)
        {
            return ValueTask.FromResult(
                new SolverAvailabilityInfo(
                    SolverKind.Cplex,
                    SolverAvailabilityStatus.NotInstalled,
                    solverName: "IBM ILOG CPLEX",
                    diagnostics: discovery.Diagnostics));
        }

        lock (RuntimeGate)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CplexInstallationInfo installation =
                discovery.Installation;

            string oldPath =
                Environment.GetEnvironmentVariable("PATH") ??
                string.Empty;

            try
            {
                PrependPath(
                    installation.RuntimeDirectory,
                    oldPath);

                LoadAssemblyIfNeeded(
                    "ILOG.Concert",
                    installation.ConcertAssemblyPath);

                Assembly cplexAssembly =
                    LoadAssemblyIfNeeded(
                        "ILOG.CPLEX",
                        installation.CplexAssemblyPath);

                Type cplexType =
                    cplexAssembly.GetType(
                        "ILOG.CPLEX.Cplex",
                        throwOnError: true,
                        ignoreCase: false)!;

                object? cplex =
                    Activator.CreateInstance(cplexType);

                if (cplex is null)
                {
                    throw new InvalidOperationException(
                        "ILOG.CPLEX.Cplex could not be instantiated.");
                }

                try
                {
                    string version =
                        cplexType
                            .GetProperty(
                                "Version",
                                BindingFlags.Instance |
                                BindingFlags.Public)?
                            .GetValue(cplex)?
                            .ToString() ??
                        string.Empty;

                    var diagnostics =
                        discovery.Diagnostics
                            .Concat(
                            [
                                "The ILOG.Concert and ILOG.CPLEX managed " +
                                "assemblies were loaded successfully.",
                                "ILOG.CPLEX.Cplex was instantiated successfully."
                            ])
                            .ToArray();

                    return ValueTask.FromResult(
                        new SolverAvailabilityInfo(
                            SolverKind.Cplex,
                            SolverAvailabilityStatus.Available,
                            solverName: "IBM ILOG CPLEX",
                            solverVersion: version,
                            installationPath:
                                installation.RootDirectory,
                            managedAssemblyPath:
                                installation.CplexAssemblyPath,
                            nativeLibraryPath:
                                installation.RuntimeDirectory,
                            licenseInformation:
                                "CPLEX environment creation succeeded.",
                            diagnostics: diagnostics));
                }
                finally
                {
                    cplexType
                        .GetMethod(
                            "End",
                            BindingFlags.Instance |
                            BindingFlags.Public,
                            binder: null,
                            types: Type.EmptyTypes,
                            modifiers: null)?
                        .Invoke(
                            cplex,
                            null);
                }
            }
            catch (Exception exception)
            {
                Exception effective =
                    Unwrap(exception);

                return ValueTask.FromResult(
                    new SolverAvailabilityInfo(
                        SolverKind.Cplex,
                        ClassifyFailure(effective),
                        solverName: "IBM ILOG CPLEX",
                        installationPath:
                            discovery.Installation.RootDirectory,
                        managedAssemblyPath:
                            discovery.Installation.CplexAssemblyPath,
                        nativeLibraryPath:
                            discovery.Installation.RuntimeDirectory,
                        diagnostics:
                            discovery.Diagnostics.Concat(
                            [
                                effective.Message
                            ])));
            }
            finally
            {
                Environment.SetEnvironmentVariable(
                    "PATH",
                    oldPath);
            }
        }
    }

    private static Assembly LoadAssemblyIfNeeded(
        string simpleName,
        string path)
    {
        Assembly? loaded =
            AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(
                    assembly =>
                        string.Equals(
                            assembly.GetName().Name,
                            simpleName,
                            StringComparison.OrdinalIgnoreCase));

        return loaded ??
               AssemblyLoadContext.Default.LoadFromAssemblyPath(
                   Path.GetFullPath(path));
    }

    private static void PrependPath(
        string directory,
        string currentPath)
    {
        string updated =
            string.IsNullOrWhiteSpace(currentPath)
                ? directory
                : directory +
                  Path.PathSeparator +
                  currentPath;

        Environment.SetEnvironmentVariable(
            "PATH",
            updated);
    }
}
