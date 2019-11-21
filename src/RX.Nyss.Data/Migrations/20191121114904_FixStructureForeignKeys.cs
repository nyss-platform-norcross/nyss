using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class FixStructureForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Districts_Regions_RegionId",
                schema: "nyss",
                table: "Districts");

            migrationBuilder.DropForeignKey(
                name: "FK_Regions_NationalSocieties_NationalSocietyId",
                schema: "nyss",
                table: "Regions");

            migrationBuilder.DropForeignKey(
                name: "FK_Villages_Districts_DistrictId",
                schema: "nyss",
                table: "Villages");

            migrationBuilder.DropForeignKey(
                name: "FK_Zones_Villages_VillageId",
                schema: "nyss",
                table: "Zones");

            migrationBuilder.AlterColumn<int>(
                name: "HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Districts_Regions_RegionId",
                schema: "nyss",
                table: "Districts",
                column: "RegionId",
                principalSchema: "nyss",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Regions_NationalSocieties_NationalSocietyId",
                schema: "nyss",
                table: "Regions",
                column: "NationalSocietyId",
                principalSchema: "nyss",
                principalTable: "NationalSocieties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Villages_Districts_DistrictId",
                schema: "nyss",
                table: "Villages",
                column: "DistrictId",
                principalSchema: "nyss",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Zones_Villages_VillageId",
                schema: "nyss",
                table: "Zones",
                column: "VillageId",
                principalSchema: "nyss",
                principalTable: "Villages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Districts_Regions_RegionId",
                schema: "nyss",
                table: "Districts");

            migrationBuilder.DropForeignKey(
                name: "FK_Regions_NationalSocieties_NationalSocietyId",
                schema: "nyss",
                table: "Regions");

            migrationBuilder.DropForeignKey(
                name: "FK_Villages_Districts_DistrictId",
                schema: "nyss",
                table: "Villages");

            migrationBuilder.DropForeignKey(
                name: "FK_Zones_Villages_VillageId",
                schema: "nyss",
                table: "Zones");

            migrationBuilder.AlterColumn<int>(
                name: "HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Districts_Regions_RegionId",
                schema: "nyss",
                table: "Districts",
                column: "RegionId",
                principalSchema: "nyss",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Regions_NationalSocieties_NationalSocietyId",
                schema: "nyss",
                table: "Regions",
                column: "NationalSocietyId",
                principalSchema: "nyss",
                principalTable: "NationalSocieties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Villages_Districts_DistrictId",
                schema: "nyss",
                table: "Villages",
                column: "DistrictId",
                principalSchema: "nyss",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Zones_Villages_VillageId",
                schema: "nyss",
                table: "Zones",
                column: "VillageId",
                principalSchema: "nyss",
                principalTable: "Villages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
