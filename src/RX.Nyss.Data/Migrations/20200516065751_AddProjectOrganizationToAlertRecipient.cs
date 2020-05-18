using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddProjectOrganizationToAlertRecipient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectOrganizationId",
                schema: "nyss",
                table: "AlertNotificationRecipients",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE AlertRecipient SET AlertRecipient.ProjectOrganizationId = 
                (SELECT TOP(1) Id FROM nyss.ProjectOrganizations po WHERE AlertRecipient.ProjectId=po.ProjectId)
                FROM nyss.AlertNotificationRecipients AlertRecipient
            ");
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
