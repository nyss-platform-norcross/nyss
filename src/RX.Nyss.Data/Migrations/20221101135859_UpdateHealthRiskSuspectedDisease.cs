using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    public partial class UpdateHealthRiskSuspectedDisease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HealthRiskSuspectedDisease",
                schema: "nyss",
                table: "HealthRiskSuspectedDisease");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "nyss",
                table: "HealthRiskSuspectedDisease",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumberDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateOfOnsetDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HealthRiskSuspectedDisease",
                schema: "nyss",
                table: "HealthRiskSuspectedDisease",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_HealthRiskSuspectedDisease_SuspectedDiseaseId",
                schema: "nyss",
                table: "HealthRiskSuspectedDisease",
                column: "SuspectedDiseaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HealthRiskSuspectedDisease",
                schema: "nyss",
                table: "HealthRiskSuspectedDisease");

            migrationBuilder.DropIndex(
                name: "IX_HealthRiskSuspectedDisease_SuspectedDiseaseId",
                schema: "nyss",
                table: "HealthRiskSuspectedDisease");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "nyss",
                table: "HealthRiskSuspectedDisease");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumberDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateOfOnsetDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HealthRiskSuspectedDisease",
                schema: "nyss",
                table: "HealthRiskSuspectedDisease",
                column: "SuspectedDiseaseId");
        }
    }
}
