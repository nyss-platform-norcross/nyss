using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddRawReportsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValid",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RawContent",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.AddColumn<int>(
                name: "EpiWeek",
                schema: "nyss",
                table: "Reports",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "nyss",
                table: "Reports",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsInTrainingMode",
                schema: "nyss",
                table: "DataCollectors",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RawReports",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sender = table.Column<string>(maxLength: 20, nullable: true),
                    Timestamp = table.Column<string>(maxLength: 14, nullable: true),
                    ReceivedAt = table.Column<DateTime>(nullable: false),
                    Text = table.Column<string>(maxLength: 160, nullable: true),
                    IncomingMessageId = table.Column<int>(nullable: true),
                    OutgoingMessageId = table.Column<int>(nullable: true),
                    ModemNumber = table.Column<int>(nullable: true),
                    ApiKey = table.Column<string>(maxLength: 100, nullable: false),
                    ReportId = table.Column<int>(nullable: true),
                    NationalSocietyId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawReports_NationalSocieties_NationalSocietyId",
                        column: x => x.NationalSocietyId,
                        principalSchema: "nyss",
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawReports_Reports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "nyss",
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RawReports_NationalSocietyId",
                schema: "nyss",
                table: "RawReports",
                column: "NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_RawReports_ReportId",
                schema: "nyss",
                table: "RawReports",
                column: "ReportId",
                unique: true,
                filter: "[ReportId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawReports",
                schema: "nyss");

            migrationBuilder.DropColumn(
                name: "EpiWeek",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IsInTrainingMode",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                schema: "nyss",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RawContent",
                schema: "nyss",
                table: "Reports",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");
        }
    }
}
