using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class UpdateDeleteBehavior : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthRiskLanguageContents_HealthRisks_HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents");

            migrationBuilder.DropForeignKey(
                name: "FK_HealthRisks_AlertRules_AlertRuleId",
                schema: "nyss",
                table: "HealthRisks");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthRiskLanguageContents_HealthRisks_HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                column: "HealthRiskId",
                principalSchema: "nyss",
                principalTable: "HealthRisks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HealthRisks_AlertRules_AlertRuleId",
                schema: "nyss",
                table: "HealthRisks",
                column: "AlertRuleId",
                principalSchema: "nyss",
                principalTable: "AlertRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthRiskLanguageContents_HealthRisks_HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents");

            migrationBuilder.DropForeignKey(
                name: "FK_HealthRisks_AlertRules_AlertRuleId",
                schema: "nyss",
                table: "HealthRisks");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthRiskLanguageContents_HealthRisks_HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                column: "HealthRiskId",
                principalSchema: "nyss",
                principalTable: "HealthRisks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HealthRisks_AlertRules_AlertRuleId",
                schema: "nyss",
                table: "HealthRisks",
                column: "AlertRuleId",
                principalSchema: "nyss",
                principalTable: "AlertRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
