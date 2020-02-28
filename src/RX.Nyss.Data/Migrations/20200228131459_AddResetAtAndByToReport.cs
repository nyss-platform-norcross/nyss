using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddResetAtAndByToReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ResetAt",
                schema: "nyss",
                table: "Reports",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResetById",
                schema: "nyss",
                table: "Reports",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ResetById",
                schema: "nyss",
                table: "Reports",
                column: "ResetById");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_ResetById",
                schema: "nyss",
                table: "Reports",
                column: "ResetById",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_ResetById",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ResetById",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ResetAt",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ResetById",
                schema: "nyss",
                table: "Reports");
        }
    }
}
