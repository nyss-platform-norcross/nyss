using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddProjectHealthRiskToAlertNotificationRecipients : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectHealthRiskAlertRecipients",
                schema: "nyss",
                columns: table => new
                {
                    AlertNotificationRecipientId = table.Column<int>(nullable: false),
                    ProjectHealthRiskId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectHealthRiskAlertRecipients", x => new { x.ProjectHealthRiskId, x.AlertNotificationRecipientId });
                    table.ForeignKey(
                        name: "FK_ProjectHealthRiskAlertRecipients_AlertNotificationRecipients_AlertNotificationRecipientId",
                        column: x => x.AlertNotificationRecipientId,
                        principalSchema: "nyss",
                        principalTable: "AlertNotificationRecipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectHealthRiskAlertRecipients_ProjectHealthRisks_ProjectHealthRiskId",
                        column: x => x.ProjectHealthRiskId,
                        principalSchema: "nyss",
                        principalTable: "ProjectHealthRisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHealthRiskAlertRecipients_AlertNotificationRecipientId",
                schema: "nyss",
                table: "ProjectHealthRiskAlertRecipients",
                column: "AlertNotificationRecipientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectHealthRiskAlertRecipients",
                schema: "nyss");
        }
    }
}
