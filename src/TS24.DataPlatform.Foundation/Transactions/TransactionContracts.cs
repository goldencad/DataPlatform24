namespace TS24.DataPlatform.Foundation.Transactions;

/// <summary>A transaction scoped to one owning authority.</summary>
public interface IAuthorityTransaction : IAsyncDisposable
{
    ValueTask CommitAsync(CancellationToken cancellationToken = default);

    ValueTask RollbackAsync(CancellationToken cancellationToken = default);
}

public interface ITransactionBoundary
{
    ValueTask<IAuthorityTransaction> BeginAsync(CancellationToken cancellationToken = default);
}
