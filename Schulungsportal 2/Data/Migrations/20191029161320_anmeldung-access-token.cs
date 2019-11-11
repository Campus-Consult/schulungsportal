using Microsoft.EntityFrameworkCore.Migrations;

namespace Schulungsportal_2.Data.Migrations
{
    public partial class anmeldungaccesstoken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "Anmeldung",
                maxLength: 40,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "Anmeldung");
        }
    }
}
