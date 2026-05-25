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
        SetDate(date);
        SetBalance(balance);
        SetProcessedAt(processedAt);
    }

    public void Apply(decimal balance, DateTime processedAt)
    {
        SetProcessedAt(processedAt);
        SetBalance(balance);
    }

    private static void Guard(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private void SetDate(DateOnly date)
    {
        Guard(date != default, "Invalid date");
        Date = date;
    }

    private void SetBalance(decimal balance)
    {
        Balance = balance;
    }

    private void SetProcessedAt(DateTime processedAt)
    {
        Guard(processedAt != default, "Invalid processed date");
        ProcessedAt = processedAt;
    }
}
