using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddHeadManagerToNsAndRemoveManagerProps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentedAt",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HasConsented",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDataOwner",
                schema: "nyss",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "HeadManagerId",
                schema: "nyss",
                table: "NationalSocieties",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PendingHeadManagerId",
                schema: "nyss",
                table: "NationalSocieties",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_HeadManagerId",
                schema: "nyss",
                table: "NationalSocieties",
                column: "HeadManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_PendingHeadManagerId",
                schema: "nyss",
                table: "NationalSocieties",
                column: "PendingHeadManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_NationalSocieties_Users_HeadManagerId",
                schema: "nyss",
                table: "NationalSocieties",
                column: "HeadManagerId",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NationalSocieties_Users_PendingHeadManagerId",
                schema: "nyss",
                table: "NationalSocieties",
                column: "PendingHeadManagerId",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NationalSocieties_Users_HeadManagerId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropForeignKey(
                name: "FK_NationalSocieties_Users_PendingHeadManagerId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropIndex(
                name: "IX_NationalSocieties_HeadManagerId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropIndex(
                name: "IX_NationalSocieties_PendingHeadManagerId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropColumn(
                name: "HeadManagerId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropColumn(
                name: "PendingHeadManagerId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentedAt",
                schema: "nyss",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasConsented",
                schema: "nyss",
                table: "Users",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDataOwner",
                schema: "nyss",
                table: "Users",
                type: "bit",
                nullable: true);
        }
    }
}
