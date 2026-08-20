namespace TS24.DataPlatform.Foundation.Connections;

/// <summary>
/// A deployment-neutral reference to externally supplied connection configuration.
/// It deliberately contains neither credentials nor a provider-specific connection string.
/// </summary>
public sealed record DeploymentConnectionDescriptor(
    string Name,
    string ConfigurationKey,
    string? DeploymentScope = null);

public interface IConnectionDescriptorResolver
{
    ValueTask<DeploymentConnectionDescriptor?> ResolveAsync(
        string name,
        CancellationToken cancellationToken = default);
}
