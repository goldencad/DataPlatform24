namespace TS24.DataPlatform.Tests;

using TS24.DataPlatform.Deployment;
using Xunit;

public sealed class DeploymentConnectionResolutionTests
{
    public static TheoryData<DeploymentProfile> Profiles => new()
    {
        DeploymentProfile.Local,
        DeploymentProfile.OnPremise,
        DeploymentProfile.CustomerCloud,
        DeploymentProfile.Ts24Cloud,
        DeploymentProfile.Cloud24Private,
        DeploymentProfile.Hybrid,
    };

    [Theory]
    [MemberData(nameof(Profiles))]
    public void ResolvesNamedStoreForEveryDeploymentProfile(DeploymentProfile profile)
    {
        var resolver = new DeploymentConnectionResolver(Configuration(profile));

        var result = resolver.Resolve("MASTERDATA");

        Assert.True(result.IsResolved);
        Assert.Equal(profile, result.Connection!.DeploymentProfile);
        Assert.Equal("MariaDb", result.Connection.Provider);
        Assert.Equal("configuration:endpoints:masterdata", result.Connection.EndpointReference);
        Assert.Equal("vault:runtime/masterdata", result.Connection.SecretReference);
    }

    [Fact]
    public void SelectsProviderAndConnectivityByLogicalStoreNotProfile()
    {
        var configuration = new DeploymentConnectionConfiguration(
            DeploymentProfile.Hybrid,
            [
                Store("masterdata", "MariaDb", "on-premises", ConnectivityMode.PrivateNetwork),
                Store("application-documents", "MongoDb", "customer-cloud", ConnectivityMode.ManagedGateway),
            ]);

        var connection = new DeploymentConnectionResolver(configuration).Resolve("application-documents").Connection!;

        Assert.Equal("MongoDb", connection.Provider);
        Assert.Equal("customer-cloud", connection.Location);
        Assert.Equal(ConnectivityMode.ManagedGateway, connection.ConnectivityMode);
    }

    [Fact]
    public void MissingStoreFailsClosedWithoutEchoingRequestedValue()
    {
        const string sensitiveInput = "password=do-not-log";

        var result = new DeploymentConnectionResolver(Configuration(DeploymentProfile.Local)).Resolve(sensitiveInput);

        Assert.Equal(ConnectionResolutionStatus.StoreNotFound, result.Status);
        Assert.Null(result.Connection);
        Assert.DoesNotContain(sensitiveInput, string.Join(' ', result.Diagnostics.Select(item => item.Message)));
    }

    [Fact]
    public void InvalidConfigurationFailsClosedAndDoesNotExposeValues()
    {
        const string secret = "super-secret-password";
        var configuration = new DeploymentConnectionConfiguration(
            (DeploymentProfile)999,
            [new("masterdata", "MariaDb", $"Server=db;Password={secret}", $"password={secret}", "", (ConnectivityMode)999)]);

        var result = new DeploymentConnectionResolver(configuration).Resolve("masterdata");
        var diagnostics = string.Join(' ', result.Diagnostics.Select(item => $"{item.Code}:{item.Message}"));

        Assert.Equal(ConnectionResolutionStatus.InvalidConfiguration, result.Status);
        Assert.Null(result.Connection);
        Assert.DoesNotContain(secret, diagnostics);
        Assert.Contains("profile.invalid", diagnostics);
        Assert.Contains("store.endpoint-reference.invalid", diagnostics);
        Assert.Contains("store.secret-reference.invalid", diagnostics);
        Assert.Contains("store.location.missing", diagnostics);
        Assert.Contains("store.connectivity.invalid", diagnostics);
    }

    [Fact]
    public void DuplicateLogicalStoresAreRejectedCaseInsensitively()
    {
        var configuration = new DeploymentConnectionConfiguration(
            DeploymentProfile.OnPremise,
            [Store("masterdata", "MariaDb", "site-a", ConnectivityMode.Direct), Store("MasterData", "MongoDb", "site-b", ConnectivityMode.Direct)]);

        var result = new DeploymentConnectionResolver(configuration).Resolve("masterdata");

        Assert.Equal(ConnectionResolutionStatus.InvalidConfiguration, result.Status);
        Assert.Contains(result.Diagnostics, item => item.Code == "store.name.duplicate");
    }

    [Fact]
    public void DescriptorDiagnosticsRedactEndpointAndSecretReferences()
    {
        var descriptor = new DeploymentConnectionResolver(Configuration(DeploymentProfile.CustomerCloud))
            .Resolve("masterdata").Connection!;

        var diagnosticText = descriptor.ToString();

        Assert.DoesNotContain(descriptor.EndpointReference, diagnosticText);
        Assert.DoesNotContain(descriptor.SecretReference!, diagnosticText);
        Assert.Contains("Endpoint=<reference>", diagnosticText);
        Assert.Contains("Secret=<redacted>", diagnosticText);
    }

    [Fact]
    public void NullAndEmptyConfigurationFailClosed()
    {
        var missing = new DeploymentConnectionResolver(null).Resolve("masterdata");
        var empty = new DeploymentConnectionResolver(new(DeploymentProfile.Local, [])).Resolve("masterdata");

        Assert.Equal(ConnectionResolutionStatus.InvalidConfiguration, missing.Status);
        Assert.Equal(ConnectionResolutionStatus.InvalidConfiguration, empty.Status);
        Assert.Contains(missing.Diagnostics, item => item.Code == "configuration.missing");
        Assert.Contains(empty.Diagnostics, item => item.Code == "stores.missing");
    }

    private static DeploymentConnectionConfiguration Configuration(DeploymentProfile profile) =>
        new(profile, [Store("masterdata", "MariaDb", "configured-location", ConnectivityMode.Direct)]);

    private static LogicalStoreConnectionConfiguration Store(
        string name,
        string provider,
        string location,
        ConnectivityMode connectivityMode) =>
        new(name, provider, $"configuration:endpoints:{name}", $"vault:runtime/{name}", location, connectivityMode);
}
