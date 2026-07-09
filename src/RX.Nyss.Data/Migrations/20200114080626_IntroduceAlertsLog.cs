using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class IntroduceAlertsLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                schema: "nyss",
                table: "Reports",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcceptedById",
                schema: "nyss",
                table: "Reports",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                schema: "nyss",
                table: "Reports",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedById",
                schema: "nyss",
                table: "Reports",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                schema: "nyss",
                table: "Alerts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClosedById",
                schema: "nyss",
                table: "Alerts",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DismissedAt",
                schema: "nyss",
                table: "Alerts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DismissedById",
                schema: "nyss",
                table: "Alerts",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EscalatedAt",
                schema: "nyss",
                table: "Alerts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EscalatedById",
                schema: "nyss",
                table: "Alerts",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AcceptedById",
                schema: "nyss",
                table: "Reports",
                column: "AcceptedById");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_RejectedById",
                schema: "nyss",
                table: "Reports",
                column: "RejectedById");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ClosedById",
                schema: "nyss",
                table: "Alerts",
                column: "ClosedById");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_DismissedById",
                schema: "nyss",
                table: "Alerts",
                column: "DismissedById");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_EscalatedById",
                schema: "nyss",
                table: "Alerts",
                column: "EscalatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Users_ClosedById",
                schema: "nyss",
                table: "Alerts",
                column: "ClosedById",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Users_DismissedById",
                schema: "nyss",
                table: "Alerts",
                column: "DismissedById",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Users_EscalatedById",
                schema: "nyss",
                table: "Alerts",
                column: "EscalatedById",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_AcceptedById",
                schema: "nyss",
                table: "Reports",
                column: "AcceptedById",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_RejectedById",
                schema: "nyss",
                table: "Reports",
                column: "RejectedById",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Users_ClosedById",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Users_DismissedById",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Users_EscalatedById",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_AcceptedById",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_RejectedById",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_AcceptedById",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_RejectedById",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_ClosedById",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_DismissedById",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_EscalatedById",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "AcceptedById",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RejectedById",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ClosedById",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "DismissedAt",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "DismissedById",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "EscalatedAt",
                schema: "nyss",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "EscalatedById",
                schema: "nyss",
                table: "Alerts");
        }
    }
}
