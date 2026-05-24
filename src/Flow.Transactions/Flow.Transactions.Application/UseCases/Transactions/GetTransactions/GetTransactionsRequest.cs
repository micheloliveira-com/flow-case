namespace Flow.Transactions.Application.UseCases.Transactions.GetTransactions;

public sealed record GetTransactionsRequest(
    DateOnly? Start,
    DateOnly? End
);