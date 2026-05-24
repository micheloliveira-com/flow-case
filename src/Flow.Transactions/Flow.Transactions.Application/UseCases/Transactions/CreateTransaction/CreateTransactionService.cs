using System.Text.Json;
using Flow.Transactions.Infrastructure;
using RabbitMQ.Client;

public sealed class CreateTransactionService(
    TransactionDbContext db,
    IConnection connection)
    : ICreateTransactionService
{
    public async Task<Transaction> ExecuteAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var tx = new Transaction(
            amount: request.Amount,
            type: request.Type,
            date: request.Date,
            description: request.Description
        );

        db.Transactions.Add(tx);

        await db.SaveChangesAsync(cancellationToken);

        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "transaction-daily-recompute",
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "transaction-daily-recompute",
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: JsonSerializer.SerializeToUtf8Bytes(new
            {
                Date = tx.Date
            }));

        return tx;
    }
}