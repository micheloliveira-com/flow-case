

public sealed record GetTransactionDailyBalanceRequest(
    DateOnly? Start,
    DateOnly? End
);