using System.ComponentModel.DataAnnotations.Schema;

namespace Flow.Reports.Domain.Entities;

public class TransactionDailyBalance
{
    public Guid Id { get; set; }

    public decimal Balance { get; set; }

    public DateOnly Date { get; set; }

    public DateTime ProcessedAt { get; set; }
}