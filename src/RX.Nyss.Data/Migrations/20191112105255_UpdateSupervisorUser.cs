using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class UpdateSupervisorUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ManagerUserId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Villages_VillageId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Zones_ZoneId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ManagerUserId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_VillageId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ZoneId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ManagerUserId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VillageId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Sex",
                schema: "nyss",
                table: "Users",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecadeOfBirth",
                schema: "nyss",
                table: "Users",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupervisorUserProjects",
                schema: "nyss",
                columns: table => new
                {
                    SupervisorUserId = table.Column<int>(nullable: false),
                    ProjectId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupervisorUserProjects", x => new { x.SupervisorUserId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_SupervisorUserProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "nyss",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupervisorUserProjects_Users_SupervisorUserId",
                        column: x => x.SupervisorUserId,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorUserProjects_ProjectId",
                schema: "nyss",
                table: "SupervisorUserProjects",
                column: "ProjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupervisorUserProjects",
                schema: "nyss");

            migrationBuilder.DropColumn(
                name: "DecadeOfBirth",
                schema: "nyss",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Sex",
                schema: "nyss",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManagerUserId",
                schema: "nyss",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VillageId",
                schema: "nyss",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ZoneId",
                schema: "nyss",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ManagerUserId",
                schema: "nyss",
                table: "Users",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_VillageId",
                schema: "nyss",
                table: "Users",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ZoneId",
                schema: "nyss",
                table: "Users",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ManagerUserId",
                schema: "nyss",
                table: "Users",
                column: "ManagerUserId",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Villages_VillageId",
                schema: "nyss",
                table: "Users",
                column: "VillageId",
                principalSchema: "nyss",
                principalTable: "Villages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Zones_ZoneId",
                schema: "nyss",
                table: "Users",
                column: "ZoneId",
                principalSchema: "nyss",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
