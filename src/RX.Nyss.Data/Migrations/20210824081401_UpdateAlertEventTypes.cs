using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class UpdateAlertEventTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertEventSubtypes_AlertEventTypes_AlertEventTypeId",
                schema: "nyss",
                table: "AlertEventSubtypes");

            migrationBuilder.Sql(@"
                UPDATE [nyss].[AlertEventLogs] SET [AlertEventTypeId]=4 WHERE [AlertEventTypeId]=2 OR [AlertEventTypeId]=3
            ");

            migrationBuilder.DeleteData(
                schema: "nyss",
                table: "AlertEventTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "nyss",
                table: "AlertEventTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AlertEventTypeId", "Name" },
                values: new object[] { 4, "PresumedCasePositiveLab" });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AlertEventTypeId", "Name" },
                values: new object[] { 4, "PresumedCasePositiveClinical" });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AlertEventTypeId", "Name" },
                values: new object[] { 4, "PresumedCasePositiveUnknown" });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AlertEventTypeId", "Name" },
                values: new object[] { 4, "PresumedCaseNegativeLab" });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AlertEventTypeId", "Name" },
                values: new object[] { 4, "PresumedCaseNegativeClinical" });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AlertEventTypeId", "Name" },
                values: new object[] { 4, "PresumedCaseNegativeUnknown" });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Summary");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertEventSubtypes_AlertEventTypes_AlertEventTypeId",
                schema: "nyss",
                table: "AlertEventSubtypes",
                column: "AlertEventTypeId",
                principalSchema: "nyss",
                principalTable: "AlertEventTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertEventSubtypes_AlertEventTypes_AlertEventTypeId",
                schema: "nyss",
                table: "AlertEventSubtypes");

            migrationBuilder.Sql(@"
                UPDATE [nyss].[AlertEventLogs] SET [AlertEventTypeId]=2 WHERE [AlertEventTypeId]=4 AND ([AlertEventSubtypeId]=4 OR [AlertEventSubtypeId]=5 OR [AlertEventSubtypeId]=6)
                UPDATE [nyss].[AlertEventLogs] SET [AlertEventTypeId]=3 WHERE [AlertEventTypeId]=4 AND ([AlertEventSubtypeId]=7 OR [AlertEventSubtypeId]=8 OR [AlertEventSubtypeId]=9)
            ");

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Details");

            migrationBuilder.InsertData(
                schema: "nyss",
                table: "AlertEventTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3, "PresumedCaseNegative" });

            migrationBuilder.InsertData(
                schema: "nyss",
                table: "AlertEventTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2, "PresumedCasePositive" });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AlertEventTypeId", "Name" },
                values: new object[] { 2, "Lab" });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AlertEventTypeId", "Name" },
                values: new object[] { 2, "Clinical" });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AlertEventTypeId", "Name" },
                values: new object[] { 2, "Unknown" });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AlertEventTypeId", "Name" },
                values: new object[] { 3, "Lab" });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AlertEventTypeId", "Name" },
                values: new object[] { 3, "Clinical" });

            migrationBuilder.UpdateData(
                schema: "nyss",
                table: "AlertEventSubtypes",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AlertEventTypeId", "Name" },
                values: new object[] { 3, "Unknown" });

            migrationBuilder.AddForeignKey(
                name: "FK_AlertEventSubtypes_AlertEventTypes_AlertEventTypeId",
                schema: "nyss",
                table: "AlertEventSubtypes",
                column: "AlertEventTypeId",
                principalSchema: "nyss",
                principalTable: "AlertEventTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
