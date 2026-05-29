using Flow.Web.Blazor.Models.Enums;

namespace Flow.Web.Blazor.Models;

public sealed class TransactionEditModel
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public TransactionTypeEnum Type { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
}
