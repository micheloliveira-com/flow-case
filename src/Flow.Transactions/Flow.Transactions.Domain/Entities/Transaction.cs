public class Transaction
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public DateOnly Date { get; set; }

    public string? Description { get; set; }
}