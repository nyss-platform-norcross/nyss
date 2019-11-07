using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class RemoveNationalSocietyForeignKeysFromUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_NationalSocieties_NationalSocietyId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_NationalSocieties_SupervisorUser_NationalSocietyId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_NationalSocietyId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SupervisorUser_NationalSocietyId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NationalSocietyId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SupervisorUser_NationalSocietyId",
                schema: "nyss",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NationalSocietyId",
                schema: "nyss",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorUser_NationalSocietyId",
                schema: "nyss",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NationalSocietyId",
                schema: "nyss",
                table: "Users",
                column: "NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SupervisorUser_NationalSocietyId",
                schema: "nyss",
                table: "Users",
                column: "SupervisorUser_NationalSocietyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_NationalSocieties_NationalSocietyId",
                schema: "nyss",
                table: "Users",
                column: "NationalSocietyId",
                principalSchema: "nyss",
                principalTable: "NationalSocieties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_NationalSocieties_SupervisorUser_NationalSocietyId",
                schema: "nyss",
                table: "Users",
                column: "SupervisorUser_NationalSocietyId",
                principalSchema: "nyss",
                principalTable: "NationalSocieties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
