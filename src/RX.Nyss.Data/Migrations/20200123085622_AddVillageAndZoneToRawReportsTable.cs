using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddVillageAndZoneToRawReportsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VillageId",
                schema: "nyss",
                table: "RawReports",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ZoneId",
                schema: "nyss",
                table: "RawReports",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawReports_VillageId",
                schema: "nyss",
                table: "RawReports",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_RawReports_ZoneId",
                schema: "nyss",
                table: "RawReports",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_RawReports_Villages_VillageId",
                schema: "nyss",
                table: "RawReports",
                column: "VillageId",
                principalSchema: "nyss",
                principalTable: "Villages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RawReports_Zones_ZoneId",
                schema: "nyss",
                table: "RawReports",
                column: "ZoneId",
                principalSchema: "nyss",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("UPDATE rr SET " +
                                 "rr.[VillageId]=r.[VillageId], rr.[ZoneId]=r.[ZoneId] " +
                                 "FROM [nyss].[RawReports] rr " +
                                 "JOIN [nyss].[Reports] r ON rr.[ReportId] = r.[Id]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RawReports_Villages_VillageId",
                schema: "nyss",
                table: "RawReports");

            migrationBuilder.DropForeignKey(
                name: "FK_RawReports_Zones_ZoneId",
                schema: "nyss",
                table: "RawReports");

            migrationBuilder.DropIndex(
                name: "IX_RawReports_VillageId",
                schema: "nyss",
                table: "RawReports");

            migrationBuilder.DropIndex(
                name: "IX_RawReports_ZoneId",
                schema: "nyss",
                table: "RawReports");

            migrationBuilder.DropColumn(
                name: "VillageId",
                schema: "nyss",
                table: "RawReports");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                schema: "nyss",
                table: "RawReports");
        }
    }
}
