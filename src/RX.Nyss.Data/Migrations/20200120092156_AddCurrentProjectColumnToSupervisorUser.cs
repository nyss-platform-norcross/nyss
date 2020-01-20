using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddCurrentProjectColumnToSupervisorUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentProjectId",
                schema: "nyss",
                table: "Users",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CurrentProjectId",
                schema: "nyss",
                table: "Users",
                column: "CurrentProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Projects_CurrentProjectId",
                schema: "nyss",
                table: "Users",
                column: "CurrentProjectId",
                principalSchema: "nyss",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"UPDATE u 
                                    SET u.CurrentProjectId = sup.ProjectId
                                    FROM
                                    [nyss].[Users] u
                                    INNER JOIN [nyss].[SupervisorUserProjects] sup
                                    ON u.Id = sup.SupervisorUserId
                                    INNER JOIN [nyss].[Projects] p
                                    ON p.Id = sup.ProjectId
                                    WHERE p.State = 'Open'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Projects_CurrentProjectId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CurrentProjectId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CurrentProjectId",
                schema: "nyss",
                table: "Users");
        }
    }
}
