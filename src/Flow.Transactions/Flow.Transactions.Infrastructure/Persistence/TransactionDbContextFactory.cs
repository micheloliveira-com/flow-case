using Flow.Transactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Flow.Transactions.Infrastructure.Persistence;

public class TransactionDbContextFactory
    : IDesignTimeDbContextFactory<TransactionDbContext>
{
    public TransactionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<TransactionDbContext>();

        optionsBuilder.UseNpgsql();

        return new TransactionDbContext(optionsBuilder.Options);
    }
}