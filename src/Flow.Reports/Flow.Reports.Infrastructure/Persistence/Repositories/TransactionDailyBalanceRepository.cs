using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Flow.Reports.Domain.Entities;
using Flow.Reports.Application.Abstractions.Persistence;

namespace Flow.Reports.Infrastructure.Persistence.Repositories;

public sealed class TransactionDailyBalanceRepository(
    ReportsDbContext db
) : ITransactionDailyBalanceRepository
{
    public async Task<List<TransactionDailyBalance>> GetAsync(
        DateOnly? start,
        DateOnly? end)
    {
        var query = db.TransactionDailyBalance.AsNoTracking();
        query = query.Where(x => x.Balance != 0);

        if (start.HasValue)
            query = query.Where(x => x.Date >= start.Value);

        if (end.HasValue)
            query = query.Where(x => x.Date <= end.Value);

        return await query
            .OrderByDescending(x => x.Date)
            .ToListAsync();
    }

    public async Task<TransactionDailyBalance?> GetByDateAsync(
        DateOnly date)
    {
        return await db.TransactionDailyBalance
            .FirstOrDefaultAsync(x => x.Date == date);
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }

    public async Task AddAsync(
        TransactionDailyBalance entity)
    {
        await db.TransactionDailyBalance.AddAsync(entity);
    }
}