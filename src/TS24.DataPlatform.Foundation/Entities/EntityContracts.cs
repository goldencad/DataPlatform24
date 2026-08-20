namespace TS24.DataPlatform.Foundation.Entities;

using TS24.DataPlatform.Foundation.Identity;

public interface IEntity<out TIdentity>
    where TIdentity : ISemanticIdentity
{
    TIdentity Id { get; }

    EntityVersion Version { get; }
}

/// <summary>A technology-neutral logical version used for optimistic concurrency.</summary>
public readonly record struct EntityVersion
{
    public static EntityVersion Initial { get; } = new(0);

    public EntityVersion(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        Value = value;
    }

    public long Value { get; }

    public EntityVersion Next() => new(checked(Value + 1));
}
