namespace TS24.DataPlatform.Provider.MariaDb.Migrations;

public sealed class MigrationPlan
{
    private MigrationPlan(IReadOnlyList<MariaDbMigration> ordered, IReadOnlyList<MariaDbMigration> pending)
    {
        Ordered = ordered;
        Pending = pending;
    }

    public IReadOnlyList<MariaDbMigration> Ordered { get; }

    public IReadOnlyList<MariaDbMigration> Pending { get; }

    public static MigrationPlan Create(IEnumerable<MariaDbMigration> discovered, IEnumerable<AppliedMigration> applied)
    {
        ArgumentNullException.ThrowIfNull(discovered);
        ArgumentNullException.ThrowIfNull(applied);

        var ordered = discovered.OrderBy(migration => migration.Identity.Version).ToArray();
        var duplicate = ordered.GroupBy(migration => migration.Identity.Version).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new MigrationValidationException($"Duplicate migration version {duplicate.Key} was discovered.");
        }

        var appliedOrdered = applied.OrderBy(migration => migration.Identity.Version).ToArray();
        var duplicateApplied = appliedOrdered.GroupBy(migration => migration.Identity.Version).FirstOrDefault(group => group.Count() > 1);
        if (duplicateApplied is not null)
        {
            throw new MigrationValidationException($"The migration ledger contains duplicate version {duplicateApplied.Key}.");
        }

        var discoveredByVersion = ordered.ToDictionary(migration => migration.Identity.Version);
        foreach (var entry in appliedOrdered)
        {
            if (!discoveredByVersion.TryGetValue(entry.Identity.Version, out var migration))
            {
                throw new MigrationValidationException($"Applied migration {entry.Identity} is not present in this deployment.");
            }

            if (migration.Identity.Name != entry.Identity.Name ||
                !string.Equals(migration.Checksum, entry.Checksum, StringComparison.Ordinal))
            {
                throw new MigrationValidationException($"Checksum or identity drift was detected for migration version {entry.Identity.Version}.");
            }
        }

        var appliedVersions = appliedOrdered.Select(entry => entry.Identity.Version).ToHashSet();
        var highestApplied = appliedOrdered.LastOrDefault()?.Identity.Version;
        if (highestApplied is not null && ordered.Any(migration => migration.Identity.Version < highestApplied && !appliedVersions.Contains(migration.Identity.Version)))
        {
            throw new MigrationValidationException("The migration ledger has an order gap; an older migration cannot run after a newer migration.");
        }

        return new MigrationPlan(ordered, ordered.Where(migration => !appliedVersions.Contains(migration.Identity.Version)).ToArray());
    }
}

public sealed class MigrationValidationException : Exception
{
    public MigrationValidationException(string message) : base(message) { }
}
