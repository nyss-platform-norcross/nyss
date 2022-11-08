using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    public partial class AddSuspectedDiseaseTableAndUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SuspectedDisease",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SuspectedDiseaseCode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspectedDisease", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthRiskSuspectedDisease",
                schema: "nyss",
                columns: table => new
                {
                    SuspectedDiseaseId = table.Column<int>(type: "int", nullable: false),
                    HealthRiskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthRiskSuspectedDisease", x => x.SuspectedDiseaseId);
                    table.ForeignKey(
                        name: "FK_HealthRiskSuspectedDisease_HealthRisks_HealthRiskId",
                        column: x => x.HealthRiskId,
                        principalSchema: "nyss",
                        principalTable: "HealthRisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthRiskSuspectedDisease_SuspectedDisease_SuspectedDiseaseId",
                        column: x => x.SuspectedDiseaseId,
                        principalSchema: "nyss",
                        principalTable: "SuspectedDisease",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuspectedDiseaseLanguageContent",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SuspectedDiseaseId = table.Column<int>(type: "int", nullable: false),
                    ContentLanguageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspectedDiseaseLanguageContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuspectedDiseaseLanguageContent_ContentLanguages_ContentLanguageId",
                        column: x => x.ContentLanguageId,
                        principalSchema: "nyss",
                        principalTable: "ContentLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuspectedDiseaseLanguageContent_SuspectedDisease_SuspectedDiseaseId",
                        column: x => x.SuspectedDiseaseId,
                        principalSchema: "nyss",
                        principalTable: "SuspectedDisease",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthRiskSuspectedDisease_HealthRiskId",
                schema: "nyss",
                table: "HealthRiskSuspectedDisease",
                column: "HealthRiskId");

            migrationBuilder.CreateIndex(
                name: "IX_SuspectedDiseaseLanguageContent_ContentLanguageId",
                schema: "nyss",
                table: "SuspectedDiseaseLanguageContent",
                column: "ContentLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_SuspectedDiseaseLanguageContent_SuspectedDiseaseId",
                schema: "nyss",
                table: "SuspectedDiseaseLanguageContent",
                column: "SuspectedDiseaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthRiskSuspectedDisease",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "SuspectedDiseaseLanguageContent",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "SuspectedDisease",
                schema: "nyss");
        }
    }
}
