using Flow.Web.Blazor.Clients;
using Flow.Web.Blazor.Clients.Models;

namespace Flow.Web.Blazor.Components.Pages.Models;

public sealed class TransactionEditModel
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
}
