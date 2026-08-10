using System.Globalization;
using System.Text.RegularExpressions;

namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// Parses solver text solutions using portable variable names v_&lt;id&gt;.
/// </summary>
public static partial class NamedSolutionValueParser
{
    /// <summary>Parses a text solution file.</summary>
    public static IReadOnlyDictionary<int, double> ParseFile(
        string path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            !File.Exists(path))
        {
            return new Dictionary<int, double>();
        }

        return ParseLines(
            File.ReadLines(path));
    }

    /// <summary>Parses portable variable/value pairs from text lines.</summary>
    public static IReadOnlyDictionary<int, double> ParseLines(
        IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var values =
            new Dictionary<int, double>();

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            Match nameMatch =
                VariableNameRegex().Match(line);

            if (!nameMatch.Success ||
                !int.TryParse(
                    nameMatch.Groups[1].Value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int variableId))
            {
                continue;
            }

            string suffix =
                line[
                    (nameMatch.Index +
                     nameMatch.Length)..];

            Match numberMatch =
                NumberRegex().Match(suffix);

            if (!numberMatch.Success ||
                !double.TryParse(
                    numberMatch.Value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double value) ||
                !double.IsFinite(value))
            {
                continue;
            }

            values[variableId] =
                value;
        }

        return values;
    }

    [GeneratedRegex(
        @"\bv_(\d+)\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex VariableNameRegex();

    [GeneratedRegex(
        @"[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?",
        RegexOptions.CultureInvariant)]
    private static partial Regex NumberRegex();
}
