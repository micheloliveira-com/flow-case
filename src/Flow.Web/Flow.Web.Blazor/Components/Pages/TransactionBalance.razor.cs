using Microsoft.AspNetCore.Components;
using Flow.Web.Blazor.Clients;

namespace Flow.Web.Blazor.Components.Pages;

public partial class TransactionBalance
{
    [Inject]
    private TransactionBalanceApiClient TransactionBalanceApi { get; set; } = default!;

    private List<TransactionDailyBalance> dailyBalances = [];
    private bool isLoading = true;
    private string? errorMessage;

    private DateOnly? filterStart;
    private DateOnly? filterEnd;

    protected override async Task OnInitializedAsync()
    {
        await Load();
    }

    private async Task Load()
    {
        try
        {
            isLoading = true;
            errorMessage = null;

            dailyBalances = (await TransactionBalanceApi
                .GetTransactionDailyBalancesAsync(filterStart, filterEnd))
                .ToList();
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
}
