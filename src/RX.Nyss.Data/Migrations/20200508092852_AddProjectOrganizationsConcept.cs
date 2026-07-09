using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddProjectOrganizationsConcept : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectOrganizations",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(nullable: false),
                    OrganizationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectOrganizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectOrganizations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "nyss",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectOrganizations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "nyss",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOrganizations_OrganizationId",
                schema: "nyss",
                table: "ProjectOrganizations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOrganizations_ProjectId_OrganizationId",
                schema: "nyss",
                table: "ProjectOrganizations",
                columns: new[] { "ProjectId", "OrganizationId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectOrganizations",
                schema: "nyss");
        }
    }
}
