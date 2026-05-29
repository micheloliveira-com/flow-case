namespace Flow.Web.Blazor.Clients.Models;
public record Transaction(
    Guid Id,
    decimal Amount,
    TransactionType Type,
    DateOnly Date,
    string? Description);