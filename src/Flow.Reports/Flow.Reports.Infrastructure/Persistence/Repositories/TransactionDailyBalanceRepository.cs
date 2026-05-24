

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

        if (start.HasValue)
            query = query.Where(x => x.Date >= start.Value);

        if (end.HasValue)
            query = query.Where(x => x.Date <= end.Value);

        return await query
            .OrderByDescending(x => x.Date)
            .ToListAsync(cancellationToken);
    }
}