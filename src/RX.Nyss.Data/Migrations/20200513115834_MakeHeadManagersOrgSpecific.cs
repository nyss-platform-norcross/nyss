using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class MakeHeadManagersOrgSpecific : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NationalSocieties_Users_HeadManagerId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropForeignKey(
                name: "FK_NationalSocieties_Users_PendingHeadManagerId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropIndex(
                name: "IX_NationalSocieties_HeadManagerId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropIndex(
                name: "IX_NationalSocieties_PendingHeadManagerId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.AddColumn<int>(
                name: "HeadManagerId",
                schema: "nyss",
                table: "Organizations",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PendingHeadManagerId",
                schema: "nyss",
                table: "Organizations",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultOrganizationId",
                schema: "nyss",
                table: "NationalSocieties",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE Nses set Nses.DefaultOrganizationId =
                (SELECT top(1) Id FROM [nyss].[Organizations] o where Nses.Id = o.NationalSocietyId)
                FROM [nyss].[NationalSocieties] Nses");

            migrationBuilder.Sql(@"
                UPDATE Org set Org.HeadManagerId = Ns.HeadManagerId, Org.PendingHeadManagerId = Ns.PendingHeadManagerId
                FROM [nyss].[Organizations] Org JOIN [nyss].[NationalSocieties] Ns ON Org.Id = Ns.DefaultOrganizationId"
            );

            migrationBuilder.DropColumn(
                name: "HeadManagerId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropColumn(
                name: "PendingHeadManagerId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_HeadManagerId",
                schema: "nyss",
                table: "Organizations",
                column: "HeadManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_PendingHeadManagerId",
                schema: "nyss",
                table: "Organizations",
                column: "PendingHeadManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_DefaultOrganizationId",
                schema: "nyss",
                table: "NationalSocieties",
                column: "DefaultOrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_NationalSocieties_Organizations_DefaultOrganizationId",
                schema: "nyss",
                table: "NationalSocieties",
                column: "DefaultOrganizationId",
                principalSchema: "nyss",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Users_HeadManagerId",
                schema: "nyss",
                table: "Organizations",
                column: "HeadManagerId",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Users_PendingHeadManagerId",
                schema: "nyss",
                table: "Organizations",
                column: "PendingHeadManagerId",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NationalSocieties_Organizations_DefaultOrganizationId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Users_HeadManagerId",
                schema: "nyss",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Users_PendingHeadManagerId",
                schema: "nyss",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_HeadManagerId",
                schema: "nyss",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_PendingHeadManagerId",
                schema: "nyss",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_NationalSocieties_DefaultOrganizationId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.DropColumn(
                name: "HeadManagerId",
                schema: "nyss",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "PendingHeadManagerId",
                schema: "nyss",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "DefaultOrganizationId",
                schema: "nyss",
                table: "NationalSocieties");

            migrationBuilder.AddColumn<int>(
                name: "HeadManagerId",
                schema: "nyss",
                table: "NationalSocieties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PendingHeadManagerId",
                schema: "nyss",
                table: "NationalSocieties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_HeadManagerId",
                schema: "nyss",
                table: "NationalSocieties",
                column: "HeadManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_PendingHeadManagerId",
                schema: "nyss",
                table: "NationalSocieties",
                column: "PendingHeadManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_NationalSocieties_Users_HeadManagerId",
                schema: "nyss",
                table: "NationalSocieties",
                column: "HeadManagerId",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NationalSocieties_Users_PendingHeadManagerId",
                schema: "nyss",
                table: "NationalSocieties",
                column: "PendingHeadManagerId",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
