using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    public partial class UpdateGatewaySettingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EidsrConfiguration_NationalSocietyId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.AddColumn<string>(
                name: "TelerivetProjectId",
                schema: "nyss",
                table: "GatewaySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelerivetSendSmsApiKey",
                schema: "nyss",
                table: "GatewaySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EidsrConfiguration_NationalSocietyId",
                schema: "nyss",
                table: "EidsrConfiguration",
                column: "NationalSocietyId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EidsrConfiguration_NationalSocietyId",
                schema: "nyss",
                table: "EidsrConfiguration");

            migrationBuilder.DropColumn(
                name: "TelerivetProjectId",
                schema: "nyss",
                table: "GatewaySettings");

            migrationBuilder.DropColumn(
                name: "TelerivetSendSmsApiKey",
                schema: "nyss",
                table: "GatewaySettings");

            migrationBuilder.CreateIndex(
                name: "IX_EidsrConfiguration_NationalSocietyId",
                schema: "nyss",
                table: "EidsrConfiguration",
                column: "NationalSocietyId");
        }
    }
}
