using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Membership.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalFileToHealthProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MedicalFileName",
                table: "MemberHealthProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalFileUrl",
                table: "MemberHealthProfiles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicalFileName",
                table: "MemberHealthProfiles");

            migrationBuilder.DropColumn(
                name: "MedicalFileUrl",
                table: "MemberHealthProfiles");
        }
    }
}
