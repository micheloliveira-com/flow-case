using Flow.Reports.Application.UseCases.TransactionDailyBalance.GetTransactionDailyBalance;

namespace Flow.Reports.ApiService.Endpoints;

public static class ReportEndpointsExtensions
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", () => "Reports API is running.");

        endpoints.MapGet("/transaction_daily_balance", async (
            IGetTransactionDailyBalance service,
            [AsParameters] GetTransactionDailyBalanceRequest request) =>
        {
            return await service.ExecuteAsync(request);
        });

        return endpoints;
    }
}
