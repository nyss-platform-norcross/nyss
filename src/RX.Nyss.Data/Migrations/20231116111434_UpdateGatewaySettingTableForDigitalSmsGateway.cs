using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    public partial class UpdateGatewaySettingTableForDigitalSmsGateway : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GatewayApiKey",
                schema: "nyss",
                table: "GatewaySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GatewayApiKeyName",
                schema: "nyss",
                table: "GatewaySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GatewayExtraKey",
                schema: "nyss",
                table: "GatewaySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GatewayExtraKeyName",
                schema: "nyss",
                table: "GatewaySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GatewaySenderId",
                schema: "nyss",
                table: "GatewaySettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GatewayUrl",
                schema: "nyss",
                table: "GatewaySettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GatewayApiKey",
                schema: "nyss",
                table: "GatewaySettings");

            migrationBuilder.DropColumn(
                name: "GatewayApiKeyName",
                schema: "nyss",
                table: "GatewaySettings");

            migrationBuilder.DropColumn(
                name: "GatewayExtraKey",
                schema: "nyss",
                table: "GatewaySettings");

            migrationBuilder.DropColumn(
                name: "GatewayExtraKeyName",
                schema: "nyss",
                table: "GatewaySettings");

            migrationBuilder.DropColumn(
                name: "GatewaySenderId",
                schema: "nyss",
                table: "GatewaySettings");

            migrationBuilder.DropColumn(
                name: "GatewayUrl",
                schema: "nyss",
                table: "GatewaySettings");
        }
    }
}
