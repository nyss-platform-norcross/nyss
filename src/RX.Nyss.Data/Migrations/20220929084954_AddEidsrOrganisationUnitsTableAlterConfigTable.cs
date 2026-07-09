using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    public partial class AddEidsrOrganisationUnitsTableAlterConfigTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
              name: "Username",
              schema: "nyss",
              table: "EidsrConfiguration",
              type: "varchar(256)",
              nullable: true,
              oldClrType: typeof(string),
              oldType: "nvarchar(256)",
              oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrackerProgramId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SuspectedDiseaseDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumberDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LocationDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GenderDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EventTypeDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateOfOnsetDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "varchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "EidsrOrganisationUnits",
                schema: "nyss",
                columns: table => new
                {
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    OrganisationUnitId = table.Column<string>(type: "varchar(256)", nullable: true),
                    OrganisationUnitName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EidsrOrganisationUnits", x => x.DistrictId);
                    table.ForeignKey(
                        name: "FK_EidsrOrganisationUnits_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalSchema: "nyss",
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EidsrOrganisationUnits",
                schema: "nyss");

            migrationBuilder.AlterColumn<string>(
              name: "Username",
              schema: "nyss",
              table: "EidsrConfiguration",
              type: "nvarchar(256)",
              nullable: true,
              oldClrType: typeof(string),
              oldType: "varchar(256)",
              oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrackerProgramId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SuspectedDiseaseDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumberDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LocationDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GenderDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EventTypeDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateOfOnsetDataElementId",
                schema: "nyss",
                table: "EidsrConfiguration",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldNullable: true);
        }
    }
}
