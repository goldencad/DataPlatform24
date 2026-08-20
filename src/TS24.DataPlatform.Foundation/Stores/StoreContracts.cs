namespace TS24.DataPlatform.Foundation.Stores;

using TS24.DataPlatform.Foundation.Persistence;

public sealed record StoreDescriptor(
    string Name,
    string OwningModule,
    string OwningDomain,
    IReadOnlyCollection<string> ApprovedWriters,
    IReadOnlyCollection<string> ApprovedReaders,
    string MigrationAuthority,
    string BackupResponsibility,
    string RestoreResponsibility,
    PersistenceCapability Capabilities);

public interface IStoreRegistration
{
    StoreDescriptor Descriptor { get; }
}

public interface IStoreRegistry
{
    IReadOnlyCollection<IStoreRegistration> Registrations { get; }
}
