using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddOrganization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "nyss",
                table: "UserNationalSocieties",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organizations",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    NationalSocietyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizations_NationalSocieties_NationalSocietyId",
                        column: x => x.NationalSocietyId,
                        principalSchema: "nyss",
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "Countries",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Andorra");

            migrationBuilder.CreateIndex(
                name: "IX_UserNationalSocieties_OrganizationId",
                schema: "nyss",
                table: "UserNationalSocieties",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_NationalSocietyId",
                schema: "nyss",
                table: "Organizations",
                column: "NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name_NationalSocietyId",
                schema: "nyss",
                table: "Organizations",
                columns: new[] { "Name", "NationalSocietyId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNationalSocieties_Organizations_OrganizationId",
                schema: "nyss",
                table: "UserNationalSocieties",
                column: "OrganizationId",
                principalSchema: "nyss",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                INSERT INTO [nyss].[Organizations] (Name, NationalSocietyId) SELECT Name+' Default Organization', Id FROM nyss.NationalSocieties

                UPDATE NatUsers set NatUsers.OrganizationId = 
                (SELECT top(1) Id from [nyss].[Organizations] o where NatUsers.NationalSocietyId = o.NationalSocietyId)
                FROM [nyss].[UserNationalSocieties] NatUsers"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNationalSocieties_Organizations_OrganizationId",
                schema: "nyss",
                table: "UserNationalSocieties");

            migrationBuilder.DropTable(
                name: "Organizations",
                schema: "nyss");

            migrationBuilder.DropIndex(
                name: "IX_UserNationalSocieties_OrganizationId",
                schema: "nyss",
                table: "UserNationalSocieties");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "nyss",
                table: "UserNationalSocieties");

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "Countries",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "AndorrA");
        }
    }
}
