using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class RenameHeadManagerConsentsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("HeadManagerConsents", "nyss", "NationalSocietyConsents", "nyss");
            migrationBuilder.RenameIndex("PK_HeadManagerConsents", "PK_NationalSocietyConsents", "NationalSocietyConsents", "nyss");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex("PK_NationalSocietyConsents", "PK_HeadManagerConsents", "NationalSocietyConsents", "nyss");
            migrationBuilder.RenameTable("NationalSocietyConsents", "nyss", "HeadManagerConsents", "nyss");
        }
    }
}
