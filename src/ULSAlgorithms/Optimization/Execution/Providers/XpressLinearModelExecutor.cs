using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using ULSAlgorithms.Optimization.Adapters.Xpress;
using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Optimization.Execution.Providers;

/// <summary>
/// Executes portable LP/MILP models through the optional FICO Xpress
/// Optimizer .NET runtime loaded by reflection.
/// </summary>
public sealed class XpressLinearModelExecutor :
    ILinearModelSolverExecutor
{
    private static readonly SemaphoreSlim RuntimeGate =
        new(1, 1);

    /// <inheritdoc />
    public SolverKind SolverKind =>
        SolverKind.Xpress;

    /// <inheritdoc />
    public async ValueTask<LinearModelSolveResult> SolveAsync(
        LinearModel model,
        SolverSelectionResult selection,
        LinearModelSolveOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(options);

        if (selection.SelectedSolver != SolverKind.Xpress)
        {
            throw new ArgumentException(
                "The supplied selection does not target Xpress.",
                nameof(selection));
        }

        string artifacts =
            SolverExecutionUtilities.CreateArtifactDirectory(
                SolverKind,
                options);

        var stopwatch =
            Stopwatch.StartNew();

        await RuntimeGate.WaitAsync(
            cancellationToken);

        try
        {
            string modelPath =
                Path.Combine(
                    artifacts,
                    "model.lp");

            new PortableLpModelWriter().Write(
                model,
                modelPath);

            SolverExecutionUtilities.ExportModelIfRequested(
                modelPath,
                options);

            Assembly? assembly =
                XpressRuntimeLocator.TryLoad(
                    out string resolvedPath);

            if (assembly is null)
            {
                return new LinearModelSolveResult(
                    model.Name,
                    LinearModelSolveStatus.Failed,
                    new SolverExecutionInfo(selection),
                    null,
                    null,
                    stopwatch.Elapsed,
                    "Optimizer.dll unavailable",
                    ["Xpress was selected but Optimizer.dll could not be loaded."]);
            }

            var api =
                XpressExecutionReflectionApi.Create(
                    assembly);

            api.Initialize();

            object? problem =
                null;

            try
            {
                problem =
                    api.CreateProblem();

                api.ReadProblem(
                    problem,
                    modelPath);

                using CancellationTokenRegistration cancellationRegistration =
                    cancellationToken.Register(
                        static state =>
                            XpressExecutionReflectionApi.TryInterrupt(
                                state!),
                        problem);

                await Task.Run(
                    () =>
                        api.Optimize(problem),
                    CancellationToken.None);

                stopwatch.Stop();

                double[]? vector =
                    api.TryGetSolution(
                        problem);

                IReadOnlyDictionary<int, double> values =
                    api.GetValues(
                        problem,
                        model,
                        vector,
                        out string mappingDiagnostic);

                bool solutionExists =
                    vector is not null;

                string nativeStatus =
                    api.TryGetStatus(
                        problem);

                LinearModelSolveStatus status =
                    MapStatus(
                        nativeStatus,
                        problem,
                        api,
                        solutionExists);

                var diagnostics =
                    new List<string>
                    {
                        $"Xpress runtime: {resolvedPath}."
                    };

                if (!string.IsNullOrWhiteSpace(
                        mappingDiagnostic))
                {
                    diagnostics.Add(
                        mappingDiagnostic);
                }

                if (!string.IsNullOrWhiteSpace(
                        nativeStatus))
                {
                    diagnostics.Add(
                        $"Xpress status: {nativeStatus}.");
                }

                return SolverExecutionUtilities.BuildSolutionResult(
                    model,
                    selection,
                    options,
                    status,
                    values,
                    solutionExists,
                    stopwatch.Elapsed,
                    nativeStatus,
                    diagnostics,
                    artifacts);
            }
            finally
            {
                if (problem is not null)
                {
                    api.DestroyProblem(
                        problem);
                }

                api.Free();
            }
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            return new LinearModelSolveResult(
                model.Name,
                LinearModelSolveStatus.Cancelled,
                new SolverExecutionInfo(selection),
                null,
                null,
                stopwatch.Elapsed,
                "Cancelled",
                ["Xpress execution was cancelled by the caller."],
                options.KeepTemporaryFiles
                    ? artifacts
                    : string.Empty);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            Exception effective =
                exception is TargetInvocationException invocation &&
                invocation.InnerException is Exception inner
                    ? inner
                    : exception;

            return new LinearModelSolveResult(
                model.Name,
                LinearModelSolveStatus.Failed,
                new SolverExecutionInfo(selection),
                null,
                null,
                stopwatch.Elapsed,
                effective.GetType().Name,
                [effective.Message],
                options.KeepTemporaryFiles
                    ? artifacts
                    : string.Empty);
        }
        finally
        {
            RuntimeGate.Release();

            SolverExecutionUtilities.DeleteArtifactsUnlessRetained(
                artifacts,
                options);
        }
    }

    private static LinearModelSolveStatus MapStatus(
        string status,
        object problem,
        XpressExecutionReflectionApi api,
        bool hasSolution)
    {
        if (status.Contains(
                "infeasible",
                StringComparison.OrdinalIgnoreCase) &&
            status.Contains(
                "unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.InfeasibleOrUnbounded;
        }

        if (status.Contains(
                "infeasible",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Infeasible;
        }

        if (status.Contains(
                "unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Unbounded;
        }

        if (status.Contains(
                "optimal",
                StringComparison.OrdinalIgnoreCase))
        {
            return LinearModelSolveStatus.Optimal;
        }

        if (hasSolution)
        {
            double? objective =
                api.TryGetDoubleProperty(
                    problem,
                    "MIPBestObjVal",
                    "MIPObjVal",
                    "LPObjVal",
                    "ObjVal");

            double? bound =
                api.TryGetDoubleProperty(
                    problem,
                    "BestBound",
                    "MIPBestBound");

            if (objective.HasValue &&
                bound.HasValue &&
                Math.Abs(
                    objective.Value -
                    bound.Value) <=
                1.0e-10 *
                Math.Max(
                    1.0,
                    Math.Abs(objective.Value)))
            {
                return LinearModelSolveStatus.Optimal;
            }

            return LinearModelSolveStatus.Feasible;
        }

        return LinearModelSolveStatus.Unknown;
    }
}

internal sealed class XpressExecutionReflectionApi
{
    private readonly Type _xprsType;
    private readonly Type _problemType;

    private XpressExecutionReflectionApi(
        Type xprsType,
        Type problemType)
    {
        _xprsType = xprsType;
        _problemType = problemType;
    }

    internal static XpressExecutionReflectionApi Create(
        Assembly assembly)
    {
        Type xprsType =
            assembly.GetType(
                "Optimizer.XPRS",
                throwOnError: true,
                ignoreCase: false)!;

        Type problemType =
            assembly.GetType(
                "Optimizer.XPRSprob",
                throwOnError: true,
                ignoreCase: false)!;

        return new XpressExecutionReflectionApi(
            xprsType,
            problemType);
    }

    internal void Initialize()
    {
        MethodInfo? oneArgument =
            FindMethod(
                _xprsType,
                "Init",
                isStatic: true,
                parameterCount: 1);

        if (oneArgument is not null)
        {
            oneArgument.Invoke(
                null,
                [string.Empty]);

            return;
        }

        MethodInfo? zeroArgument =
            FindMethod(
                _xprsType,
                "Init",
                isStatic: true,
                parameterCount: 0);

        zeroArgument?.Invoke(
            null,
            null);

        if (oneArgument is null &&
            zeroArgument is null)
        {
            throw new MissingMethodException(
                _xprsType.FullName,
                "Init");
        }
    }

    internal void Free()
    {
        try
        {
            FindMethod(
                _xprsType,
                "Free",
                isStatic: true,
                parameterCount: 0)?
                .Invoke(
                    null,
                    null);
        }
        catch
        {
            // Cleanup must not mask the solve result.
        }
    }

    internal object CreateProblem()
    {
        return Activator.CreateInstance(
                   _problemType) ??
               throw new InvalidOperationException(
                   "Unable to create Optimizer.XPRSprob.");
    }

    internal void ReadProblem(
        object problem,
        string modelPath)
    {
        TrySetProperty(
            problem,
            "MPSFormat",
            -1);

        InvokeRequired(
            problem,
            "ReadProb",
            modelPath,
            string.Empty);
    }

    internal void Optimize(
        object problem)
    {
        InvokeRequired(
            problem,
            "Optimize");
    }

    internal double[]? TryGetSolution(
        object problem)
    {
        try
        {
            return InvokeRequired(
                problem,
                "GetSolution") as double[];
        }
        catch
        {
            return null;
        }
    }

    internal IReadOnlyDictionary<int, double> GetValues(
        object problem,
        LinearModel model,
        double[]? solution,
        out string diagnostic)
    {
        diagnostic = string.Empty;

        if (solution is null)
        {
            return new Dictionary<int, double>();
        }

        var byName =
            new Dictionary<int, double>();

        bool allResolved = true;

        foreach (LinearVariable variable in model.Variables)
        {
            if (!TryGetColumnIndex(
                    problem,
                    PortableLpModelWriter.GetVariableName(
                        variable.Id),
                    out int index) ||
                index < 0 ||
                index >= solution.Length)
            {
                allResolved = false;
                break;
            }

            byName[variable.Id] =
                solution[index];
        }

        if (allResolved &&
            byName.Count == model.VariableCount)
        {
            diagnostic =
                "Xpress solution values were mapped by portable variable name.";

            return byName;
        }

        if (solution.Length ==
            model.VariableCount)
        {
            var positional =
                new Dictionary<int, double>();

            for (int index = 0;
                 index < model.VariableCount;
                 index++)
            {
                positional[
                    model.Variables[index].Id] =
                    solution[index];
            }

            diagnostic =
                "Xpress used the LP column-order fallback because GetIndex " +
                "could not map every portable variable name.";

            return positional;
        }

        diagnostic =
            "Xpress returned a solution vector that could not be mapped safely.";

        return new Dictionary<int, double>();
    }

    internal string TryGetStatus(
        object problem)
    {
        return TryGetProperty(
                   problem,
                   "SolStatus",
                   "MIPStatus",
                   "LPStatus")?
               .ToString() ??
               string.Empty;
    }

    internal double? TryGetDoubleProperty(
        object problem,
        params string[] names)
    {
        object? value =
            TryGetProperty(
                problem,
                names);

        if (value is null)
        {
            return null;
        }

        try
        {
            double converted =
                Convert.ToDouble(
                    value,
                    CultureInfo.InvariantCulture);

            return double.IsFinite(converted)
                ? converted
                : null;
        }
        catch
        {
            return null;
        }
    }

    internal void DestroyProblem(
        object problem)
    {
        try
        {
            InvokeRequired(
                problem,
                "Destroy");
        }
        catch
        {
            if (problem is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    internal static void TryInterrupt(
        object problem)
    {
        foreach (string name in
                 new[]
                 {
                     "Interrupt",
                     "Stop"
                 })
        {
            try
            {
                MethodInfo? method =
                    FindMethod(
                        problem.GetType(),
                        name,
                        isStatic: false,
                        parameterCount: 0);

                if (method is null)
                {
                    continue;
                }

                method.Invoke(
                    problem,
                    null);

                return;
            }
            catch
            {
                // Best-effort interruption.
            }
        }
    }

    private static bool TryGetColumnIndex(
        object problem,
        string name,
        out int index)
    {
        index = -1;

        MethodInfo? twoParameter =
            FindMethod(
                problem.GetType(),
                "GetIndex",
                isStatic: false,
                parameterCount: 2);

        if (twoParameter is not null)
        {
            try
            {
                object? value =
                    twoParameter.Invoke(
                        problem,
                        [2, name]);

                if (value is not null)
                {
                    index =
                        Convert.ToInt32(
                            value,
                            CultureInfo.InvariantCulture);

                    return index >= 0;
                }
            }
            catch
            {
                // Try out-parameter shape.
            }
        }

        MethodInfo? threeParameter =
            FindMethod(
                problem.GetType(),
                "GetIndex",
                isStatic: false,
                parameterCount: 3);

        if (threeParameter is not null)
        {
            try
            {
                object?[] arguments =
                    [2, name, 0];

                threeParameter.Invoke(
                    problem,
                    arguments);

                index =
                    Convert.ToInt32(
                        arguments[2],
                        CultureInfo.InvariantCulture);

                return index >= 0;
            }
            catch
            {
                // No safe named mapping.
            }
        }

        return false;
    }

    private static object? TryGetProperty(
        object target,
        params string[] names)
    {
        foreach (string name in names)
        {
            PropertyInfo? property =
                target.GetType().GetProperty(
                    name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.IgnoreCase);

            if (property is not null)
            {
                return property.GetValue(
                    target);
            }
        }

        return null;
    }

    private static bool TrySetProperty(
        object target,
        string name,
        object value)
    {
        PropertyInfo? property =
            target.GetType().GetProperty(
                name,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.IgnoreCase);

        if (property is null ||
            !property.CanWrite)
        {
            return false;
        }

        try
        {
            object converted =
                Convert.ChangeType(
                    value,
                    property.PropertyType,
                    CultureInfo.InvariantCulture);

            property.SetValue(
                target,
                converted);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static object? InvokeRequired(
        object target,
        string name,
        params object?[] arguments)
    {
        MethodInfo method =
            FindMethod(
                target.GetType(),
                name,
                isStatic: false,
                parameterCount: arguments.Length) ??
            throw new MissingMethodException(
                target.GetType().FullName,
                name);

        return method.Invoke(
            target,
            arguments);
    }

    private static MethodInfo? FindMethod(
        Type type,
        string name,
        bool isStatic,
        int parameterCount)
    {
        BindingFlags flags =
            BindingFlags.Public |
            (isStatic
                ? BindingFlags.Static
                : BindingFlags.Instance);

        return type
            .GetMethods(flags)
            .FirstOrDefault(
                method =>
                    string.Equals(
                        method.Name,
                        name,
                        StringComparison.OrdinalIgnoreCase) &&
                    method.GetParameters().Length ==
                        parameterCount);
    }
}
