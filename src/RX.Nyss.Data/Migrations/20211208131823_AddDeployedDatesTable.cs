using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddDeployedDatesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataCollectorNotDeployedDates",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataCollectorId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataCollectorNotDeployedDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataCollectorNotDeployedDates_DataCollectors_DataCollectorId",
                        column: x => x.DataCollectorId,
                        principalSchema: "nyss",
                        principalTable: "DataCollectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectorNotDeployedDates_DataCollectorId",
                schema: "nyss",
                table: "DataCollectorNotDeployedDates",
                column: "DataCollectorId");

            migrationBuilder.Sql(@"
                INSERT INTO [nyss].[DataCollectorNotDeployedDates] ([DataCollectorId], [StartDate])
                SELECT [Id] as DataCollectorId,
                  GETUTCDATE() as StartDate
                FROM [nyss].[DataCollectors]
                WHERE [Deployed]=0
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataCollectorNotDeployedDates",
                schema: "nyss");
        }
    }
}
