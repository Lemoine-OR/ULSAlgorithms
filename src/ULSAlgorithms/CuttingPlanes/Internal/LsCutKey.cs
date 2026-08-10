using ULSAlgorithms.CuttingPlanes;

namespace ULSAlgorithms.CuttingPlanes.Internal;

internal static class LsCutKey
{
    internal static string Create(
        LsCutDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return definition.L.ToString(
                   System.Globalization.CultureInfo.InvariantCulture) +
               ":" +
               string.Join(
                   ",",
                   definition.S);
    }
}
