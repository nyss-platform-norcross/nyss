using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddAlertNotificationRecipients : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertNotificationRecipients",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<string>(maxLength: 100, nullable: false),
                    Organization = table.Column<string>(maxLength: 100, nullable: false),
                    Email = table.Column<string>(maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(maxLength: 100, nullable: true),
                    ProjectId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertNotificationRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertNotificationRecipients_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "nyss",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupervisorUserAlertRecipients",
                schema: "nyss",
                columns: table => new
                {
                    AlertNotificationRecipientId = table.Column<int>(nullable: false),
                    SupervisorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupervisorUserAlertRecipients", x => new { x.SupervisorId, x.AlertNotificationRecipientId });
                    table.ForeignKey(
                        name: "FK_SupervisorUserAlertRecipients_AlertNotificationRecipients_AlertNotificationRecipientId",
                        column: x => x.AlertNotificationRecipientId,
                        principalSchema: "nyss",
                        principalTable: "AlertNotificationRecipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupervisorUserAlertRecipients_Users_SupervisorId",
                        column: x => x.SupervisorId,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertNotificationRecipients_ProjectId",
                schema: "nyss",
                table: "AlertNotificationRecipients",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorUserAlertRecipients_AlertNotificationRecipientId",
                schema: "nyss",
                table: "SupervisorUserAlertRecipients",
                column: "AlertNotificationRecipientId");

            migrationBuilder.Sql("INSERT INTO [nyss].[AlertNotificationRecipients] ([Role], [Organization], [Email], [ProjectId]) SELECT 'Role not set','Organization not set',[EmailAddress], [ProjectId] FROM [nyss].[EmailAlertRecipients]");
            migrationBuilder.Sql("INSERT INTO [nyss].[AlertNotificationRecipients] ([Role], [Organization], [PhoneNumber], [ProjectId]) SELECT 'Role not set','Organization not set',[PhoneNumber], [ProjectId] FROM [nyss].[SmsAlertRecipients]");

            migrationBuilder.Sql(@"
                INSERT INTO [nyss].SupervisorUserAlertRecipients (SupervisorId, AlertNotificationRecipientId)
                SELECT users.Id, alertRecipient.Id FROM [nyss].[Users] as users
                INNER JOIN [nyss].[AlertNotificationRecipients] as alertRecipient ON users.CurrentProjectId=alertRecipient.ProjectId
                WHERE users.Role = 'Supervisor'
            ");

            migrationBuilder.DropTable(
                name: "EmailAlertRecipients",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "SmsAlertRecipients",
                schema: "nyss");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailAlertRecipients",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAlertRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailAlertRecipients_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "nyss",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SmsAlertRecipients",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsAlertRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsAlertRecipients_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "nyss",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailAlertRecipients_ProjectId",
                schema: "nyss",
                table: "EmailAlertRecipients",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsAlertRecipients_ProjectId",
                schema: "nyss",
                table: "SmsAlertRecipients",
                column: "ProjectId");

            migrationBuilder.Sql("INSERT INTO [nyss].[EmailAlertRecipients] ([EmailAddress], [ProjectId]) SELECT [Email], [ProjectId] FROM [nyss].[AlertNotificationRecipients] WHERE [Email] IS NOT NULL");
            migrationBuilder.Sql("INSERT INTO [nyss].[SmsAlertRecipients] ([PhoneNumber], [ProjectId]) SELECT [PhoneNumber], [ProjectId] FROM [nyss].[AlertNotificationRecipients] WHERE [PhoneNumber] IS NOT NULL");

            migrationBuilder.DropTable(
                name: "SupervisorUserAlertRecipients",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "AlertNotificationRecipients",
                schema: "nyss");
        }
    }
}
