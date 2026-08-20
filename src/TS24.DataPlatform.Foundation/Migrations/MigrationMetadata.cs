namespace TS24.DataPlatform.Foundation.Migrations;

/// <summary>Describes a migration without containing executable migration or schema logic.</summary>
public sealed record MigrationMetadata(
    string Id,
    string Version,
    string Description,
    bool IsRepeatable = false,
    string? Checksum = null,
    string? DeploymentScope = null);
