namespace TS24.DataPlatform.Foundation.Concurrency;

using TS24.DataPlatform.Foundation.Entities;

public enum ConcurrencyOutcome
{
    Success,
    Conflict,
    InvalidExpectedVersion,
}

public sealed record ConcurrencyResult
{
    private ConcurrencyResult(
        ConcurrencyOutcome outcome,
        EntityVersion? currentVersion,
        string? message)
    {
        Outcome = outcome;
        CurrentVersion = currentVersion;
        Message = message;
    }

    public ConcurrencyOutcome Outcome { get; }

    public EntityVersion? CurrentVersion { get; }

    public string? Message { get; }

    public bool IsSuccess => Outcome is ConcurrencyOutcome.Success;

    public static ConcurrencyResult Succeeded(EntityVersion currentVersion) =>
        new(ConcurrencyOutcome.Success, currentVersion, null);

    public static ConcurrencyResult Conflict(EntityVersion currentVersion, string? message = null) =>
        new(ConcurrencyOutcome.Conflict, currentVersion, message);

    public static ConcurrencyResult InvalidExpectedVersion(
        EntityVersion? currentVersion = null,
        string? message = null) =>
        new(ConcurrencyOutcome.InvalidExpectedVersion, currentVersion, message);
}

public readonly record struct ConcurrencyExpectation(EntityVersion ExpectedVersion);
