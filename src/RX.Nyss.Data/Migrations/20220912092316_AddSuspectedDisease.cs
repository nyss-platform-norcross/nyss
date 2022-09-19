using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    public partial class AddSuspectedDisease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrackerProgramId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SuspectedDiseaseDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumberDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LocationDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GenderDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EventTypeDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateOfOnsetDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SuspectedDiseases",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SuspectedDiseaseCode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspectedDiseases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthRiskSuspectedDisease",
                schema: "nyss",
                columns: table => new
                {
                    HealthRisksId = table.Column<int>(type: "int", nullable: false),
                    SuspectedDiseasesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthRiskSuspectedDisease", x => new { x.HealthRisksId, x.SuspectedDiseasesId });
                    table.ForeignKey(
                        name: "FK_HealthRiskSuspectedDisease_HealthRisks_HealthRisksId",
                        column: x => x.HealthRisksId,
                        principalSchema: "nyss",
                        principalTable: "HealthRisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthRiskSuspectedDisease_SuspectedDiseases_SuspectedDiseasesId",
                        column: x => x.SuspectedDiseasesId,
                        principalSchema: "nyss",
                        principalTable: "SuspectedDiseases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuspectedDiseaseLanguageContents",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuspectedDiseaseId = table.Column<int>(type: "int", nullable: true),
                    ContentLanguageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspectedDiseaseLanguageContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuspectedDiseaseLanguageContents_ContentLanguages_ContentLanguageId",
                        column: x => x.ContentLanguageId,
                        principalSchema: "nyss",
                        principalTable: "ContentLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SuspectedDiseaseLanguageContents_SuspectedDiseases_SuspectedDiseaseId",
                        column: x => x.SuspectedDiseaseId,
                        principalSchema: "nyss",
                        principalTable: "SuspectedDiseases",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthRiskSuspectedDisease_SuspectedDiseasesId",
                schema: "nyss",
                table: "HealthRiskSuspectedDisease",
                column: "SuspectedDiseasesId");

            migrationBuilder.CreateIndex(
                name: "IX_SuspectedDiseaseLanguageContents_ContentLanguageId",
                schema: "nyss",
                table: "SuspectedDiseaseLanguageContents",
                column: "ContentLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_SuspectedDiseaseLanguageContents_SuspectedDiseaseId",
                schema: "nyss",
                table: "SuspectedDiseaseLanguageContents",
                column: "SuspectedDiseaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthRiskSuspectedDisease",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "SuspectedDiseaseLanguageContents",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "SuspectedDiseases",
                schema: "nyss");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrackerProgramId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SuspectedDiseaseDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumberDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LocationDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GenderDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EventTypeDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateOfOnsetDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
