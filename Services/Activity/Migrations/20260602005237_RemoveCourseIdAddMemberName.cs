using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Activity.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCourseIdAddMemberName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "MemberActivities");

            migrationBuilder.AddColumn<string>(
                name: "MemberName",
                table: "MemberActivities",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberName",
                table: "MemberActivities");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "MemberActivities",
                type: "uuid",
                nullable: true);
        }
    }
}
