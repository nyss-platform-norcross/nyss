using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddGatewayModem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GatewayModems",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModemId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    GatewaySettingId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewayModems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GatewayModems_GatewaySettings_GatewaySettingId",
                        column: x => x.GatewaySettingId,
                        principalSchema: "nyss",
                        principalTable: "GatewaySettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GatewayModems_GatewaySettingId",
                schema: "nyss",
                table: "GatewayModems",
                column: "GatewaySettingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GatewayModems",
                schema: "nyss");
        }
    }
}
