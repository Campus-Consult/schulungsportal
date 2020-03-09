using Microsoft.EntityFrameworkCore.Migrations;

namespace Schulungsportal_2.Data.Migrations
{
    public partial class mehreredozenten : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dozent",
                columns: table => new
                {
                    SchulungGUID = table.Column<string>(nullable: false),
                    EMail = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Nummer = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dozent", x => new { x.SchulungGUID, x.EMail });
                    table.ForeignKey(
                        name: "FK_Dozent_Schulung_SchulungGUID",
                        column: x => x.SchulungGUID,
                        principalTable: "Schulung",
                        principalColumn: "SchulungGUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("INSERT INTO Dozent (`SchulungGUID`, `EMail`, `Name`, `Nummer`) SELECT SchulungGUID, EmailDozent, NameDozent, NummerDozent FROM Schulung");

            migrationBuilder.DropColumn(
                name: "EmailDozent",
                table: "Schulung");

            migrationBuilder.DropColumn(
                name: "NameDozent",
                table: "Schulung");

            migrationBuilder.DropColumn(
                name: "NummerDozent",
                table: "Schulung");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailDozent",
                table: "Schulung",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameDozent",
                table: "Schulung",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NummerDozent",
                table: "Schulung",
                nullable: true);
            
            migrationBuilder.Sql("UPDATE Schulung s, (SELECT SchulungGUID, EMail, Name, Nummer FROM Dozent) d SET s.EmailDozent = d.EMail, s.NameDozent = d.Name, s.NummerDozent = d.Nummer WHERE s.SchulungGUID = d.SchulungGUID");

            migrationBuilder.DropTable(
                name: "Dozent");
            
            migrationBuilder.AlterColumn<string>(
                name: "EmailDozent",
                table: "Schulung",
                nullable: false
            );
            
            migrationBuilder.AlterColumn<string>(
                name: "NameDozent",
                table: "Schulung",
                nullable: false
            );
            
            migrationBuilder.AlterColumn<string>(
                name: "NummerDozent",
                table: "Schulung",
                nullable: false
            );
        }
    }
}
