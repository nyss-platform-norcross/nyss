using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddHeadManagerConsentsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HeadManagerConsents",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NationalSocietyId = table.Column<int>(nullable: false),
                    UserEmailAddress = table.Column<string>(maxLength: 100, nullable: false),
                    UserPhoneNumber = table.Column<string>(maxLength: 20, nullable: false),
                    ConsentedFrom = table.Column<DateTime>(nullable: false),
                    ConsentedUntil = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeadManagerConsents", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeadManagerConsents",
                schema: "nyss");
        }
    }
}
