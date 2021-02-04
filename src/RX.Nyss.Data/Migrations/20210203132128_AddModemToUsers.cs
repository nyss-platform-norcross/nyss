using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddModemToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModemId",
                schema: "nyss",
                table: "Users",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TechnicalAdvisorUserGatewayModems",
                schema: "nyss",
                columns: table => new
                {
                    TechnicalAdvisorUserId = table.Column<int>(nullable: false),
                    GatewayModemId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalAdvisorUserGatewayModems", x => new { x.TechnicalAdvisorUserId, x.GatewayModemId });
                    table.ForeignKey(
                        name: "FK_TechnicalAdvisorUserGatewayModems_GatewayModems_GatewayModemId",
                        column: x => x.GatewayModemId,
                        principalSchema: "nyss",
                        principalTable: "GatewayModems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TechnicalAdvisorUserGatewayModems_Users_TechnicalAdvisorUserId",
                        column: x => x.TechnicalAdvisorUserId,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ModemId",
                schema: "nyss",
                table: "Users",
                column: "ModemId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalAdvisorUserGatewayModems_GatewayModemId",
                schema: "nyss",
                table: "TechnicalAdvisorUserGatewayModems",
                column: "GatewayModemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_GatewayModem_GatewayModemId",
                schema: "nyss",
                table: "Users",
                column: "ModemId",
                principalSchema: "nyss",
                principalTable: "GatewayModems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_GatewayModem_GatewayModemId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropTable(
                name: "TechnicalAdvisorUserGatewayModems",
                schema: "nyss");

            migrationBuilder.DropIndex(
                name: "IX_Users_ModemId",
                schema: "nyss",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModemId",
                schema: "nyss",
                table: "Users");
        }
    }
}
