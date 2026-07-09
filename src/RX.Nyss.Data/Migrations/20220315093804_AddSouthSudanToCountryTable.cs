using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    public partial class AddSouthSudanToCountryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "nyss",
                table: "Countries",
                columns: new[] { "Id", "CountryCode", "Name" },
                values: new object[] { 243, "SS", "South Sudan" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "nyss",
                table: "Countries",
                keyColumn: "Id",
                keyValue: 243);
        }
    }
}
