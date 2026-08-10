using System.Reflection;

namespace ULSAlgorithms.Optimization.Adapters.Xpress;

/// <summary>
/// Detects and validates FICO Xpress through its optional Optimizer .NET
/// assembly.
/// </summary>
public sealed class XpressSolverAdapter :
    OptimizationSolverAdapterBase
{
    private static readonly SemaphoreSlim RuntimeGate =
        new(1, 1);

    /// <summary>Initializes the Xpress adapter.</summary>
    public XpressSolverAdapter()
        : base(
            SolverCapability.LinearProgramming,
            SolverCapability.MixedIntegerLinearProgramming,
            SolverCapability.LpExport,
            SolverCapability.OptimalityGapReporting,
            SolverCapability.SearchStatistics)
    {
    }

    /// <inheritdoc />
    public override string AdapterId =>
        "ULSAlgorithms.Solver.Xpress";

    /// <inheritdoc />
    public override string AdapterName =>
        "ULSAlgorithms Xpress Adapter";

    /// <inheritdoc />
    public override SolverKind SolverKind =>
        SolverKind.Xpress;

    /// <inheritdoc />
    public override async ValueTask<SolverAvailabilityInfo>
        CheckAvailabilityAsync(
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Assembly? assembly;
        string resolvedPath;

        try
        {
            assembly =
                XpressRuntimeLocator.TryLoad(
                    out resolvedPath);
        }
        catch (Exception exception)
        {
            Exception effective =
                Unwrap(exception);

            return Failure(
                ClassifyFailure(effective),
                string.Empty,
                effective.Message);
        }

        if (assembly is null)
        {
            return new SolverAvailabilityInfo(
                SolverKind.Xpress,
                SolverAvailabilityStatus.NotInstalled,
                solverName: "FICO Xpress MP",
                diagnostics:
                [
                    "Optimizer.dll was not found. Define XPRESSDIR or set " +
                    "ULSALGORITHMS_XPRESS_OPTIMIZER_ASSEMBLY."
                ]);
        }

        await RuntimeGate.WaitAsync(
            cancellationToken);

        try
        {
            Type xprsType =
                assembly.GetType(
                    "Optimizer.XPRS",
                    throwOnError: true,
                    ignoreCase: false)!;

            InvokeXpressInit(
                xprsType);

            try
            {
                string version =
                    TryGetStaticProperty(
                        xprsType,
                        "Version",
                        "VERSION")?
                    .ToString() ??
                    string.Empty;

                return new SolverAvailabilityInfo(
                    SolverKind.Xpress,
                    SolverAvailabilityStatus.Available,
                    solverName: "FICO Xpress MP",
                    solverVersion: version,
                    installationPath:
                        ResolveInstallationDirectory(
                            resolvedPath),
                    managedAssemblyPath:
                        resolvedPath,
                    nativeLibraryPath:
                        Environment.GetEnvironmentVariable(
                            "XPRESSDIR") ??
                        string.Empty,
                    licenseInformation:
                        "XPRS.Init completed successfully.",
                    diagnostics:
                    [
                        $"FICO Xpress Optimizer runtime loaded from " +
                        $"'{resolvedPath}'.",
                        "XPRS.Init completed successfully, including " +
                        "native runtime/license initialization."
                    ]);
            }
            finally
            {
                TryInvokeStatic(
                    xprsType,
                    "Free");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Exception effective =
                Unwrap(exception);

            return Failure(
                ClassifyFailure(effective),
                resolvedPath,
                effective.Message);
        }
        finally
        {
            RuntimeGate.Release();
        }
    }

    private static SolverAvailabilityInfo Failure(
        SolverAvailabilityStatus status,
        string resolvedPath,
        string diagnostic)
    {
        return new SolverAvailabilityInfo(
            SolverKind.Xpress,
            status,
            solverName: "FICO Xpress MP",
            installationPath:
                ResolveInstallationDirectory(
                    resolvedPath),
            managedAssemblyPath:
                resolvedPath,
            nativeLibraryPath:
                Environment.GetEnvironmentVariable(
                    "XPRESSDIR") ??
                string.Empty,
            diagnostics: [diagnostic]);
    }

    private static string ResolveInstallationDirectory(
        string path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            string.Equals(
                path,
                "already loaded",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                path,
                "assembly probing",
                StringComparison.OrdinalIgnoreCase))
        {
            return Environment.GetEnvironmentVariable(
                       "XPRESSDIR") ??
                   string.Empty;
        }

        return Path.GetDirectoryName(path) ??
               string.Empty;
    }

    private static object? TryGetStaticProperty(
        Type type,
        params string[] names)
    {
        foreach (string name in names)
        {
            PropertyInfo? property =
                type.GetProperty(
                    name,
                    BindingFlags.Public |
                    BindingFlags.Static |
                    BindingFlags.IgnoreCase);

            if (property is not null)
            {
                return property.GetValue(null);
            }

            FieldInfo? field =
                type.GetField(
                    name,
                    BindingFlags.Public |
                    BindingFlags.Static |
                    BindingFlags.IgnoreCase);

            if (field is not null)
            {
                return field.GetValue(null);
            }
        }

        return null;
    }

    private static void InvokeXpressInit(
        Type xprsType)
    {
        MethodInfo? withArgument =
            FindStaticMethod(
                xprsType,
                "Init",
                parameterCount: 1);

        if (withArgument is not null)
        {
            withArgument.Invoke(
                null,
                [string.Empty]);
            return;
        }

        MethodInfo? withoutArgument =
            FindStaticMethod(
                xprsType,
                "Init",
                parameterCount: 0);

        if (withoutArgument is not null)
        {
            withoutArgument.Invoke(
                null,
                null);
            return;
        }

        throw new MissingMethodException(
            xprsType.FullName,
            "Init");
    }

    private static void InvokeStatic(
        Type type,
        string methodName,
        params object?[] arguments)
    {
        MethodInfo method =
            FindStaticMethod(
                type,
                methodName,
                arguments.Length) ??
            throw new MissingMethodException(
                type.FullName,
                methodName);

        method.Invoke(
            null,
            arguments);
    }

    private static void TryInvokeStatic(
        Type type,
        string methodName,
        params object?[] arguments)
    {
        try
        {
            MethodInfo? method =
                FindStaticMethod(
                    type,
                    methodName,
                    arguments.Length);

            method?.Invoke(
                null,
                arguments);
        }
        catch
        {
            // Cleanup must not mask the original result/error.
        }
    }

    private static MethodInfo? FindStaticMethod(
        Type type,
        string methodName,
        int parameterCount)
    {
        return type
            .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
            .FirstOrDefault(
                method =>
                    string.Equals(
                        method.Name,
                        methodName,
                        StringComparison.OrdinalIgnoreCase) &&
                    method.GetParameters().Length ==
                        parameterCount);
    }
}
