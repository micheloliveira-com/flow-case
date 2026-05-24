namespace Flow.Reports.Application.UseCases.TransactionDailyBalance.GetTransactionDailyBalance;

public sealed record GetTransactionDailyBalanceRequest(
    DateOnly? Start,
    DateOnly? End
);