namespace TS24.DataPlatform.Provider.MariaDb.Migrations;

/// <summary>Describes one immutable, module-owned MariaDB schema migration.</summary>
public abstract class MariaDbMigration
{
    protected MariaDbMigration(long version, string name, string checksum, MigrationTransactionMode transactionMode = MigrationTransactionMode.Preferred)
    {
        Identity = new MigrationIdentity(version, name);
        Checksum = string.IsNullOrWhiteSpace(checksum)
            ? throw new ArgumentException("A migration checksum is required.", nameof(checksum))
            : checksum.Trim();
        TransactionMode = transactionMode;
    }

    public MigrationIdentity Identity { get; }

    public string Checksum { get; }

    public MigrationTransactionMode TransactionMode { get; }

    public abstract ValueTask ExecuteAsync(IMariaDbMigrationCommandContext context, CancellationToken cancellationToken);
}

public readonly record struct MigrationIdentity
{
    public MigrationIdentity(long version, string name)
    {
        if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version), "Migration versions must be positive.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A migration name is required.", nameof(name));

        Version = version;
        Name = name.Trim();
    }

    public long Version { get; }

    public string Name { get; }

    public override string ToString() => $"{Version:D12}_{Name}";
}

public enum MigrationTransactionMode
{
    /// <summary>Use a transaction when the connection and MariaDB operation permit it.</summary>
    Preferred,

    /// <summary>Fail before execution if a transaction is unavailable.</summary>
    Required,

    /// <summary>Execute without a transaction, for operations MariaDB implicitly commits.</summary>
    None,
}
