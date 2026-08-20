namespace TS24.DataPlatform.Foundation.Identity;

/// <summary>
/// Marks a stable semantic identity without prescribing its physical representation.
/// </summary>
public interface ISemanticIdentity;

/// <summary>Identifies an actor without coupling it to an application user identity.</summary>
public interface IActorIdentity : ISemanticIdentity;

/// <summary>Identifies an optional company context without defining a Company entity.</summary>
public interface ICompanyContext : ISemanticIdentity;

/// <summary>Identifies a correlation context without prescribing an identifier format.</summary>
public interface ICorrelationIdentity : ISemanticIdentity;
