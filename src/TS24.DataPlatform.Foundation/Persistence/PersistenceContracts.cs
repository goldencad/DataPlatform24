namespace TS24.DataPlatform.Foundation.Persistence;

[Flags]
public enum PersistenceCapability
{
    None = 0,
    Transactions = 1 << 0,
    OptimisticConcurrency = 1 << 1,
    EffectiveDating = 1 << 2,
    Migrations = 1 << 3,
    Diagnostics = 1 << 4,
}

public interface IPersistenceCapabilities
{
    PersistenceCapability Supported { get; }

    bool Supports(PersistenceCapability capability) => (Supported & capability) == capability;
}
