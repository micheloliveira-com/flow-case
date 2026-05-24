namespace Flow.Reports.Domain.Entities;

public class TransactionDailyBalance
{
    public Guid Id { get; private set; }

    public decimal Balance { get; private set; }

    public DateOnly Date { get; private set; }

    public DateTime ProcessedAt { get; private set; }

    private TransactionDailyBalance() { } // EF Core

    public TransactionDailyBalance(DateOnly date, decimal balance, DateTime processedAt)
    {
        Date = date;
        Balance = balance;
        ProcessedAt = processedAt;
    }

    public void Apply(decimal balance, DateTime processedAt)
    {
        Balance = balance;
        ProcessedAt = processedAt;
    }
}