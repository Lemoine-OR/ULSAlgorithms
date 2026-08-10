using System.Reflection;
using System.Runtime.Loader;

namespace ULSAlgorithms.Optimization.Adapters.Xpress;

/// <summary>
/// Locates and loads the optional FICO Xpress Optimizer managed assembly.
/// </summary>
public static class XpressRuntimeLocator
{
    /// <summary>
    /// Resolves the Optimizer.dll path without loading it.
    /// </summary>
    public static string ResolveAssemblyPath()
    {
        string[] explicitVariables =
        [
            "ULSALGORITHMS_XPRESS_OPTIMIZER_ASSEMBLY",
            "LOTSIZING_XPRESS_OPTIMIZER_ASSEMBLY"
        ];

        foreach (string variableName in explicitVariables)
        {
            string explicitPath =
                Environment.GetEnvironmentVariable(
                    variableName)?
                    .Trim()
                    .Trim('"') ??
                string.Empty;

            if (File.Exists(explicitPath))
            {
                return Path.GetFullPath(explicitPath);
            }
        }

        string xpressDirectory =
            Environment.GetEnvironmentVariable(
                "XPRESSDIR")?.Trim() ??
            string.Empty;

        if (!string.IsNullOrWhiteSpace(xpressDirectory))
        {
            string[] candidates =
            [
                Path.Combine(
                    xpressDirectory,
                    "bin",
                    "Optimizer.dll"),
                Path.Combine(
                    xpressDirectory,
                    "lib",
                    "Optimizer.dll"),
                Path.Combine(
                    xpressDirectory,
                    "bin",
                    "dotnet",
                    "Optimizer.dll"),
                Path.Combine(
                    xpressDirectory,
                    "lib",
                    "dotnet",
                    "Optimizer.dll"),
                Path.Combine(
                    xpressDirectory,
                    "Optimizer.dll")
            ];

            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Loads an already loaded Optimizer assembly, an explicitly located
    /// assembly, or an assembly resolvable by normal .NET probing.
    /// </summary>
    public static Assembly? TryLoad(
        out string resolvedPath)
    {
        resolvedPath = string.Empty;

        Assembly? loaded =
            AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(
                    assembly =>
                        string.Equals(
                            assembly.GetName().Name,
                            "Optimizer",
                            StringComparison.OrdinalIgnoreCase));

        if (loaded is not null)
        {
            resolvedPath =
                string.IsNullOrWhiteSpace(loaded.Location)
                    ? "already loaded"
                    : loaded.Location;

            return loaded;
        }

        string path =
            ResolveAssemblyPath();

        if (!string.IsNullOrWhiteSpace(path))
        {
            resolvedPath = path;

            return AssemblyLoadContext.Default
                .LoadFromAssemblyPath(path);
        }

        try
        {
            Assembly assembly =
                Assembly.Load("Optimizer");

            resolvedPath =
                string.IsNullOrWhiteSpace(assembly.Location)
                    ? "assembly probing"
                    : assembly.Location;

            return assembly;
        }
        catch
        {
            return null;
        }
    }
}
