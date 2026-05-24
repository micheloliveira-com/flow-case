using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flow.Reports.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transaction_daily_balance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_daily_balance", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_daily_balance_Date",
                table: "transaction_daily_balance",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transaction_daily_balance_ProcessedAt",
                table: "transaction_daily_balance",
                column: "ProcessedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transaction_daily_balance");
        }
    }
}
