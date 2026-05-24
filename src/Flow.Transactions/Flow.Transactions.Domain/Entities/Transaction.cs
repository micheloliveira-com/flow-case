public class Transaction
{
    public Guid Id { get; private set; }

    public decimal Amount { get; private set; }

    public TransactionType Type { get; private set; }

    public DateOnly Date { get; private set; }

    public string? Description { get; private set; }

    private Transaction() { } // Entity Framework requires a parameterless constructor

    public Transaction(decimal amount, TransactionType type, DateOnly date, string? description)
    {
        SetAmount(amount);
        SetType(type);
        SetDate(date);
        SetDescription(description);
    }

    public Transaction(Guid id, decimal amount, TransactionType type, DateOnly date, string? description)
        : this(amount, type, date, description)
    {
        Id = id;
    }

    private static void Guard(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private void SetAmount(decimal amount)
    {
        Guard(amount != 0, "Amount must not be zero");
        Amount = amount;
    }

    private void SetDescription(string? description)
    {
        Guard(description is null || description.Length <= 255, "Description too long");
        Description = description;
    }

    private void SetType(TransactionType type)
    {
        Guard(Enum.IsDefined(type), "Invalid transaction type");
        Type = type;
    }

    private void SetDate(DateOnly date) => Date = date;
}