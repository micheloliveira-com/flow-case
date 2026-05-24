
using Flow.Reports.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flow.Reports.Infrastructure.Persistence.Configuration;

public sealed class TransactionDailyBalanceConfiguration : IEntityTypeConfiguration<TransactionDailyBalance>
{
    public void Configure(EntityTypeBuilder<TransactionDailyBalance> builder)
    {
        builder.ToTable("transaction_daily_balance");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Balance)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .IsRequired();

        builder.HasIndex(x => x.Date)
            .IsUnique();

        builder.HasIndex(x => x.ProcessedAt);
    }
}
