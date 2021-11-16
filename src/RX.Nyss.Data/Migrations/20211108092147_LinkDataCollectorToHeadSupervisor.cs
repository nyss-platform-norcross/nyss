using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class LinkDataCollectorToHeadSupervisor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HeadSupervisorId",
                schema: "nyss",
                table: "DataCollectors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_HeadSupervisorId",
                schema: "nyss",
                table: "DataCollectors",
                column: "HeadSupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataCollectors_Users_HeadSupervisorId",
                schema: "nyss",
                table: "DataCollectors",
                column: "HeadSupervisorId",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataCollectors_Users_HeadSupervisorId",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropIndex(
                name: "IX_DataCollectors_HeadSupervisorId",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropColumn(
                name: "HeadSupervisorId",
                schema: "nyss",
                table: "DataCollectors");
        }
    }
}
