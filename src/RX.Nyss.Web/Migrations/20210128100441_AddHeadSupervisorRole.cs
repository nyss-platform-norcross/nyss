using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Web.Migrations
{
    public partial class AddHeadSupervisorRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "identity",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "5d0fdc73-8e87-4bb9-b98b-1764a4b03a39", "5d0fdc73-8e87-4bb9-b98b-1764a4b03a39", "HeadSupervisor", "HEADSUPERVISOR" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5d0fdc73-8e87-4bb9-b98b-1764a4b03a39");
        }
    }
}
