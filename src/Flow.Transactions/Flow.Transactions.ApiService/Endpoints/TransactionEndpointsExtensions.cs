using Flow.Transactions.Application.UseCases.Transactions.CreateTransaction;
using Flow.Transactions.Application.UseCases.Transactions.DeleteTransaction;
using Flow.Transactions.Application.UseCases.Transactions.GetTransactions;
using Flow.Transactions.Application.UseCases.Transactions.UpdateTransaction;

namespace Flow.Transactions.ApiService.Endpoints;

public static class TransactionEndpointsExtensions
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", () => "Transactions API is running.");

        endpoints.MapPost("/transactions", async (
            CreateTransactionRequest request,
            ICreateTransactionService service) =>
        {
            var tx = await service.ExecuteAsync(request);

            return Results.Created($"/transactions/{tx.Id}", tx);
        });

        endpoints.MapGet("/transactions", async (
            [AsParameters] GetTransactionsRequest request,
            IGetTransactionsService service) =>
        {
            return await service.ExecuteAsync(request);
        });

        endpoints.MapPut("/transactions/{id:guid}", async Task<IResult> (
            Guid id,
            UpdateTransactionRequest request,
            IUpdateTransactionService service) =>
        {
            var tx = await service.ExecuteAsync(id, request);

            if (tx is null)
                return Results.NotFound();

            return Results.Ok(tx);
        });

        endpoints.MapDelete("/transactions/{id:guid}", async Task<IResult> (
            [AsParameters] DeleteTransactionRequest request,
            IDeleteTransactionService service) =>
        {
            var deleted = await service.ExecuteAsync(request);

            if (!deleted)
                return Results.NotFound();

            return Results.NoContent();
        });

        return endpoints;
    }
}
