using Flow.Web.Blazor.Models.Enums;

namespace Flow.Web.Blazor.Models;
public record TransactionModel(
    Guid Id,
    decimal Amount,
    TransactionTypeEnum Type,
    DateOnly Date,
    string? Description);