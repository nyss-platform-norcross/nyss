using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddedRecipientsNotifiedToAlerts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RecipientsNotifiedAt",
                schema: "nyss",
                table: "Alerts",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientsNotifiedAt",
                schema: "nyss",
                table: "Alerts");
        }
    }
}
