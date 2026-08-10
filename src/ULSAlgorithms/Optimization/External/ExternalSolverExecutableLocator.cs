namespace ULSAlgorithms.Optimization.External;

/// <summary>
/// Locates optional native-solver command-line executables without requiring
/// their SDKs at compile time.
/// </summary>
public static class ExternalSolverExecutableLocator
{
    /// <summary>
    /// Resolves an executable from explicit environment variables,
    /// installation-home hints, optional direct candidates, and PATH.
    /// </summary>
    public static string Resolve(
        IEnumerable<string> explicitExecutableEnvironmentVariables,
        IEnumerable<string> homeEnvironmentVariables,
        IEnumerable<string> relativeExecutablePaths,
        IEnumerable<string> pathExecutableNames,
        IEnumerable<string>? directCandidates = null)
    {
        ArgumentNullException.ThrowIfNull(
            explicitExecutableEnvironmentVariables);
        ArgumentNullException.ThrowIfNull(homeEnvironmentVariables);
        ArgumentNullException.ThrowIfNull(relativeExecutablePaths);
        ArgumentNullException.ThrowIfNull(pathExecutableNames);

        foreach (string variableName in
                 explicitExecutableEnvironmentVariables)
        {
            if (string.IsNullOrWhiteSpace(variableName))
            {
                continue;
            }

            string configured =
                Environment.GetEnvironmentVariable(
                    variableName)?.Trim() ??
                string.Empty;

            string existing =
                NormalizeExistingFile(configured);

            if (!string.IsNullOrEmpty(existing))
            {
                return existing;
            }
        }

        string[] relativePaths =
            relativeExecutablePaths
                .Where(static path =>
                    !string.IsNullOrWhiteSpace(path))
                .ToArray();

        foreach (string variableName in homeEnvironmentVariables)
        {
            if (string.IsNullOrWhiteSpace(variableName))
            {
                continue;
            }

            string home =
                Environment.GetEnvironmentVariable(
                    variableName)?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(home))
            {
                continue;
            }

            foreach (string relativePath in relativePaths)
            {
                string existing =
                    NormalizeExistingFile(
                        Path.Combine(home, relativePath));

                if (!string.IsNullOrEmpty(existing))
                {
                    return existing;
                }
            }
        }

        if (directCandidates is not null)
        {
            foreach (string candidate in directCandidates)
            {
                string existing =
                    NormalizeExistingFile(candidate);

                if (!string.IsNullOrEmpty(existing))
                {
                    return existing;
                }
            }
        }

        return FindOnPath(pathExecutableNames);
    }

    /// <summary>
    /// Searches PATH for one of the specified executable names.
    /// </summary>
    public static string FindOnPath(
        IEnumerable<string> executableNames)
    {
        ArgumentNullException.ThrowIfNull(executableNames);

        string pathValue =
            Environment.GetEnvironmentVariable("PATH") ??
            string.Empty;

        string[] directories =
            pathValue.Split(
                Path.PathSeparator,
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        foreach (string directory in directories)
        {
            foreach (string executableName in executableNames)
            {
                if (string.IsNullOrWhiteSpace(executableName))
                {
                    continue;
                }

                try
                {
                    string existing =
                        NormalizeExistingFile(
                            Path.Combine(
                                directory,
                                executableName.Trim()));

                    if (!string.IsNullOrEmpty(existing))
                    {
                        return existing;
                    }
                }
                catch
                {
                    // Ignore malformed PATH entries and continue.
                }
            }
        }

        return string.Empty;
    }

    private static string NormalizeExistingFile(
        string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        try
        {
            string expanded =
                Environment.ExpandEnvironmentVariables(
                    path.Trim().Trim('"'));

            return File.Exists(expanded)
                ? Path.GetFullPath(expanded)
                : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
