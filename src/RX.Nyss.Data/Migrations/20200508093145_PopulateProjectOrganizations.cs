using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class PopulateProjectOrganizations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql(@"
                INSERT INTO nyss.ProjectOrganizations (ProjectId, OrganizationId)
                SELECT p.Id as ProjectId, (SELECT TOP 1 o.Id from nyss.Organizations o where o.NationalSocietyId = p.NationalSocietyId) as OrganizationId FROM nyss.Projects p
            ");

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
