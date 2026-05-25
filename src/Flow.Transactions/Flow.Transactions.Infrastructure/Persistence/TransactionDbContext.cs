using Flow.Transactions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Flow.Transactions.Infrastructure.Persistence;

public class TransactionDbContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    public TransactionDbContext(
        DbContextOptions<TransactionDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TransactionDbContext).Assembly);
    }
}