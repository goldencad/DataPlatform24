namespace TS24.DataPlatform.Foundation.Temporal;

/// <summary>A start-inclusive, end-exclusive effective-date interval.</summary>
public readonly record struct EffectiveDateInterval
{
    public EffectiveDateInterval(DateOnly start, DateOnly? end = null)
    {
        if (end is not null && end <= start)
        {
            throw new ArgumentException("The end date must be later than the start date.", nameof(end));
        }

        Start = start;
        End = end;
    }

    public DateOnly Start { get; }

    public DateOnly? End { get; }

    public bool IsOpenEnded => End is null;

    public bool Contains(DateOnly date) => date >= Start && (End is null || date < End);
}
