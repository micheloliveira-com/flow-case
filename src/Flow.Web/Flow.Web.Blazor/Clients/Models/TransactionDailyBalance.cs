
namespace Flow.Web.Blazor.Clients.Models;
public record TransactionDailyBalance(
    Guid Id,
    decimal Balance,
    DateOnly Date,
    DateTime CreatedAtUtc);