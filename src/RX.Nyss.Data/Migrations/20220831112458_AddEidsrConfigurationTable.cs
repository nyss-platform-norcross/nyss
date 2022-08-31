using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    public partial class AddEidsrConfigurationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EidsrConfiguration",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiBaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrackerProgramId = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    LocationDataElementId = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    DateOfOnsetDataElementId = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    PhoneNumberDataElementId = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    SuspectedDiseaseDataElementId = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    EventTypeDataElementId = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    GenderDataElementId = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    NationalSocietyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EidsrConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EidsrConfiguration_NationalSocieties_NationalSocietyId",
                        column: x => x.NationalSocietyId,
                        principalSchema: "nyss",
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EidsrConfiguration_NationalSocietyId",
                schema: "nyss",
                table: "EidsrConfiguration",
                column: "NationalSocietyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EidsrConfiguration",
                schema: "nyss");
        }
    }
}
