using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AllowCascadeDeletingRecipientFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectHealthRiskAlertRecipients_AlertNotificationRecipients_AlertNotificationRecipientId",
                schema: "nyss",
                table: "ProjectHealthRiskAlertRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_SupervisorUserAlertRecipients_AlertNotificationRecipients_AlertNotificationRecipientId",
                schema: "nyss",
                table: "SupervisorUserAlertRecipients");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectHealthRiskAlertRecipients_AlertNotificationRecipients_AlertNotificationRecipientId",
                schema: "nyss",
                table: "ProjectHealthRiskAlertRecipients",
                column: "AlertNotificationRecipientId",
                principalSchema: "nyss",
                principalTable: "AlertNotificationRecipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupervisorUserAlertRecipients_AlertNotificationRecipients_AlertNotificationRecipientId",
                schema: "nyss",
                table: "SupervisorUserAlertRecipients",
                column: "AlertNotificationRecipientId",
                principalSchema: "nyss",
                principalTable: "AlertNotificationRecipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectHealthRiskAlertRecipients_AlertNotificationRecipients_AlertNotificationRecipientId",
                schema: "nyss",
                table: "ProjectHealthRiskAlertRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_SupervisorUserAlertRecipients_AlertNotificationRecipients_AlertNotificationRecipientId",
                schema: "nyss",
                table: "SupervisorUserAlertRecipients");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectHealthRiskAlertRecipients_AlertNotificationRecipients_AlertNotificationRecipientId",
                schema: "nyss",
                table: "ProjectHealthRiskAlertRecipients",
                column: "AlertNotificationRecipientId",
                principalSchema: "nyss",
                principalTable: "AlertNotificationRecipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupervisorUserAlertRecipients_AlertNotificationRecipients_AlertNotificationRecipientId",
                schema: "nyss",
                table: "SupervisorUserAlertRecipients",
                column: "AlertNotificationRecipientId",
                principalSchema: "nyss",
                principalTable: "AlertNotificationRecipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
