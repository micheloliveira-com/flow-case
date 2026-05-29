
namespace Flow.Web.Blazor.Models;
public record TransactionDailyBalanceModel(
    Guid Id,
    decimal Balance,
    DateOnly Date,
    DateTime CreatedAtUtc);