namespace ULSAlgorithms.Optimization.Adapters.Cplex;

/// <summary>
/// Locates compatible IBM ILOG CPLEX installations without a compile-time
/// reference to ILOG.Concert or ILOG.CPLEX.
/// </summary>
public static class CplexInstallationLocator
{
    private static readonly string[] ExplicitHomeVariables =
    [
        "ULSALGORITHMS_CPLEX_HOME"
    ];

    private static readonly (string Version, string[] Variables)[] KnownHomes =
    [
        ("22.2",   ["CPLEX_STUDIO_DIR222", "CPLEX_STUDIO_DIR2220"]),
        ("22.1.2", ["CPLEX_STUDIO_DIR2212"]),
        ("22.1.1", ["CPLEX_STUDIO_DIR2211"]),
        ("22.1",   ["CPLEX_STUDIO_DIR221"]),
        ("20.1",   ["CPLEX_STUDIO_DIR201"])
    ];

    /// <summary>
    /// Finds the newest compatible CPLEX installation visible to the process.
    /// </summary>
    public static CplexInstallationDiscoveryResult Discover()
    {
        var diagnostics = new List<string>();
        var visited =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (string variableName in ExplicitHomeVariables)
        {
            string home =
                Environment.GetEnvironmentVariable(
                    variableName)?.Trim() ??
                string.Empty;

            if (TryCandidate(
                    versionFamily: "explicit",
                    home,
                    $"environment variable {variableName}",
                    visited,
                    diagnostics,
                    out CplexInstallationInfo? installation))
            {
                return new CplexInstallationDiscoveryResult(
                    installation,
                    diagnostics);
            }
        }

        foreach ((string version, string[] variables) in KnownHomes)
        {
            foreach (string variableName in variables)
            {
                string home =
                    Environment.GetEnvironmentVariable(
                        variableName)?.Trim() ??
                    string.Empty;

                if (TryCandidate(
                        version,
                        home,
                        $"environment variable {variableName}",
                        visited,
                        diagnostics,
                        out CplexInstallationInfo? installation))
                {
                    return new CplexInstallationDiscoveryResult(
                        installation,
                        diagnostics);
                }
            }
        }

        if (OperatingSystem.IsWindows())
        {
            foreach (string root in EnumerateWindowsInstallations())
            {
                if (TryCandidate(
                        versionFamily:
                            Path.GetFileName(root),
                        root,
                        "IBM ILOG Program Files scan",
                        visited,
                        diagnostics,
                        out CplexInstallationInfo? installation))
                {
                    return new CplexInstallationDiscoveryResult(
                        installation,
                        diagnostics);
                }
            }
        }

        diagnostics.Add(
            "No compatible CPLEX installation was found. " +
            "Set ULSALGORITHMS_CPLEX_HOME or a standard CPLEX_STUDIO_DIR* " +
            "environment variable when CPLEX is installed in a non-standard " +
            "location.");

        return new CplexInstallationDiscoveryResult(
            null,
            diagnostics);
    }

    /// <summary>
    /// Resolves one explicit CPLEX root. This helper is also useful for tests.
    /// </summary>
    public static CplexInstallationInfo? TryResolveRoot(
        string rootDirectory,
        string discoverySource = "explicit path",
        string versionFamily = "unknown")
    {
        var diagnostics = new List<string>();
        var visited =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        return TryCandidate(
            versionFamily,
            rootDirectory,
            discoverySource,
            visited,
            diagnostics,
            out CplexInstallationInfo? installation)
            ? installation
            : null;
    }

    private static bool TryCandidate(
        string versionFamily,
        string? rootDirectory,
        string source,
        ISet<string> visited,
        ICollection<string> diagnostics,
        out CplexInstallationInfo? installation)
    {
        installation = null;

        if (string.IsNullOrWhiteSpace(rootDirectory))
        {
            return false;
        }

        string root;

        try
        {
            root =
                Path.GetFullPath(
                    Environment.ExpandEnvironmentVariables(
                        rootDirectory.Trim().Trim('"')));
        }
        catch
        {
            return false;
        }

        if (!visited.Add(root))
        {
            return false;
        }

        string runtimeDirectory =
            Path.Combine(
                root,
                "cplex",
                "bin",
                "x64_win64");

        string concertAssembly =
            Path.Combine(
                runtimeDirectory,
                "ILOG.Concert.dll");

        string cplexAssembly =
            Path.Combine(
                runtimeDirectory,
                "ILOG.CPLEX.dll");

        if (!File.Exists(concertAssembly) ||
            !File.Exists(cplexAssembly))
        {
            diagnostics.Add(
                $"CPLEX candidate '{root}' was found from {source}, but " +
                "ILOG.Concert.dll and ILOG.CPLEX.dll were not both present " +
                $"under '{runtimeDirectory}'.");

            return false;
        }

        installation =
            new CplexInstallationInfo(
                versionFamily,
                root,
                runtimeDirectory,
                concertAssembly,
                cplexAssembly,
                source);

        return true;
    }

    private static IEnumerable<string>
        EnumerateWindowsInstallations()
    {
        string programFiles =
            Environment.GetFolderPath(
                Environment.SpecialFolder.ProgramFiles);

        if (string.IsNullOrWhiteSpace(programFiles))
        {
            yield break;
        }

        string ilogRoot =
            Path.Combine(
                programFiles,
                "IBM",
                "ILOG");

        if (!Directory.Exists(ilogRoot))
        {
            yield break;
        }

        IEnumerable<string> directories;

        try
        {
            directories =
                Directory
                    .EnumerateDirectories(
                        ilogRoot,
                        "CPLEX_Studio*",
                        SearchOption.TopDirectoryOnly)
                    .OrderByDescending(
                        static directory =>
                            Path.GetFileName(directory),
                        StringComparer.OrdinalIgnoreCase)
                    .ToArray();
        }
        catch
        {
            yield break;
        }

        foreach (string directory in directories)
        {
            yield return directory;
        }
    }
}
