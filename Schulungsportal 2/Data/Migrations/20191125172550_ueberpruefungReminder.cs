using Microsoft.EntityFrameworkCore.Migrations;

namespace Schulungsportal_2.Data.Migrations
{
    public partial class ueberpruefungReminder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GeprüftReminderSent",
                table: "Schulung",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeprüftReminderSent",
                table: "Schulung");
        }
    }
}
