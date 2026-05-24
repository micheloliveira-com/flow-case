using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Flow.Reports.Infrastructure;

public class ReportsDbContextFactory
    : IDesignTimeDbContextFactory<ReportsDbContext>
{
    public ReportsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<ReportsDbContext>();

        optionsBuilder.UseNpgsql();

        return new ReportsDbContext(optionsBuilder.Options);
    }
}