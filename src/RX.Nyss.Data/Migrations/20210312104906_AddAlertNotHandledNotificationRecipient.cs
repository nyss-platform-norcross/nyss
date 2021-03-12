using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddAlertNotHandledNotificationRecipient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertNotHandledNotificationRecipients",
                schema: "nyss",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    ProjectId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertNotHandledNotificationRecipients", x => new { x.UserId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_AlertNotHandledNotificationRecipients_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "nyss",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertNotHandledNotificationRecipients_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertNotHandledNotificationRecipients_ProjectId",
                schema: "nyss",
                table: "AlertNotHandledNotificationRecipients",
                column: "ProjectId");

            migrationBuilder.Sql(@"
                INSERT INTO nyss.AlertNotHandledNotificationRecipients (UserId, ProjectId)
                SELECT o.[HeadManagerId], p.[Id]
                FROM [nyss].[ProjectOrganizations] as po
                INNER JOIN [nyss].[Organizations] as o on o.[Id]=po.[OrganizationId]
                INNER JOIN nyss.[Projects] as p on p.[Id]=po.[ProjectId]
                WHERE p.[EndDate] IS NULL
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertNotHandledNotificationRecipients",
                schema: "nyss");
        }
    }
}
