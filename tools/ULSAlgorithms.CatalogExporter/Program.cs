using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using ULSAlgorithms.Catalog;

namespace ULSAlgorithms.CatalogExporter;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

    public static int Main(string[] args)
    {
        var json = BuildCatalogJson();

        if (args.Length == 0)
        {
            Console.Write(json);
            return 0;
        }

        if (args.Length != 2)
        {
            return Usage();
        }

        var mode = args[0];
        var path = Path.GetFullPath(args[1]);

        switch (mode)
        {
            case "--write":
                Directory.CreateDirectory(
                    Path.GetDirectoryName(path) ??
                    throw new InvalidOperationException(
                        "Catalog output path has no parent directory."));

                File.WriteAllText(
                    path,
                    json,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

                Console.WriteLine(
                    $"Solver catalog projection written: {path}");
                return 0;

            case "--check":
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine(
                        $"Solver catalog projection is missing: {path}");
                    return 2;
                }

                var actual = Normalize(
                    File.ReadAllText(path, Encoding.UTF8));

                var expected = Normalize(json);

                if (!string.Equals(
                        actual,
                        expected,
                        StringComparison.Ordinal))
                {
                    Console.Error.WriteLine(
                        "docs/algorithm-catalog.json is not synchronized " +
                        "with UlsSolverCatalog.");
                    Console.Error.WriteLine(
                        "Regenerate it with:");
                    Console.Error.WriteLine(
                        "  dotnet run -c Release --project " +
                        "tools/ULSAlgorithms.CatalogExporter/" +
                        "ULSAlgorithms.CatalogExporter.csproj -- " +
                        "--write docs/algorithm-catalog.json");
                    return 3;
                }

                Console.WriteLine(
                    $"Solver catalog projection validated: {path}");
                return 0;

            default:
                return Usage();
        }
    }

    private static string BuildCatalogJson()
    {
        var document = new CatalogDocument(
            SchemaVersion: 2,
            Project: "ULSAlgorithms",
            GeneratedFrom: "ULSAlgorithms.Catalog.UlsSolverCatalog",
            Exact: UlsSolverCatalog.Exact
                .Select(ToEntry)
                .ToArray(),
            Heuristics: UlsSolverCatalog.Heuristics
                .Select(ToEntry)
                .ToArray());

        return JsonSerializer.Serialize(
                   document,
                   JsonOptions) +
               "\n";
    }

    private static CatalogEntry ToEntry(
        UlsSolverDescriptor descriptor) =>
        new(
            Id: descriptor.Id,
            Name: descriptor.Name,
            Class: descriptor.ImplementationType.Name,
            Category: CategoryName(descriptor.Category),
            Kind: descriptor.Kind == global::ULSAlgorithms.Abstractions.UlsSolverKind.Exact
                ? "exact"
                : "heuristic",
            Family: descriptor.Family,
            Time: descriptor.TimeComplexity,
            Space: descriptor.SpaceComplexity,
            Applicability: descriptor.Applicability,
            RequiresExternalSolver: descriptor.RequiresExternalSolver,
            SourcePath: descriptor.SourcePath,
            Publication: descriptor.ScientificReference,
            Doi: descriptor.Doi,
            Implementation: descriptor.Implementation);

    private static string CategoryName(
        UlsSolverCategory category) =>
        category switch
        {
            UlsSolverCategory.DirectExact => "direct-exact",
            UlsSolverCategory.OptimizationFormulation => "optimization",
            UlsSolverCategory.CuttingPlane => "cutting-plane",
            UlsSolverCategory.Heuristic => "heuristic",
            _ => throw new ArgumentOutOfRangeException(
                nameof(category),
                category,
                "Unknown solver category.")
        };

    private static string Normalize(string value) =>
        value
            .TrimStart('\uFEFF')
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');

    private static int Usage()
    {
        Console.Error.WriteLine(
            "Usage: ULSAlgorithms.CatalogExporter " +
            "[--write <path> | --check <path>]");
        return 1;
    }

    private sealed record CatalogDocument(
        int SchemaVersion,
        string Project,
        string GeneratedFrom,
        CatalogEntry[] Exact,
        CatalogEntry[] Heuristics);

    private sealed record CatalogEntry(
        string Id,
        string Name,
        [property: JsonPropertyName("class")] string Class,
        string Category,
        string Kind,
        string Family,
        string Time,
        string Space,
        string Applicability,
        bool RequiresExternalSolver,
        string SourcePath,
        string Publication,
        string Doi,
        string Implementation);
}
