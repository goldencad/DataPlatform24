namespace TS24.DataPlatform.Deployment;

/// <summary>Identifies a supported hosting profile without implying connectivity.</summary>
public enum DeploymentProfile
{
    Local,
    OnPremise,
    CustomerCloud,
    Ts24Cloud,
    Cloud24Private,
    Hybrid,
}

/// <summary>Describes how a runtime can reach a store independently of its hosting profile.</summary>
public enum ConnectivityMode
{
    Direct,
    PrivateNetwork,
    ManagedGateway,
    Offline,
}
