namespace TS24.DataPlatform.Deployment;

public interface IDeploymentConnectionResolver
{
    ConnectionResolutionResult Resolve(string logicalStore);
}

/// <summary>Resolves a named logical store without fetching or exposing secret values.</summary>
public sealed class DeploymentConnectionResolver : IDeploymentConnectionResolver
{
    private readonly DeploymentConnectionConfiguration? configuration;
    private readonly DeploymentConfigurationValidationResult validation;

    public DeploymentConnectionResolver(DeploymentConnectionConfiguration? configuration)
    {
        this.configuration = configuration;
        validation = DeploymentConnectionConfigurationValidator.Validate(configuration);
    }

    public ConnectionResolutionResult Resolve(string logicalStore)
    {
        if (!validation.IsValid)
        {
            return new(ConnectionResolutionStatus.InvalidConfiguration, null, validation.Diagnostics);
        }

        if (string.IsNullOrWhiteSpace(logicalStore))
        {
            return Failure("store.request.invalid", "A logical store name is required.");
        }

        var store = configuration!.Stores.SingleOrDefault(
            candidate => string.Equals(candidate.LogicalStore, logicalStore.Trim(), StringComparison.OrdinalIgnoreCase));
        if (store is null)
        {
            return Failure("store.not-found", "The requested logical store is not configured.");
        }

        var connection = new ResolvedStoreConnection(
            store.LogicalStore.Trim(),
            store.Provider.Trim(),
            store.EndpointReference,
            store.SecretReference,
            store.Location.Trim(),
            configuration.Profile,
            store.ConnectivityMode);
        return new(ConnectionResolutionStatus.Resolved, connection, Array.Empty<ConnectionResolutionDiagnostic>());
    }

    private static ConnectionResolutionResult Failure(string code, string message) =>
        new(ConnectionResolutionStatus.StoreNotFound, null, [new(code, message)]);
}
