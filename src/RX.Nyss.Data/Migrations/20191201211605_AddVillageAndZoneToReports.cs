using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddVillageAndZoneToReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VillageId",
                schema: "nyss",
                table: "Reports",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ZoneId",
                schema: "nyss",
                table: "Reports",
                nullable: true);

            migrationBuilder.Sql(@"
            UPDATE [nyss].[Reports]
            SET [nyss].[Reports].[VillageId] = [nyss].[DataCollectors].[VillageId],
                [nyss].[Reports].[ZoneId] = [nyss].[DataCollectors].[ZoneId]
            FROM [nyss].[Reports]
            INNER JOIN  [nyss].[DataCollectors] ON [nyss].[Reports].[DataCollectorId] = [nyss].[DataCollectors].[Id]");

            migrationBuilder.AlterColumn<int>(
                name: "VillageId",
                schema: "nyss",
                table: "Reports",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_VillageId",
                schema: "nyss",
                table: "Reports",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ZoneId",
                schema: "nyss",
                table: "Reports",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Villages_VillageId",
                schema: "nyss",
                table: "Reports",
                column: "VillageId",
                principalSchema: "nyss",
                principalTable: "Villages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Zones_ZoneId",
                schema: "nyss",
                table: "Reports",
                column: "ZoneId",
                principalSchema: "nyss",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Villages_VillageId",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Zones_ZoneId",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_VillageId",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ZoneId",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "VillageId",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                schema: "nyss",
                table: "Reports");
        }
    }
}
