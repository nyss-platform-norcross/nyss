using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddDataCollectorLocations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataCollectorLocations",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataCollectorId = table.Column<int>(nullable: false),
                    Location = table.Column<Point>(nullable: false),
                    VillageId = table.Column<int>(nullable: false),
                    ZoneId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataCollectorLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataCollectorLocations_DataCollectors_DataCollectorId",
                        column: x => x.DataCollectorId,
                        principalSchema: "nyss",
                        principalTable: "DataCollectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataCollectorLocations_Villages_VillageId",
                        column: x => x.VillageId,
                        principalSchema: "nyss",
                        principalTable: "Villages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataCollectorLocations_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalSchema: "nyss",
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectorLocations_DataCollectorId",
                schema: "nyss",
                table: "DataCollectorLocations",
                column: "DataCollectorId");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectorLocations_VillageId",
                schema: "nyss",
                table: "DataCollectorLocations",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectorLocations_ZoneId",
                schema: "nyss",
                table: "DataCollectorLocations",
                column: "ZoneId");

            migrationBuilder.Sql(@"
                INSERT INTO [nyss].[DataCollectorLocations] ([DataCollectorId], [Location], [VillageId], [ZoneId])
                SELECT [Id] as DataCollectorId
                      ,[Location]
                      ,[VillageId]
                      ,[ZoneId]
                FROM [nyss].[DataCollectors]
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_DataCollectors_Villages_VillageId",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropForeignKey(
                name: "FK_DataCollectors_Zones_ZoneId",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropIndex(
                name: "IX_DataCollectors_VillageId",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropIndex(
                name: "IX_DataCollectors_ZoneId",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropColumn(
                name: "VillageId",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                schema: "nyss",
                table: "DataCollectors");

            migrationBuilder.AlterColumn<Point>(
                name: "Location",
                schema: "nyss",
                table: "Reports",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geography");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<Point>(
                name: "Location",
                schema: "nyss",
                table: "Reports",
                type: "geography",
                nullable: false,
                oldClrType: typeof(Point),
                oldNullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                schema: "nyss",
                table: "DataCollectors",
                type: "geography",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "VillageId",
                schema: "nyss",
                table: "DataCollectors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ZoneId",
                schema: "nyss",
                table: "DataCollectors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_VillageId",
                schema: "nyss",
                table: "DataCollectors",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_ZoneId",
                schema: "nyss",
                table: "DataCollectors",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataCollectors_Villages_VillageId",
                schema: "nyss",
                table: "DataCollectors",
                column: "VillageId",
                principalSchema: "nyss",
                principalTable: "Villages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DataCollectors_Zones_ZoneId",
                schema: "nyss",
                table: "DataCollectors",
                column: "ZoneId",
                principalSchema: "nyss",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                UPDATE dc SET dc.[Location]=dcl.[Location], dc.[VillageId]=dcl.[VillageId], dc.[ZoneId]=dcl.[ZoneId]
                FROM [nyss].[DataCollectors] as dc
                CROSS APPLY (
                    SELECT TOP 1 dcl.[Location], dcl.[VillageId], dcl.[ZoneId]
                    FROM [nyss].[DataCollectorLocations] as dcl
                    WHERE dcl.[DataCollectorId]=dc.[Id]
                    ORDER BY dcl.[Id]
                )
            ");

            migrationBuilder.DropTable(
                name: "DataCollectorLocations",
                schema: "nyss");
        }
    }
}
