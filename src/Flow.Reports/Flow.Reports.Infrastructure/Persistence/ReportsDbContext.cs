using Microsoft.EntityFrameworkCore;
using Flow.Reports.Domain.Entities;

namespace Flow.Reports.Infrastructure.Persistence;

public class ReportsDbContext : DbContext
{
    public DbSet<TransactionDailyBalance> TransactionDailyBalance => Set<TransactionDailyBalance>();

    public ReportsDbContext(
        DbContextOptions<ReportsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ReportsDbContext).Assembly);
    }
}