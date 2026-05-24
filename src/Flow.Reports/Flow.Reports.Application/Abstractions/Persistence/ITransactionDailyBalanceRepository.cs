using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flow.Reports.Domain.Entities;

public interface ITransactionDailyBalanceRepository
{
    Task<List<TransactionDailyBalance>> GetAsync(
        DateOnly? start,
        DateOnly? end,
        CancellationToken cancellationToken = default);

    Task<TransactionDailyBalance?> GetByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TransactionDailyBalance entity,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        CancellationToken cancellationToken = default);
}