using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Web.Migrations
{
    public partial class AddedCoordinatorRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "identity",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "8e2028a0-bb1a-4297-4ed1-e08e4f2b6735", "9b639463-727b-6ba7-4416-dc8a8d8fdbfc", "Coordinator", "COORDINATOR" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8e2028a0-bb1a-4297-4ed1-e08e4f2b6735");
        }
    }
}
