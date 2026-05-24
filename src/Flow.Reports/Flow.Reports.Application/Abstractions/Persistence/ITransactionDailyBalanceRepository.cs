using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flow.Reports.Domain.Entities;

public interface ITransactionDailyBalanceRepository
{
    Task<List<TransactionDailyBalance>> GetAsync(
        DateOnly? start,
        DateOnly? end);

    Task<TransactionDailyBalance?> GetByDateAsync(
        DateOnly date);

    Task AddAsync(
        TransactionDailyBalance entity);

    Task SaveAsync();
}