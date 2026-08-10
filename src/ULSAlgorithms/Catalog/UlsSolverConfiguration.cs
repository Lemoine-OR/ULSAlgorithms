using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ULSAlgorithms.Abstractions;

namespace ULSAlgorithms.Catalog;

/// <summary>
/// Versioned, serializable definition of one ULS strategy and its
/// constructor-level options.
/// </summary>
/// <remarks>
/// <para>
/// The JSON schema is intentionally independent from the NuGet/package version.
/// Schema version 1 is the first public reproducibility format.
/// </para>
/// <para>
/// Parsing is strict: unknown JSON properties, integer enum values, unsupported
/// schema versions, unknown solver identifiers and incompatible strategy
/// options are rejected.
/// </para>
/// </remarks>
public sealed class UlsSolverConfiguration
{
    /// <summary>The first and current public JSON schema version.</summary>
    public const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions SerializerOptions =
        CreateSerializerOptions();

    /// <summary>Gets or sets the serialized schema version.</summary>
    [JsonRequired]
    public int SchemaVersion { get; set; } =
        CurrentSchemaVersion;

    /// <summary>Gets or sets the stable solver identifier.</summary>
    [JsonRequired]
    public string SolverId { get; set; } =
        string.Empty;

    /// <summary>
    /// Gets or sets the constructor-level strategy options.
    /// </summary>
    public UlsSolverCreationOptions Options { get; set; } =
        new();

    /// <summary>
    /// Validates the schema, stable solver ID and all strategy-specific
    /// options without constructing or solving a problem.
    /// </summary>
    public void Validate()
    {
        if (SchemaVersion != CurrentSchemaVersion)
        {
            throw new NotSupportedException(
                $"Unsupported ULS solver configuration schema version " +
                $"'{SchemaVersion}'. Supported version: " +
                $"{CurrentSchemaVersion}.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(SolverId);
        ArgumentNullException.ThrowIfNull(Options);

        var descriptor =
            UlsSolverCatalog.Get(SolverId);

        Options.EnsureValidFor(descriptor);
    }

    /// <summary>
    /// Creates a fresh solver from this validated configuration.
    /// </summary>
    /// <returns>The configured solver.</returns>
    public IUlsSolver CreateSolver() =>
        UlsSolverFactory.Create(this);

    /// <summary>
    /// Serializes this configuration to canonical, indented JSON.
    /// </summary>
    /// <returns>UTF-16 managed text containing UTF-8-compatible JSON.</returns>
    public string ToJson()
    {
        Validate();

        return JsonSerializer.Serialize(
            this,
            SerializerOptions);
    }

    /// <summary>
    /// Writes this configuration as UTF-8 without a byte-order mark.
    /// </summary>
    /// <param name="path">Destination JSON path.</param>
    public void SaveJson(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath =
            Path.GetFullPath(path);

        var directory =
            Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(
            fullPath,
            ToJson() + Environment.NewLine,
            new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false));
    }

    /// <summary>
    /// Parses and validates one JSON configuration.
    /// </summary>
    /// <param name="json">JSON text.</param>
    /// <returns>The validated configuration.</returns>
    public static UlsSolverConfiguration ParseJson(
        string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var configuration =
            JsonSerializer.Deserialize<UlsSolverConfiguration>(
                json,
                SerializerOptions)
            ?? throw new JsonException(
                "The JSON document did not contain a ULS solver configuration.");

        configuration.Validate();
        return configuration;
    }

    /// <summary>
    /// Loads and validates one UTF-8 JSON configuration file.
    /// </summary>
    /// <param name="path">Configuration file path.</param>
    /// <returns>The validated configuration.</returns>
    public static UlsSolverConfiguration LoadJson(
        string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return ParseJson(
            File.ReadAllText(
                Path.GetFullPath(path),
                Encoding.UTF8));
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options =
            new JsonSerializerOptions
            {
                PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition =
                    JsonIgnoreCondition.WhenWritingNull,
                UnmappedMemberHandling =
                    JsonUnmappedMemberHandling.Disallow,
                PropertyNameCaseInsensitive = false
            };

        options.Converters.Add(
            new JsonStringEnumConverter(
                JsonNamingPolicy.CamelCase,
                allowIntegerValues: false));

        return options;
    }
}
