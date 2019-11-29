using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddDataCollectorRelationToRawReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DataCollectorId",
                schema: "nyss",
                table: "RawReports",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawReports_DataCollectorId",
                schema: "nyss",
                table: "RawReports",
                column: "DataCollectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_RawReports_DataCollectors_DataCollectorId",
                schema: "nyss",
                table: "RawReports",
                column: "DataCollectorId",
                principalSchema: "nyss",
                principalTable: "DataCollectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RawReports_DataCollectors_DataCollectorId",
                schema: "nyss",
                table: "RawReports");

            migrationBuilder.DropIndex(
                name: "IX_RawReports_DataCollectorId",
                schema: "nyss",
                table: "RawReports");

            migrationBuilder.DropColumn(
                name: "DataCollectorId",
                schema: "nyss",
                table: "RawReports");

            migrationBuilder.AlterColumn<int>(
                name: "HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
