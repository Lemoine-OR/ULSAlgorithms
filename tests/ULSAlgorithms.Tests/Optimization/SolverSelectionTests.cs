using ULSAlgorithms.Optimization;
using Xunit;

namespace ULSAlgorithms.Tests.Optimization;

public sealed class SolverSelectionTests
{
    [Fact]
    public void DefaultPriority_MatchesLotSizingDataModelOrder()
    {
        var options = new SolverSelectionOptions();

        Assert.Equal(
            [
                SolverKind.Cplex,
                SolverKind.Gurobi,
                SolverKind.Xpress,
                SolverKind.CoinOrCbc
            ],
            options.SolverPriority);
    }

    [Fact]
    public async Task Automatic_SkipsUnavailableSolverAndSelectsNextAsync()
    {
        var registry = new SolverAdapterRegistry();
        registry.Register(
            new FakeAdapter(
                SolverKind.Cplex,
                SolverAvailabilityStatus.LicenseUnavailable));
        registry.Register(
            new FakeAdapter(
                SolverKind.Gurobi,
                SolverAvailabilityStatus.Available));

        var service = new SolverSelectionService();

        SolverSelectionResult result =
            await service.SelectAsync(
                SolverKind.Automatic,
                registry,
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSelected);
        Assert.Equal(SolverKind.Gurobi, result.SelectedSolver);
        Assert.Contains(
            result.Diagnostics,
            message => message.Contains(
                "Cplex",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Automatic_RespectsRequiredCapabilitiesAsync()
    {
        var registry = new SolverAdapterRegistry();
        registry.Register(
            new FakeAdapter(
                SolverKind.Cplex,
                SolverAvailabilityStatus.Available,
                SolverCapability.MixedIntegerLinearProgramming));
        registry.Register(
            new FakeAdapter(
                SolverKind.Gurobi,
                SolverAvailabilityStatus.Available,
                SolverCapability.MixedIntegerLinearProgramming,
                SolverCapability.UserCutCallbacks));

        var options = new SolverSelectionOptions();
        options.RequiredCapabilities.Add(SolverCapability.UserCutCallbacks);

        var service = new SolverSelectionService();

        SolverSelectionResult result =
            await service.SelectAsync(
                SolverKind.Automatic,
                registry,
                options,
                TestContext.Current.CancellationToken);

        Assert.True(result.IsSelected);
        Assert.Equal(SolverKind.Gurobi, result.SelectedSolver);
    }

    [Fact]
    public async Task ExplicitExactSolver_DoesNotFallbackAsync()
    {
        var registry = new SolverAdapterRegistry();
        registry.Register(
            new FakeAdapter(
                SolverKind.Cplex,
                SolverAvailabilityStatus.NotInstalled));
        registry.Register(
            new FakeAdapter(
                SolverKind.Gurobi,
                SolverAvailabilityStatus.Available));

        var options = new SolverSelectionOptions
        {
            RequireExactSolverKind = true
        };

        var service = new SolverSelectionService();

        SolverSelectionResult result =
            await service.SelectAsync(
                SolverKind.Cplex,
                registry,
                options,
                TestContext.Current.CancellationToken);

        Assert.False(result.IsSelected);
        Assert.Equal(SolverKind.Unknown, result.SelectedSolver);
    }

    [Fact]
    public async Task LimitedAvailability_IsSelectableByDefaultAsync()
    {
        var registry = new SolverAdapterRegistry();
        registry.Register(
            new FakeAdapter(
                SolverKind.Cplex,
                SolverAvailabilityStatus.AvailableWithLimitations));

        var service = new SolverSelectionService();

        SolverSelectionResult result =
            await service.SelectAsync(
                SolverKind.Automatic,
                registry,
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSelected);
        Assert.Equal(SolverKind.Cplex, result.SelectedSolver);
    }

    [Fact]
    public async Task Selection_CanBeCancelledAsync()
    {
        var registry = new SolverAdapterRegistry();
        registry.Register(
            new FakeAdapter(
                SolverKind.Cplex,
                SolverAvailabilityStatus.Available));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var service = new SolverSelectionService();

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () =>
                await service.SelectAsync(
                    SolverKind.Automatic,
                    registry,
                    cancellationToken: cts.Token));
    }

    private sealed class FakeAdapter : IOptimizationSolverAdapter
    {
        private readonly SolverAvailabilityStatus _status;
        private readonly SolverCapability[] _capabilities;

        public FakeAdapter(
            SolverKind solverKind,
            SolverAvailabilityStatus status,
            params SolverCapability[] capabilities)
        {
            SolverKind = solverKind;
            _status = status;
            _capabilities = capabilities;
        }

        public string AdapterId => $"fake-{SolverKind}";

        public string AdapterName => $"Fake {SolverKind}";

        public string AdapterVersion => "test";

        public SolverKind SolverKind { get; }

        public IReadOnlyCollection<SolverCapability> Capabilities =>
            _capabilities;

        public bool SupportsCapability(SolverCapability capability) =>
            _capabilities.Contains(capability);

        public ValueTask<SolverAvailabilityInfo> CheckAvailabilityAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return ValueTask.FromResult(
                new SolverAvailabilityInfo(
                    SolverKind,
                    _status,
                    solverName: SolverKind.ToString(),
                    solverVersion: "test"));
        }
    }
}
