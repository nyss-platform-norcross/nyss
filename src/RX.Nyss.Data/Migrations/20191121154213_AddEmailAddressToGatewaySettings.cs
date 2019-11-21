using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddEmailAddressToGatewaySettings : Migration
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

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ManagerUserId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Villages_VillageId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Zones_ZoneId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ManagerUserId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_VillageId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ZoneId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ContentLanguageId",
                schema: "nyss",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_AlertRecipients_AlertRuleId",
                schema: "nyss",
                table: "AlertRecipients");

            migrationBuilder.DropColumn(
                name: "ManagerUserId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VillageId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                schema: "nyss",
                table: "Users");

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
                name: "Sex",
                schema: "nyss",
                table: "Users",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecadeOfBirth",
                schema: "nyss",
                table: "Users",
                nullable: true);

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

            migrationBuilder.AlterColumn<int>(
                name: "HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                schema: "nyss",
                table: "GatewaySettings",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BirthGroupDecade",
                schema: "nyss",
                table: "DataCollectors",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                schema: "nyss",
                table: "DataCollectors",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.CreateTable(
                name: "SupervisorUserProjects",
                schema: "nyss",
                columns: table => new
                {
                    SupervisorUserId = table.Column<int>(nullable: false),
                    ProjectId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupervisorUserProjects", x => new { x.SupervisorUserId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_SupervisorUserProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "nyss",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupervisorUserProjects_Users_SupervisorUserId",
                        column: x => x.SupervisorUserId,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertRecipients_ProjectId",
                schema: "nyss",
                table: "AlertRecipients",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorUserProjects_ProjectId",
                schema: "nyss",
                table: "SupervisorUserProjects",
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

            migrationBuilder.DropTable(
                name: "SupervisorUserProjects",
                schema: "nyss");

            migrationBuilder.DropIndex(
                name: "IX_AlertRecipients_ProjectId",
                schema: "nyss",
                table: "AlertRecipients");

            migrationBuilder.DropColumn(
                name: "DecadeOfBirth",
                schema: "nyss",
                table: "Users");

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
                name: "EmailAddress",
                schema: "nyss",
                table: "GatewaySettings");

            migrationBuilder.DropColumn(
                name: "BirthGroupDecade",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropColumn(
                name: "Sex",
                schema: "nyss",
                table: "DataCollectors");

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
                name: "Sex",
                schema: "nyss",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManagerUserId",
                schema: "nyss",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VillageId",
                schema: "nyss",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ZoneId",
                schema: "nyss",
                table: "Users",
                type: "int",
                nullable: true);

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

            migrationBuilder.AlterColumn<int>(
                name: "HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

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
                name: "IX_Users_ManagerUserId",
                schema: "nyss",
                table: "Users",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_VillageId",
                schema: "nyss",
                table: "Users",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ZoneId",
                schema: "nyss",
                table: "Users",
                column: "ZoneId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ManagerUserId",
                schema: "nyss",
                table: "Users",
                column: "ManagerUserId",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Villages_VillageId",
                schema: "nyss",
                table: "Users",
                column: "VillageId",
                principalSchema: "nyss",
                principalTable: "Villages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Zones_ZoneId",
                schema: "nyss",
                table: "Users",
                column: "ZoneId",
                principalSchema: "nyss",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
