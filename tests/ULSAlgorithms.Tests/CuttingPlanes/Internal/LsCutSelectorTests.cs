using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.CuttingPlanes.Internal;
using ULSAlgorithms.CuttingPlanes.Separation;
using Xunit;

namespace ULSAlgorithms.Tests.CuttingPlanes.Internal;

public sealed class LsCutSelectorTests
{
    [Fact]
    public void TopByViolation_SelectsLargestViolations()
    {
        IReadOnlyList<LsSeparatedCut> candidates =
        [
            Create(0, 1.0, 0.1),
            Create(1, 4.0, 0.2),
            Create(2, 2.0, 0.8)
        ];

        var options =
            new LsCuttingPlaneOptions
            {
                SelectionPolicy =
                    CutSelectionPolicy.TopByViolation,
                MaximumCutsPerIteration =
                    2
            };

        HashSet<int> selected =
            LsCutSelector.Select(
                candidates,
                [0, 1, 2],
                options);

        Assert.Equal(
            [1, 2],
            selected.OrderBy(
                static index =>
                    index));
    }

    [Fact]
    public void TopByEfficacy_SelectsLargestEfficacies()
    {
        IReadOnlyList<LsSeparatedCut> candidates =
        [
            Create(0, 5.0, 0.1),
            Create(1, 2.0, 0.9),
            Create(2, 4.0, 0.5)
        ];

        var options =
            new LsCuttingPlaneOptions
            {
                SelectionPolicy =
                    CutSelectionPolicy.TopByEfficacy,
                MaximumCutsPerIteration =
                    1
            };

        HashSet<int> selected =
            LsCutSelector.Select(
                candidates,
                [0, 1, 2],
                options);

        Assert.Equal(
            [1],
            selected);
    }

    [Fact]
    public void MostViolatedPerL_SelectsOneCandidatePerL()
    {
        IReadOnlyList<LsSeparatedCut> candidates =
        [
            Create(2, 1.0, 0.1),
            Create(2, 3.0, 0.2),
            Create(3, 2.0, 0.3),
            Create(3, 1.0, 0.9)
        ];

        var options =
            new LsCuttingPlaneOptions
            {
                SelectionPolicy =
                    CutSelectionPolicy.MostViolatedPerL
            };

        HashSet<int> selected =
            LsCutSelector.Select(
                candidates,
                [0, 1, 2, 3],
                options);

        Assert.Equal(
            [1, 2],
            selected.OrderBy(
                static index =>
                    index));
    }

    private static LsSeparatedCut Create(
        int l,
        double violation,
        double efficacy)
    {
        return new LsSeparatedCut(
            new LsCutDefinition(
                l,
                [],
                [
                    new CutCoefficient(
                        $"y[{l}]",
                        1.0)
                ],
                LinearConstraintSense.GreaterOrEqual,
                1.0),
            violation,
            efficacy);
    }
}
