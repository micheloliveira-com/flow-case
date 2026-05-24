using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Flow.Reports.Domain.Entities;

namespace Flow.Reports.Infrastructure.Persistence.Repositories;

public sealed class TransactionDailyBalanceRepository(
    ReportsDbContext db
) : ITransactionDailyBalanceRepository
{
    public async Task<List<TransactionDailyBalance>> GetAsync(
        DateOnly? start,
        DateOnly? end,
        CancellationToken cancellationToken = default)
    {
        var query = db.TransactionDailyBalance.AsNoTracking();
        query = query.Where(x => x.Balance != 0);

        if (start.HasValue)
            query = query.Where(x => x.Date >= start.Value);

        if (end.HasValue)
            query = query.Where(x => x.Date <= end.Value);

        return await query
            .OrderByDescending(x => x.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<TransactionDailyBalance?> GetByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await db.TransactionDailyBalance
            .FirstOrDefaultAsync(x => x.Date == date, cancellationToken);
    }

    public async Task SaveAsync(
        CancellationToken cancellationToken = default)
    {
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(
        TransactionDailyBalance entity,
        CancellationToken cancellationToken = default)
    {
        await db.TransactionDailyBalance.AddAsync(entity, cancellationToken);
    }
}