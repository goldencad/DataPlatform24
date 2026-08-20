namespace TS24.DataPlatform.Provider.MariaDb.Migrations;

public sealed class MariaDbMigrationRunner
{
    private readonly IMariaDbMigrationSessionFactory sessionFactory;
    private readonly IReadOnlyList<MariaDbMigration> migrations;
    private readonly TimeProvider timeProvider;

    public MariaDbMigrationRunner(
        IMariaDbMigrationSessionFactory sessionFactory,
        IEnumerable<MariaDbMigration> migrations,
        TimeProvider? timeProvider = null)
    {
        this.sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        ArgumentNullException.ThrowIfNull(migrations);
        this.migrations = migrations.ToArray();
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>Validates discovery and ledger state without executing migrations.</summary>
    public async ValueTask<MigrationPlan> ValidateAsync(CancellationToken cancellationToken = default)
    {
        await using var session = await sessionFactory.OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        await session.EnsureOwnedLedgerAsync(cancellationToken).ConfigureAwait(false);
        await using var deploymentLock = await session.AcquireDeploymentLockAsync(cancellationToken).ConfigureAwait(false);
        var applied = await session.ReadAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false);
        return MigrationPlan.Create(migrations, applied);
    }

    public async ValueTask<MigrationRunResult> RunAsync(CancellationToken cancellationToken = default)
    {
        await using var session = await sessionFactory.OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        await session.EnsureOwnedLedgerAsync(cancellationToken).ConfigureAwait(false);
        await using var deploymentLock = await session.AcquireDeploymentLockAsync(cancellationToken).ConfigureAwait(false);
        var plan = MigrationPlan.Create(migrations, await session.ReadAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false));
        var completed = new List<MigrationIdentity>();

        foreach (var migration in plan.Pending)
        {
            try
            {
                await ExecuteOneAsync(session, migration, cancellationToken).ConfigureAwait(false);
                completed.Add(migration.Identity);
            }
            catch (Exception exception) when (exception is not OperationCanceledException && exception is not MigrationExecutionException)
            {
                throw new MigrationExecutionException(migration.Identity, completed, exception);
            }
        }

        return new MigrationRunResult(plan.Ordered.Count, completed);
    }

    private async ValueTask ExecuteOneAsync(
        IMariaDbMigrationSession session,
        MariaDbMigration migration,
        CancellationToken cancellationToken)
    {
        if (migration.TransactionMode == MigrationTransactionMode.Required && !session.SupportsTransactions)
        {
            throw new InvalidOperationException($"Migration {migration.Identity} requires transaction support.");
        }

        var useTransaction = session.SupportsTransactions && migration.TransactionMode != MigrationTransactionMode.None;
        await using var transaction = useTransaction
            ? await session.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
            : null;

        await migration.ExecuteAsync(session.Commands, cancellationToken).ConfigureAwait(false);
        await session.RecordAppliedMigrationAsync(
            new AppliedMigration(migration.Identity, migration.Checksum, timeProvider.GetUtcNow()),
            cancellationToken).ConfigureAwait(false);

        if (transaction is not null) await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}

public sealed record MigrationRunResult(int DiscoveredCount, IReadOnlyList<MigrationIdentity> Applied)
{
    public int AppliedCount => Applied.Count;
}

public sealed class MigrationExecutionException : Exception
{
    public MigrationExecutionException(MigrationIdentity failedMigration, IReadOnlyList<MigrationIdentity> completed, Exception innerException)
        : base($"MariaDB migration {failedMigration} failed. Restore or roll forward before retrying.", innerException)
    {
        FailedMigration = failedMigration;
        Completed = completed.ToArray();
    }

    public MigrationIdentity FailedMigration { get; }

    public IReadOnlyList<MigrationIdentity> Completed { get; }
}
