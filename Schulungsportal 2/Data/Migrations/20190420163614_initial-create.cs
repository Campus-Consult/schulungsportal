using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Schulungsportal_2.Data.Migrations
{
    public partial class initialcreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Impressum",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Verantwortungsbereich = table.Column<string>(nullable: false),
                    Dienstanbieter = table.Column<string>(nullable: false),
                    Vorstand = table.Column<string>(nullable: false),
                    JournalistischeVerantwortung = table.Column<string>(nullable: false),
                    Kommunikation = table.Column<string>(nullable: false),
                    Anschrift = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Impressum", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MailProperties",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    mailserver = table.Column<string>(nullable: false),
                    port = table.Column<int>(nullable: false),
                    useSsl = table.Column<bool>(nullable: false),
                    absender = table.Column<string>(nullable: false),
                    passwort = table.Column<string>(nullable: false),
                    adresseSchulungsbeauftragter = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailProperties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Schulung",
                columns: table => new
                {
                    SchulungGUID = table.Column<string>(nullable: false),
                    Titel = table.Column<string>(nullable: false),
                    OrganisatorInstitution = table.Column<string>(nullable: false),
                    NameDozent = table.Column<string>(nullable: false),
                    NummerDozent = table.Column<string>(nullable: false),
                    EmailDozent = table.Column<string>(nullable: false),
                    Beschreibung = table.Column<string>(nullable: false),
                    Ort = table.Column<string>(nullable: false),
                    Anmeldefrist = table.Column<DateTime>(nullable: false),
                    IsAbgesagt = table.Column<bool>(nullable: false),
                    IsGeprüft = table.Column<bool>(nullable: false),
                    AccessToken = table.Column<string>(maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schulung", x => x.SchulungGUID);
                });

            migrationBuilder.CreateTable(
                name: "Anmeldung",
                columns: table => new
                {
                    anmeldungId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Vorname = table.Column<string>(nullable: false),
                    Nachname = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Nummer = table.Column<string>(nullable: false),
                    Status = table.Column<string>(nullable: false),
                    SchulungGuid = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anmeldung", x => x.anmeldungId);
                    table.ForeignKey(
                        name: "FK_Anmeldung_Schulung_SchulungGuid",
                        column: x => x.SchulungGuid,
                        principalTable: "Schulung",
                        principalColumn: "SchulungGUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Termin",
                columns: table => new
                {
                    SchulungGUID = table.Column<string>(nullable: false),
                    Start = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Termin", x => new { x.SchulungGUID, x.Start, x.End });
                    table.ForeignKey(
                        name: "FK_Termin_Schulung_SchulungGUID",
                        column: x => x.SchulungGUID,
                        principalTable: "Schulung",
                        principalColumn: "SchulungGUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anmeldung_SchulungGuid",
                table: "Anmeldung",
                column: "SchulungGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Schulung_AccessToken",
                table: "Schulung",
                column: "AccessToken",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anmeldung");

            migrationBuilder.DropTable(
                name: "Impressum");

            migrationBuilder.DropTable(
                name: "MailProperties");

            migrationBuilder.DropTable(
                name: "Termin");

            migrationBuilder.DropTable(
                name: "Schulung");
        }
    }
}
