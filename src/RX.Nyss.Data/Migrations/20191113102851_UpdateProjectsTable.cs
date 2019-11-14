using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class UpdateProjectsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertRecipients_AlertRules_AlertRuleId",
                schema: "nyss",
                table: "AlertRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ContentLanguages_ContentLanguageId",
                schema: "nyss",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ContentLanguageId",
                schema: "nyss",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_AlertRecipients_AlertRuleId",
                schema: "nyss",
                table: "AlertRecipients");

            migrationBuilder.DropColumn(
                name: "ContentLanguageId",
                schema: "nyss",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "HoursThreshold",
                schema: "nyss",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "MetersThreshold",
                schema: "nyss",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "AlertRuleId",
                schema: "nyss",
                table: "AlertRecipients");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "nyss",
                table: "Projects",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                schema: "nyss",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                schema: "nyss",
                table: "Projects",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CaseDefinition",
                schema: "nyss",
                table: "ProjectHealthRisks",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysThreshold",
                schema: "nyss",
                table: "AlertRules",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KilometersThreshold",
                schema: "nyss",
                table: "AlertRules",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                schema: "nyss",
                table: "AlertRecipients",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AlertRecipients_ProjectId",
                schema: "nyss",
                table: "AlertRecipients",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertRecipients_Projects_ProjectId",
                schema: "nyss",
                table: "AlertRecipients",
                column: "ProjectId",
                principalSchema: "nyss",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertRecipients_Projects_ProjectId",
                schema: "nyss",
                table: "AlertRecipients");

            migrationBuilder.DropIndex(
                name: "IX_AlertRecipients_ProjectId",
                schema: "nyss",
                table: "AlertRecipients");

            migrationBuilder.DropColumn(
                name: "EndDate",
                schema: "nyss",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "StartDate",
                schema: "nyss",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CaseDefinition",
                schema: "nyss",
                table: "ProjectHealthRisks");

            migrationBuilder.DropColumn(
                name: "DaysThreshold",
                schema: "nyss",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "KilometersThreshold",
                schema: "nyss",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "nyss",
                table: "AlertRecipients");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "nyss",
                table: "Projects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "ContentLanguageId",
                schema: "nyss",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HoursThreshold",
                schema: "nyss",
                table: "AlertRules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MetersThreshold",
                schema: "nyss",
                table: "AlertRules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AlertRuleId",
                schema: "nyss",
                table: "AlertRecipients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ContentLanguageId",
                schema: "nyss",
                table: "Projects",
                column: "ContentLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRecipients_AlertRuleId",
                schema: "nyss",
                table: "AlertRecipients",
                column: "AlertRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertRecipients_AlertRules_AlertRuleId",
                schema: "nyss",
                table: "AlertRecipients",
                column: "AlertRuleId",
                principalSchema: "nyss",
                principalTable: "AlertRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ContentLanguages_ContentLanguageId",
                schema: "nyss",
                table: "Projects",
                column: "ContentLanguageId",
                principalSchema: "nyss",
                principalTable: "ContentLanguages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
