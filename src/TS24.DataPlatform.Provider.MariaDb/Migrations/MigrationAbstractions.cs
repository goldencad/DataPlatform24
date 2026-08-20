namespace TS24.DataPlatform.Provider.MariaDb.Migrations;

/// <summary>
/// Resolves a migration session from deployment configuration. Implementations own connection strings,
/// credentials, topology, and driver details; the migration runner never does.
/// </summary>
public interface IMariaDbMigrationSessionFactory
{
    ValueTask<IMariaDbMigrationSession> OpenSessionAsync(CancellationToken cancellationToken);
}

public interface IMariaDbMigrationSession : IAsyncDisposable
{
    bool SupportsTransactions { get; }

    ValueTask EnsureOwnedLedgerAsync(CancellationToken cancellationToken);

    ValueTask<IAsyncDisposable> AcquireDeploymentLockAsync(CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<AppliedMigration>> ReadAppliedMigrationsAsync(CancellationToken cancellationToken);

    ValueTask<IMariaDbMigrationTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    IMariaDbMigrationCommandContext Commands { get; }

    ValueTask RecordAppliedMigrationAsync(AppliedMigration migration, CancellationToken cancellationToken);
}

/// <summary>A deliberately small command seam; implementations must bind parameters rather than interpolate values.</summary>
public interface IMariaDbMigrationCommandContext
{
    ValueTask<int> ExecuteAsync(MigrationCommand command, CancellationToken cancellationToken);
}

public sealed record MigrationCommand(string Sql, IReadOnlyDictionary<string, object?>? Parameters = null)
{
    public string Sql { get; } = string.IsNullOrWhiteSpace(Sql)
        ? throw new ArgumentException("Migration SQL is required.", nameof(Sql))
        : Sql;
}

public interface IMariaDbMigrationTransaction : IAsyncDisposable
{
    ValueTask CommitAsync(CancellationToken cancellationToken);
}

public sealed record AppliedMigration(MigrationIdentity Identity, string Checksum, DateTimeOffset AppliedAtUtc)
{
    public string Checksum { get; } = string.IsNullOrWhiteSpace(Checksum)
        ? throw new ArgumentException("An applied migration checksum is required.", nameof(Checksum))
        : Checksum;
}
