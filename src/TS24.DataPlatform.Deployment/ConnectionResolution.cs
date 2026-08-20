namespace TS24.DataPlatform.Deployment;

public enum ConnectionResolutionStatus
{
    Resolved,
    InvalidConfiguration,
    StoreNotFound,
}

public sealed record ConnectionResolutionDiagnostic(string Code, string Message);

/// <summary>
/// A safe descriptor containing configuration references only. Secret material and provider-specific
/// connection strings are deliberately resolved later by the runtime composition root.
/// </summary>
public sealed record ResolvedStoreConnection(
    string LogicalStore,
    string Provider,
    string EndpointReference,
    string? SecretReference,
    string Location,
    DeploymentProfile DeploymentProfile,
    ConnectivityMode ConnectivityMode)
{
    public override string ToString() =>
        $"Store={LogicalStore}; Provider={Provider}; Location={Location}; " +
        $"Profile={DeploymentProfile}; Connectivity={ConnectivityMode}; Endpoint=<reference>; Secret=<redacted>";
}

public sealed record ConnectionResolutionResult(
    ConnectionResolutionStatus Status,
    ResolvedStoreConnection? Connection,
    IReadOnlyCollection<ConnectionResolutionDiagnostic> Diagnostics)
{
    public bool IsResolved => Status == ConnectionResolutionStatus.Resolved;
}
