using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Activity.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberActivitiesEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CardUid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsManual = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberActivities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberActivities_CardUid",
                table: "MemberActivities",
                column: "CardUid");

            migrationBuilder.CreateIndex(
                name: "IX_MemberActivities_CheckInTime",
                table: "MemberActivities",
                column: "CheckInTime");

            migrationBuilder.CreateIndex(
                name: "IX_MemberActivities_MemberId",
                table: "MemberActivities",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberActivities");
        }
    }
}
