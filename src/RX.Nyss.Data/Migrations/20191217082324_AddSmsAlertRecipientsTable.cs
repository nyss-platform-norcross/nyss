using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddSmsAlertRecipientsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailAlertRecipients",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailAddress = table.Column<string>(maxLength: 100, nullable: false),
                    ProjectId = table.Column<int>(nullable: false)
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(maxLength: 20, nullable: false),
                    ProjectId = table.Column<int>(nullable: false)
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

            migrationBuilder.Sql("INSERT INTO [nyss].[EmailAlertRecipients] ([EmailAddress], [ProjectId]) SELECT [EmailAddress], [ProjectId] FROM [nyss].[AlertRecipients]");

            migrationBuilder.DropTable(
                name: "AlertRecipients",
                schema: "nyss");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertRecipients",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertRecipients_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "nyss",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertRecipients_ProjectId",
                schema: "nyss",
                table: "AlertRecipients",
                column: "ProjectId");

            migrationBuilder.Sql("INSERT INTO [nyss].[AlertRecipients] ([EmailAddress], [ProjectId]) SELECT [EmailAddress], [ProjectId] FROM [nyss].[EmailAlertRecipients]");

            migrationBuilder.DropTable(
                name: "EmailAlertRecipients",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "SmsAlertRecipients",
                schema: "nyss");
        }
    }
}
