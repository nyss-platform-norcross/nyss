using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddProjectOrgnaizationToAlertRecipient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectOrganizationId",
                schema: "nyss",
                table: "AlertNotificationRecipients",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectOrganizationId",
                schema: "nyss",
                table: "AlertNotificationRecipients");
        }
    }
}
