namespace TS24.DataPlatform.Deployment;

public sealed record LogicalStoreConnectionConfiguration(
    string LogicalStore,
    string Provider,
    string EndpointReference,
    string? SecretReference,
    string Location,
    ConnectivityMode ConnectivityMode);

public sealed record DeploymentConnectionConfiguration(
    DeploymentProfile Profile,
    IReadOnlyCollection<LogicalStoreConnectionConfiguration> Stores);

public sealed record DeploymentConfigurationValidationResult(
    bool IsValid,
    IReadOnlyCollection<ConnectionResolutionDiagnostic> Diagnostics);

public static class DeploymentConnectionConfigurationValidator
{
    public static DeploymentConfigurationValidationResult Validate(DeploymentConnectionConfiguration? configuration)
    {
        var diagnostics = new List<ConnectionResolutionDiagnostic>();
        if (configuration is null)
        {
            diagnostics.Add(Diagnostic("configuration.missing", "Deployment connection configuration is missing."));
            return new(false, diagnostics);
        }

        if (!Enum.IsDefined(configuration.Profile))
        {
            diagnostics.Add(Diagnostic("profile.invalid", "Deployment profile is invalid."));
        }

        if (configuration.Stores is null || configuration.Stores.Count == 0)
        {
            diagnostics.Add(Diagnostic("stores.missing", "At least one logical store must be configured."));
            return new(false, diagnostics);
        }

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var store in configuration.Stores)
        {
            if (store is null)
            {
                diagnostics.Add(Diagnostic("store.invalid", "A logical store configuration is invalid."));
                continue;
            }

            ValidateRequired(store.LogicalStore, "store.name.missing", "Logical store name is required.", diagnostics);
            ValidateRequired(store.Provider, "store.provider.missing", "Store provider is required.", diagnostics);
            ValidateReference(store.EndpointReference, "store.endpoint-reference.invalid", "Endpoint reference is invalid.", diagnostics);
            if (store.SecretReference is not null)
            {
                ValidateReference(store.SecretReference, "store.secret-reference.invalid", "Secret reference is invalid.", diagnostics);
            }

            ValidateRequired(store.Location, "store.location.missing", "Store location is required.", diagnostics);
            if (!Enum.IsDefined(store.ConnectivityMode))
            {
                diagnostics.Add(Diagnostic("store.connectivity.invalid", "Store connectivity mode is invalid."));
            }

            if (!string.IsNullOrWhiteSpace(store.LogicalStore) && !names.Add(store.LogicalStore.Trim()))
            {
                diagnostics.Add(Diagnostic("store.name.duplicate", "Logical store names must be unique."));
            }
        }

        return new(diagnostics.Count == 0, diagnostics);
    }

    private static void ValidateRequired(
        string? value,
        string code,
        string message,
        List<ConnectionResolutionDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value)) diagnostics.Add(Diagnostic(code, message));
    }

    private static void ValidateReference(
        string? value,
        string code,
        string message,
        List<ConnectionResolutionDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 256 || value.Any(char.IsWhiteSpace) || value.Contains('='))
        {
            diagnostics.Add(Diagnostic(code, message));
        }
    }

    private static ConnectionResolutionDiagnostic Diagnostic(string code, string message) => new(code, message);
}
