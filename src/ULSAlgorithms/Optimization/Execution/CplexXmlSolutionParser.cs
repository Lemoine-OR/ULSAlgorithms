using System.Globalization;
using System.Xml.Linq;

namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// Parses CPLEX XML .sol files written by the stand-alone CPLEX optimizer.
/// </summary>
internal static class CplexXmlSolutionParser
{
    internal static CplexXmlSolution Parse(
        string path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            !File.Exists(path))
        {
            return new CplexXmlSolution(
                string.Empty,
                new Dictionary<int, double>());
        }

        XDocument document =
            XDocument.Load(
                path,
                LoadOptions.None);

        XElement? header =
            document
                .Descendants()
                .FirstOrDefault(
                    element =>
                        element.Name.LocalName ==
                        "header");

        string status =
            header?.Attribute(
                "solutionStatusString")?.Value ??
            string.Empty;

        var values =
            new Dictionary<int, double>();

        foreach (XElement variable in
                 document.Descendants().Where(
                     element =>
                         element.Name.LocalName ==
                         "variable"))
        {
            string name =
                variable.Attribute("name")?.Value ??
                string.Empty;

            if (!name.StartsWith(
                    "v_",
                    StringComparison.Ordinal) ||
                !int.TryParse(
                    name.AsSpan(2),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int id))
            {
                continue;
            }

            string valueText =
                variable.Attribute("value")?.Value ??
                string.Empty;

            if (double.TryParse(
                    valueText,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double value) &&
                double.IsFinite(value))
            {
                values[id] =
                    value;
            }
        }

        return new CplexXmlSolution(
            status,
            values);
    }
}

internal sealed record CplexXmlSolution(
    string Status,
    IReadOnlyDictionary<int, double> Values);
