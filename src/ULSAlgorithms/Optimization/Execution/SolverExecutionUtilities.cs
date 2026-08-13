using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Optimization.Execution;

internal static class SolverExecutionUtilities
{
    internal static string CreateArtifactDirectory(
        SolverKind solverKind,
        LinearModelSolveOptions options)
    {
        string parent =
            string.IsNullOrWhiteSpace(
                options.TemporaryRootPath)
                ? Path.GetTempPath()
                : Path.GetFullPath(
                    options.TemporaryRootPath);

        Directory.CreateDirectory(parent);

        string directory =
            Path.Combine(
                parent,
                "ULSAlgorithms",
                solverKind.ToString(),
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);
        return directory;
    }

    internal static void ExportModelIfRequested(
        string generatedModelPath,
        LinearModelSolveOptions options)
    {
        if (string.IsNullOrWhiteSpace(
                options.ExportModelPath))
        {
            return;
        }

        string destination =
            Path.GetFullPath(
                options.ExportModelPath);

        string? directory =
            Path.GetDirectoryName(destination);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(
            generatedModelPath,
            destination,
            overwrite: true);
    }

    internal static IReadOnlyDictionary<int, double>
        CompleteValues(
            LinearModel model,
            IReadOnlyDictionary<int, double> returnedValues)
    {
        var completed =
            new Dictionary<int, double>(
                model.VariableCount);

        foreach (LinearVariable variable in model.Variables)
        {
            completed[variable.Id] =
                returnedValues.TryGetValue(
                    variable.Id,
                    out double value)
                    ? value
                    : variable.LowerBound == variable.UpperBound
                        ? variable.LowerBound
                        : 0.0;
        }

        return completed;
    }

    internal static LinearModelSolveResult BuildSolutionResult(
        LinearModel model,
        SolverSelectionResult selection,
        LinearModelSolveOptions options,
        LinearModelSolveStatus proposedStatus,
        IReadOnlyDictionary<int, double> returnedValues,
        bool hasCandidateSolution,
        TimeSpan duration,
        string nativeStatus,
        IEnumerable<string> diagnostics,
        string artifactDirectory)
    {
        var messages =
            diagnostics.ToList();

        IReadOnlyDictionary<int, double> completed =
            hasCandidateSolution
                ? CompleteValues(
                    model,
                    returnedValues)
                : new Dictionary<int, double>();

        IReadOnlyDictionary<int, double> normalized =
            hasCandidateSolution
                ? NormalizeValues(
                    model,
                    completed,
                    options)
                : completed;

        LinearModelSolutionValidation? validation =
            null;

        LinearModelSolveStatus normalizedStatus =
            proposedStatus;

        if (hasCandidateSolution)
        {
            validation =
                LinearModelSolutionValidator.Validate(
                    model,
                    normalized,
                    options.FeasibilityTolerance,
                    options.IntegralityTolerance);

            foreach (string diagnostic in validation.Diagnostics)
            {
                messages.Add(
                    "Independent checker: " +
                    diagnostic);
            }

            if (!validation.IsFeasible &&
                proposedStatus is
                    LinearModelSolveStatus.Optimal or
                    LinearModelSolveStatus.Feasible)
            {
                normalizedStatus =
                    LinearModelSolveStatus.Failed;

                messages.Add(
                    "The native solver returned a candidate solution, but " +
                    "the independent portable-model checker rejected it.");
            }
        }

        string retainedDirectory =
            options.KeepTemporaryFiles
                ? artifactDirectory
                : string.Empty;

        return new LinearModelSolveResult(
            model.Name,
            normalizedStatus,
            new SolverExecutionInfo(selection),
            normalized,
            validation,
            duration,
            nativeStatus,
            messages,
            retainedDirectory)
        {
            SolverReportedStatus =
                proposedStatus
        };
    }


    internal static IReadOnlyDictionary<int, double> NormalizeValues(
        LinearModel model,
        IReadOnlyDictionary<int, double> values,
        LinearModelSolveOptions options)
    {
        var normalizer =
            new LinearVariableValueNormalizer(
                options.ZeroTolerance,
                options.IntegralityTolerance,
                options.NearIntegerTolerance);

        var normalized =
            new Dictionary<int, double>(
                model.VariableCount);

        foreach (LinearVariable variable in model.Variables)
        {
            if (!values.TryGetValue(
                    variable.Id,
                    out double rawValue))
            {
                throw new InvalidOperationException(
                    $"No returned value exists for variable '{variable.Name}'.");
            }

            normalized[variable.Id] =
                normalizer.Normalize(
                    variable,
                    rawValue);
        }

        return normalized;
    }

    internal static void DeleteArtifactsUnlessRetained(
        string directory,
        LinearModelSolveOptions options)
    {
        if (options.KeepTemporaryFiles)
        {
            return;
        }

        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(
                    directory,
                    recursive: true);
            }
        }
        catch
        {
            // Cleanup must never mask a solver result.
        }
    }

    internal static string LastMeaningfulLine(
        string text)
    {
        return text
            .Split(
                ['\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .LastOrDefault() ??
            string.Empty;
    }
}

