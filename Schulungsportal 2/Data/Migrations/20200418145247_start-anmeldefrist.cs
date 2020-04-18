using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Schulungsportal_2.Data.Migrations
{
    public partial class startanmeldefrist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartAnmeldefrist",
                table: "Schulung",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartAnmeldefrist",
                table: "Schulung");
        }
    }
}
