namespace TS24.DataPlatform.Tests;

using TS24.DataPlatform.Foundation.Concurrency;
using TS24.DataPlatform.Foundation.Entities;
using TS24.DataPlatform.Foundation.Identity;
using TS24.DataPlatform.Foundation.Persistence;
using TS24.DataPlatform.Foundation.Provenance;
using TS24.DataPlatform.Foundation.Temporal;
using Xunit;

public sealed class FoundationContractTests
{
    [Fact]
    public void EffectiveIntervalContainsStartButExcludesEnd()
    {
        var interval = new EffectiveDateInterval(new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1));

        Assert.True(interval.Contains(new DateOnly(2026, 1, 1)));
        Assert.True(interval.Contains(new DateOnly(2026, 1, 31)));
        Assert.False(interval.Contains(new DateOnly(2026, 2, 1)));
        Assert.False(interval.Contains(new DateOnly(2025, 12, 31)));
    }

    [Fact]
    public void OpenEndedIntervalContainsAllDatesFromStart()
    {
        var interval = new EffectiveDateInterval(new DateOnly(2026, 1, 1));

        Assert.True(interval.IsOpenEnded);
        Assert.True(interval.Contains(DateOnly.MaxValue));
    }

    [Theory]
    [InlineData(2026, 1, 1)]
    [InlineData(2025, 12, 31)]
    public void EffectiveIntervalRejectsNonPositiveDuration(int year, int month, int day)
    {
        Assert.Throws<ArgumentException>(() => new EffectiveDateInterval(
            new DateOnly(2026, 1, 1),
            new DateOnly(year, month, day)));
    }

    [Fact]
    public void LogicalVersionIsNonNegativeAndMonotonic()
    {
        Assert.Equal(new EntityVersion(1), EntityVersion.Initial.Next());
        Assert.Throws<ArgumentOutOfRangeException>(() => new EntityVersion(-1));
    }

    [Fact]
    public void ConcurrencyModelRepresentsRequiredOutcomes()
    {
        var version = new EntityVersion(3);

        Assert.Equal(ConcurrencyOutcome.Success, ConcurrencyResult.Succeeded(version).Outcome);
        Assert.Equal(ConcurrencyOutcome.Conflict, ConcurrencyResult.Conflict(version).Outcome);
        Assert.Equal(
            ConcurrencyOutcome.InvalidExpectedVersion,
            ConcurrencyResult.InvalidExpectedVersion(version).Outcome);
    }

    [Fact]
    public void ProvenanceKeepsActorAndCompanyContextAsDistinctAbstractions()
    {
        var actor = new TestActorId();
        var company = new TestCompanyContext();
        var correlation = new TestCorrelationId();
        var context = new ProvenanceContext(
            actor,
            "application",
            "module",
            company,
            DateTimeOffset.UtcNow,
            correlation,
            "update",
            "correction");

        Assert.Same(actor, context.ActorId);
        Assert.Same(company, context.CompanyContext);
        Assert.Same(correlation, context.CorrelationId);
    }

    [Fact]
    public void PersistenceCapabilitiesAreComposable()
    {
        IPersistenceCapabilities capabilities = new TestCapabilities(
            PersistenceCapability.Transactions | PersistenceCapability.Diagnostics);

        Assert.True(capabilities.Supports(PersistenceCapability.Transactions));
        Assert.False(capabilities.Supports(PersistenceCapability.Migrations));
    }

    private sealed record TestActorId : IActorIdentity;

    private sealed record TestCompanyContext : ICompanyContext;

    private sealed record TestCorrelationId : ICorrelationIdentity;

    private sealed record TestCapabilities(PersistenceCapability Supported) : IPersistenceCapabilities;
}
