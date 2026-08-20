namespace TS24.DataPlatform.Foundation.Provenance;

using TS24.DataPlatform.Foundation.Identity;

/// <summary>Describes mutation provenance; it is not a physical audit-column model.</summary>
public sealed record ProvenanceContext(
    IActorIdentity ActorId,
    string SourceApplication,
    string SourceModule,
    ICompanyContext? CompanyContext,
    DateTimeOffset Timestamp,
    ICorrelationIdentity CorrelationId,
    string? Operation = null,
    string? Reason = null);
