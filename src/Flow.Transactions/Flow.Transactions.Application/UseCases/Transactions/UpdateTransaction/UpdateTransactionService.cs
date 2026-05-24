

using System.Text.Json;
using Flow.Transactions.Infrastructure;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace Flow.Transactions.Application.UseCases.Transactions.UpdateTransaction;

public sealed class UpdateTransactionService(
    TransactionDbContext db,
    IConnection connection)
    : IUpdateTransactionService
{
    public async Task<Transaction?> ExecuteAsync(
        Guid id,
        UpdateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var tx = await db.Transactions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (tx is null)
            return null;

        var oldDate = tx.Date;

        var updated = new Transaction(
            id: id,
            amount: request.Amount,
            type: request.Type,
            date: request.Date,
            description: request.Description
        );

        db.Entry(tx).CurrentValues.SetValues(updated);

        await db.SaveChangesAsync(cancellationToken);

        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "transaction-daily-recompute",
            durable: true,
            exclusive: false,
            autoDelete: false);

        var affectedDates = new HashSet<DateOnly>
        {
            oldDate,
            request.Date
        };

        foreach (var date in affectedDates)
        {
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: "transaction-daily-recompute",
                mandatory: false,
                basicProperties: new BasicProperties(),
                body: JsonSerializer.SerializeToUtf8Bytes(new
                {
                    Date = date
                }));
        }

        return tx;
    }
}