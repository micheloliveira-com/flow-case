using System.Text.Json;
using Flow.Transactions.Infrastructure;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

public sealed class DeleteTransactionService(
    TransactionDbContext db,
    IConnection connection)
    : IDeleteTransactionService
{
    public async Task<bool> ExecuteAsync(
        DeleteTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var tx = await db.Transactions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (tx is null)
            return false;

        var date = tx.Date;

        db.Transactions.Remove(tx);

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
                Date = date
            }));

        return true;
    }
}
