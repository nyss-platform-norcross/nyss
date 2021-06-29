using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddAlertEventLogTableAndEventTypeTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertEventTypes",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertEventTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertEventSubtypes",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    AlertEventTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertEventSubtypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertEventSubtypes_AlertEventTypes_AlertEventTypeId",
                        column: x => x.AlertEventTypeId,
                        principalSchema: "nyss",
                        principalTable: "AlertEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlertEventLogs",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlertId = table.Column<int>(nullable: false),
                    AlertEventTypeId = table.Column<int>(nullable: false),
                    AlertEventSubtypeId = table.Column<int>(nullable: true),
                    LoggedById = table.Column<int>(nullable: false),
                    Text = table.Column<string>(maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertEventLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertEventLogs_AlertEventSubtypes_AlertEventSubtypeId",
                        column: x => x.AlertEventSubtypeId,
                        principalSchema: "nyss",
                        principalTable: "AlertEventSubtypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertEventLogs_AlertEventTypes_AlertEventTypeId",
                        column: x => x.AlertEventTypeId,
                        principalSchema: "nyss",
                        principalTable: "AlertEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertEventLogs_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalSchema: "nyss",
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertEventLogs_Users_LoggedById",
                        column: x => x.LoggedById,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "nyss",
                table: "AlertEventTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Investigation" },
                    { 2, "PresumedCasePositive" },
                    { 3, "PresumedCaseNegative" },
                    { 4, "Outcome" },
                    { 5, "Details" },
                    { 6, "PublicHealthActionTaken" }
                });

            migrationBuilder.InsertData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                columns: new[] { "Id", "AlertEventTypeId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Investigated" },
                    { 20, 6, "AssistedInvestigation" },
                    { 19, 6, "CommunityMeeting" },
                    { 18, 6, "SafeDignifiedBurials" },
                    { 17, 6, "AnimalsDisposed" },
                    { 16, 6, "ProvidedORS" },
                    { 15, 6, "Isolation" },
                    { 14, 6, "Referral" },
                    { 13, 6, "HealthMessagesAwarenessRaising" },
                    { 12, 6, "ImmunizationCampaign" },
                    { 11, 4, "Deceased" },
                    { 10, 4, "Recovered" },
                    { 9, 3, "Unknown" },
                    { 8, 3, "Clinical" },
                    { 7, 3, "Lab" },
                    { 6, 2, "Unknown" },
                    { 5, 2, "Clinical" },
                    { 4, 2, "Lab" },
                    { 3, 1, "Unknown" },
                    { 2, 1, "NotInvestigated" },
                    { 21, 6, "CleanupFogging" },
                    { 22, 6, "Other" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertEventLogs_AlertEventSubtypeId",
                schema: "nyss",
                table: "AlertEventLogs",
                column: "AlertEventSubtypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertEventLogs_AlertEventTypeId",
                schema: "nyss",
                table: "AlertEventLogs",
                column: "AlertEventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertEventLogs_AlertId",
                schema: "nyss",
                table: "AlertEventLogs",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertEventLogs_LoggedById",
                schema: "nyss",
                table: "AlertEventLogs",
                column: "LoggedById");

            migrationBuilder.CreateIndex(
                name: "IX_AlertEventSubtypes_AlertEventTypeId",
                schema: "nyss",
                table: "AlertEventSubtypes",
                column: "AlertEventTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertEventLogs",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "AlertEventSubtypes",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "AlertEventTypes",
                schema: "nyss");
        }
    }
}
