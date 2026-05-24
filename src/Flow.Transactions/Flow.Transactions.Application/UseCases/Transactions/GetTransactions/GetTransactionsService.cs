

using Flow.Transactions.Infrastructure;
using Microsoft.EntityFrameworkCore;

public sealed class GetTransactionsService(
    TransactionDbContext db)
    : IGetTransactionsService
{
    public async Task<List<Transaction>> ExecuteAsync(
        GetTransactionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = db.Transactions.AsNoTracking();

        if (request.Start.HasValue)
            query = query.Where(x => x.Date >= request.Start.Value);

        if (request.End.HasValue)
            query = query.Where(x => x.Date <= request.End.Value);

        query = query.OrderByDescending(x => x.Date);

        return await query.ToListAsync(cancellationToken);
    }
}