using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Aggregation.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggregatedStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TotalMembers = table.Column<int>(type: "integer", nullable: false),
                    TodayCheckIns = table.Column<int>(type: "integer", nullable: false),
                    MonthlyRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ActiveSubscriptions = table.Column<int>(type: "integer", nullable: false),
                    TotalPayments = table.Column<int>(type: "integer", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalUsers = table.Column<int>(type: "integer", nullable: false),
                    TotalRegistrations = table.Column<int>(type: "integer", nullable: false),
                    TotalSubscriptions = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregatedStats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyBreakdowns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Revenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NewMembers = table.Column<int>(type: "integer", nullable: false),
                    TotalSessions = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyBreakdowns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlansBreakdowns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanName = table.Column<string>(type: "text", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlansBreakdowns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyBreakdowns_Year_Month",
                table: "MonthlyBreakdowns",
                columns: new[] { "Year", "Month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggregatedStats");

            migrationBuilder.DropTable(
                name: "MonthlyBreakdowns");

            migrationBuilder.DropTable(
                name: "PlansBreakdowns");
        }
    }
}
