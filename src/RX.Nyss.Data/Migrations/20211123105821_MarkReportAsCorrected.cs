using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class MarkReportAsCorrected : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MarkedAsCorrectedAtUtc",
                schema: "nyss",
                table: "RawReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarkedAsCorrectedBy",
                schema: "nyss",
                table: "RawReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkedAsCorrectedAtUtc",
                schema: "nyss",
                table: "RawReports");

            migrationBuilder.DropColumn(
                name: "MarkedAsCorrectedBy",
                schema: "nyss",
                table: "RawReports");
        }
    }
}
