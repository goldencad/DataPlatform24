namespace TS24.DataPlatform.Tests;

using TS24.DataPlatform.Provider.MariaDb.Migrations;
using Xunit;

public sealed class MariaDbMigrationInfrastructureTests
{
    [Fact]
    public async Task DiscoversPendingMigrationsInVersionOrderAndSkipsAppliedOnRepeat()
    {
        var session = new FixtureSession();
        var runner = Runner(session, Migration(20), Migration(10), Migration(30));

        var first = await runner.RunAsync();
        var second = await runner.RunAsync();

        Assert.Equal([10L, 20L, 30L], session.ExecutedVersions);
        Assert.Equal(3, first.AppliedCount);
        Assert.Empty(second.Applied);
        Assert.Equal(2, session.LockCount);
    }

    [Fact]
    public async Task DryValidationFindsPendingWithoutExecuting()
    {
        var session = new FixtureSession();
        session.Applied.Add(Applied(10));
        var runner = Runner(session, Migration(20), Migration(10));

        var plan = await runner.ValidateAsync();

        Assert.Equal([20L], plan.Pending.Select(item => item.Identity.Version));
        Assert.Empty(session.ExecutedVersions);
    }

    [Fact]
    public async Task FailureIdentifiesMigrationAndDoesNotRecordIt()
    {
        var session = new FixtureSession();
        var runner = Runner(session, Migration(10), Migration(20, fails: true), Migration(30));

        var error = await Assert.ThrowsAsync<MigrationExecutionException>(() => runner.RunAsync().AsTask());

        Assert.Equal(20, error.FailedMigration.Version);
        Assert.Equal([10L], error.Completed.Select(item => item.Version));
        Assert.Equal([10L, 20L], session.ExecutedVersions);
        Assert.Equal([10L], session.Applied.Select(item => item.Identity.Version));
    }

    [Fact]
    public void RejectsDuplicateVersionsChecksumDriftAndOrderGaps()
    {
        Assert.Throws<MigrationValidationException>(() => MigrationPlan.Create([Migration(10), Migration(10)], []));
        Assert.Throws<MigrationValidationException>(() => MigrationPlan.Create([Migration(10)], [Applied(10, "changed")]));
        Assert.Throws<MigrationValidationException>(() => MigrationPlan.Create(
            [Migration(10), Migration(20)],
            [Applied(20)]));
    }

    [Fact]
    public async Task UsesTransactionsOnlyWhenPermitted()
    {
        var transactional = new FixtureSession { SupportsTransactions = true };
        await Runner(transactional, Migration(10), Migration(20, mode: MigrationTransactionMode.None)).RunAsync();
        Assert.Equal(1, transactional.TransactionCount);
        Assert.Equal(1, transactional.CommitCount);

        var unavailable = new FixtureSession();
        var error = await Assert.ThrowsAsync<MigrationExecutionException>(
            () => Runner(unavailable, Migration(10, mode: MigrationTransactionMode.Required)).RunAsync().AsTask());
        Assert.IsType<InvalidOperationException>(error.InnerException);
        Assert.Empty(unavailable.ExecutedVersions);
    }

    private static MariaDbMigrationRunner Runner(FixtureSession session, params TestMigration[] migrations) =>
        new(new FixtureSessionFactory(session), migrations);

    private static TestMigration Migration(long version, bool fails = false, MigrationTransactionMode mode = MigrationTransactionMode.Preferred) =>
        new(version, $"migration_{version}", $"checksum-{version}", fails, mode);

    private static AppliedMigration Applied(long version, string? checksum = null) =>
        new(new MigrationIdentity(version, $"migration_{version}"), checksum ?? $"checksum-{version}", DateTimeOffset.UnixEpoch);

    private sealed class TestMigration : MariaDbMigration
    {
        private readonly bool fails;

        public TestMigration(long version, string name, string checksum, bool fails, MigrationTransactionMode mode)
            : base(version, name, checksum, mode) => this.fails = fails;

        public override async ValueTask ExecuteAsync(IMariaDbMigrationCommandContext context, CancellationToken cancellationToken) =>
            await context.ExecuteAsync(
                new MigrationCommand(fails ? $"FAIL {Identity.Version}" : $"APPLY {Identity.Version}"),
                cancellationToken);
    }

    private sealed class FixtureSessionFactory(FixtureSession session) : IMariaDbMigrationSessionFactory
    {
        public ValueTask<IMariaDbMigrationSession> OpenSessionAsync(CancellationToken cancellationToken) => ValueTask.FromResult<IMariaDbMigrationSession>(session);
    }

    private sealed class FixtureSession : IMariaDbMigrationSession, IMariaDbMigrationCommandContext, IMariaDbMigrationTransaction
    {
        public bool SupportsTransactions { get; init; }
        public List<AppliedMigration> Applied { get; } = [];
        public List<long> ExecutedVersions { get; } = [];
        public int TransactionCount { get; private set; }
        public int CommitCount { get; private set; }
        public int LockCount { get; private set; }
        public IMariaDbMigrationCommandContext Commands => this;

        public ValueTask EnsureOwnedLedgerAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;
        public ValueTask<IAsyncDisposable> AcquireDeploymentLockAsync(CancellationToken cancellationToken)
        {
            LockCount++;
            return ValueTask.FromResult<IAsyncDisposable>(new NoOpLease());
        }

        public ValueTask<IReadOnlyList<AppliedMigration>> ReadAppliedMigrationsAsync(CancellationToken cancellationToken) =>
            ValueTask.FromResult<IReadOnlyList<AppliedMigration>>(Applied.ToArray());

        public ValueTask<IMariaDbMigrationTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            TransactionCount++;
            return ValueTask.FromResult<IMariaDbMigrationTransaction>(this);
        }

        public ValueTask<int> ExecuteAsync(MigrationCommand command, CancellationToken cancellationToken)
        {
            var version = long.Parse(command.Sql.Split(' ')[1], System.Globalization.CultureInfo.InvariantCulture);
            ExecutedVersions.Add(version);
            if (command.Sql.StartsWith("FAIL", StringComparison.Ordinal)) throw new InvalidOperationException("fixture failure");
            return ValueTask.FromResult(1);
        }

        public ValueTask RecordAppliedMigrationAsync(AppliedMigration migration, CancellationToken cancellationToken)
        {
            Applied.Add(migration);
            return ValueTask.CompletedTask;
        }

        public ValueTask CommitAsync(CancellationToken cancellationToken)
        {
            CommitCount++;
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        private sealed class NoOpLease : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }
}
