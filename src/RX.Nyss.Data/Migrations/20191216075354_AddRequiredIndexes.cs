using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddRequiredIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "nyss",
                table: "Alerts",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailAddress",
                schema: "nyss",
                table: "Users",
                column: "EmailAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_CreatedAt",
                schema: "nyss",
                table: "Reports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_EpiWeek",
                schema: "nyss",
                table: "Reports",
                column: "EpiWeek");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReceivedAt",
                schema: "nyss",
                table: "Reports",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_IsArchived",
                schema: "nyss",
                table: "NationalSocieties",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_StartDate",
                schema: "nyss",
                table: "NationalSocieties",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_HealthRisks_HealthRiskType",
                schema: "nyss",
                table: "HealthRisks",
                column: "HealthRiskType");

            migrationBuilder.CreateIndex(
                name: "IX_GatewaySettings_ApiKey",
                schema: "nyss",
                table: "GatewaySettings",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_AdditionalPhoneNumber",
                schema: "nyss",
                table: "DataCollectors",
                column: "AdditionalPhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_CreatedAt",
                schema: "nyss",
                table: "DataCollectors",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_DataCollectorType",
                schema: "nyss",
                table: "DataCollectors",
                column: "DataCollectorType");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_DeletedAt",
                schema: "nyss",
                table: "DataCollectors",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_PhoneNumber",
                schema: "nyss",
                table: "DataCollectors",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_CreatedAt",
                schema: "nyss",
                table: "Alerts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Status",
                schema: "nyss",
                table: "Alerts",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_EmailAddress",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Reports_CreatedAt",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_EpiWeek",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ReceivedAt",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_NationalSocieties_IsArchived",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropIndex(
                name: "IX_NationalSocieties_StartDate",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropIndex(
                name: "IX_HealthRisks_HealthRiskType",
                schema: "nyss",
                table: "HealthRisks");

            migrationBuilder.DropIndex(
                name: "IX_GatewaySettings_ApiKey",
                schema: "nyss",
                table: "GatewaySettings");

            migrationBuilder.DropIndex(
                name: "IX_DataCollectors_AdditionalPhoneNumber",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropIndex(
                name: "IX_DataCollectors_CreatedAt",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropIndex(
                name: "IX_DataCollectors_DataCollectorType",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropIndex(
                name: "IX_DataCollectors_DeletedAt",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropIndex(
                name: "IX_DataCollectors_PhoneNumber",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_CreatedAt",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_Status",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "nyss",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 20);
        }
    }
}
