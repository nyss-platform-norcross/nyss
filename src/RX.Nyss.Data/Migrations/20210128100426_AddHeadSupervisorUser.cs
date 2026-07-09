using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddHeadSupervisorUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Projects_CurrentProjectId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "HeadSupervisorId",
                schema: "nyss",
                table: "Users",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HeadSupervisorUserAlertRecipients",
                schema: "nyss",
                columns: table => new
                {
                    AlertNotificationRecipientId = table.Column<int>(nullable: false),
                    HeadSupervisorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeadSupervisorUserAlertRecipients", x => new { x.HeadSupervisorId, x.AlertNotificationRecipientId });
                    table.ForeignKey(
                        name: "FK_HeadSupervisorUserAlertRecipients_AlertNotificationRecipients_AlertNotificationRecipientId",
                        column: x => x.AlertNotificationRecipientId,
                        principalSchema: "nyss",
                        principalTable: "AlertNotificationRecipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HeadSupervisorUserAlertRecipients_Users_HeadSupervisorId",
                        column: x => x.HeadSupervisorId,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HeadSupervisorUserProjects",
                schema: "nyss",
                columns: table => new
                {
                    HeadSupervisorUserId = table.Column<int>(nullable: false),
                    ProjectId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeadSupervisorUserProjects", x => new { x.HeadSupervisorUserId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_HeadSupervisorUserProjects_Users_HeadSupervisorUserId",
                        column: x => x.HeadSupervisorUserId,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HeadSupervisorUserProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "nyss",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_HeadSupervisorId",
                schema: "nyss",
                table: "Users",
                column: "HeadSupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_HeadSupervisorUserAlertRecipients_AlertNotificationRecipientId",
                schema: "nyss",
                table: "HeadSupervisorUserAlertRecipients",
                column: "AlertNotificationRecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_HeadSupervisorUserProjects_ProjectId",
                schema: "nyss",
                table: "HeadSupervisorUserProjects",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Project_CurrentProjectId",
                schema: "nyss",
                table: "Users",
                column: "CurrentProjectId",
                principalSchema: "nyss",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_HeadSupervisorId",
                schema: "nyss",
                table: "Users",
                column: "HeadSupervisorId",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Project_CurrentProjectId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_HeadSupervisorId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropTable(
                name: "HeadSupervisorUserAlertRecipients",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "HeadSupervisorUserProjects",
                schema: "nyss");

            migrationBuilder.DropIndex(
                name: "IX_Users_HeadSupervisorId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HeadSupervisorId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Projects_CurrentProjectId",
                schema: "nyss",
                table: "Users",
                column: "CurrentProjectId",
                principalSchema: "nyss",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
