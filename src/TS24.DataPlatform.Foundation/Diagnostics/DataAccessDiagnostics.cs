namespace TS24.DataPlatform.Foundation.Diagnostics;

using TS24.DataPlatform.Foundation.Identity;

public enum DataAccessOutcome
{
    Succeeded,
    Failed,
    Cancelled,
}

public sealed record DataAccessDiagnostic(
    string StoreName,
    string Operation,
    DateTimeOffset Timestamp,
    TimeSpan Duration,
    DataAccessOutcome Outcome,
    ICorrelationIdentity? CorrelationId = null,
    string? Detail = null);

public interface IDataAccessDiagnostics
{
    void Record(DataAccessDiagnostic diagnostic);
}
