using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Flow.Web.Blazor.Clients;

namespace Flow.Web.Blazor.Components.Pages;

public partial class Transaction
{
    [Inject]
    private TransactionApiClient TransactionsApi { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private Flow.Web.Blazor.Clients.Transaction[]? transactions;
    private bool isLoading = true;
    private string? errorMessage;

    private bool isModalOpen;
    private bool isEditing;

    private TransactionEditModel formModel = new();

    private DateOnly? filterStart;
    private DateOnly? filterEnd;

    protected override async Task OnInitializedAsync()
    {
        await Load();
    }

    private async Task Load()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            transactions = await TransactionsApi.GetTransactionsAsync(filterStart, filterEnd);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OpenCreate()
    {
        isModalOpen = true;
        isEditing = false;
        formModel = CreateDefaultFormModel();
    }

    private void StartEdit(Flow.Web.Blazor.Clients.Transaction tx)
    {
        isModalOpen = true;
        isEditing = true;

        formModel = new TransactionEditModel
        {
            Id = tx.Id,
            Amount = tx.Amount,
            Type = tx.Type,
            Date = tx.Date,
            Description = tx.Description
        };
    }

    private void CloseModal()
    {
        isModalOpen = false;
        isEditing = false;
        formModel = CreateDefaultFormModel();
    }

    private async Task SaveForm()
    {
        errorMessage = null;

        try
        {
            var input = new Flow.Web.Blazor.Clients.Transaction(
                formModel.Id,
                formModel.Amount,
                formModel.Type,
                formModel.Date,
                formModel.Description);

            if (isEditing)
            {
                await TransactionsApi.UpdateAsync(input.Id, input);
            }
            else
            {
                await TransactionsApi.CreateAsync(input with { Id = Guid.Empty });
            }

            CloseModal();
            await Load();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task ConfirmDelete(Guid id)
    {
        var ok = await JS.InvokeAsync<bool>("confirm",
            $"Delete transaction {id}?");

        if (!ok)
            return;

        try
        {
            errorMessage = null;
            await TransactionsApi.DeleteAsync(id);
            await Load();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private static TransactionEditModel CreateDefaultFormModel()
    {
        return new TransactionEditModel
        {
            Amount = 0,
            Type = TransactionType.Credit,
            Date = DateOnly.FromDateTime(DateTime.Now),
            Description = string.Empty
        };
    }

    private sealed class TransactionEditModel
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public DateOnly Date { get; set; }
        public string? Description { get; set; }
    }
}
