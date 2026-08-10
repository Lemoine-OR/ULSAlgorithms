using ULSAlgorithms.CuttingPlanes.Separation;

namespace ULSAlgorithms.CuttingPlanes.Internal;

internal static class LsCutSelector
{
    internal static HashSet<int> Select(
        IReadOnlyList<LsSeparatedCut> candidates,
        IReadOnlyCollection<int> eligibleIndices,
        LsCuttingPlaneOptions options)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(eligibleIndices);
        ArgumentNullException.ThrowIfNull(options);

        options.EnsureValid();

        IEnumerable<int> eligible =
            eligibleIndices;

        IEnumerable<int> selected =
            options.SelectionPolicy switch
            {
                CutSelectionPolicy.AllViolated =>
                    eligible,

                CutSelectionPolicy.MostViolatedPerL =>
                    eligible
                        .GroupBy(
                            index =>
                                candidates[index].Definition.L)
                        .Select(
                            group =>
                                group
                                    .OrderByDescending(
                                        index =>
                                            candidates[index].Violation)
                                    .ThenByDescending(
                                        index =>
                                            candidates[index].Efficacy)
                                    .ThenBy(
                                        static index =>
                                            index)
                                    .First()),

                CutSelectionPolicy.TopByViolation =>
                    eligible
                        .OrderByDescending(
                            index =>
                                candidates[index].Violation)
                        .ThenByDescending(
                            index =>
                                candidates[index].Efficacy)
                        .ThenBy(
                            static index =>
                                index)
                        .Take(
                            options.MaximumCutsPerIteration),

                CutSelectionPolicy.TopByEfficacy =>
                    eligible
                        .OrderByDescending(
                            index =>
                                candidates[index].Efficacy)
                        .ThenByDescending(
                            index =>
                                candidates[index].Violation)
                        .ThenBy(
                            static index =>
                                index)
                        .Take(
                            options.MaximumCutsPerIteration),

                _ =>
                    throw new NotSupportedException(
                        $"Unsupported cut-selection policy " +
                        $"'{options.SelectionPolicy}'.")
            };

        return selected.ToHashSet();
    }
}
