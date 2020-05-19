using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddProjectOrganizationToAlertRecipient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "nyss",
                table: "AlertNotificationRecipients",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE AlertRecipient SET AlertRecipient.OrganizationId =
                (SELECT TOP(1) Id FROM nyss.ProjectOrganizations po WHERE AlertRecipient.ProjectId=po.ProjectId)
                FROM nyss.AlertNotificationRecipients AlertRecipient
            ");

            migrationBuilder.Sql(@"
                DELETE sar
                FROM [nyssDemo].[nyss].[SupervisorUserAlertRecipients] sar
                INNER JOIN [nyssDemo].[nyss].[Users] as u ON sar.SupervisorId=u.Id
                WHERE u.DeletedAt IS NOT NULL
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "nyss",
                table: "AlertNotificationRecipients");
        }
    }
}
