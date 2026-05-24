public sealed record GetTransactionsRequest(
    DateOnly? Start,
    DateOnly? End
);