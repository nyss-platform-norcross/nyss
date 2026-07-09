using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReportsAndRawreportsWithNullValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_ResetById",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.AlterColumn<int>(
                name: "ResetById",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "RejectedById",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DataCollectorId",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CorrectedById",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AcceptedById",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ZoneId",
                schema: "nyss",
                table: "RawReports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "VillageId",
                schema: "nyss",
                table: "RawReports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "NationalSocietyId",
                schema: "nyss",
                table: "RawReports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DataCollectorId",
                schema: "nyss",
                table: "RawReports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_ResetById",
                schema: "nyss",
                table: "Reports",
                column: "ResetById",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_ResetById",
                schema: "nyss",
                table: "Reports");

            migrationBuilder.AlterColumn<int>(
                name: "ResetById",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RejectedById",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: false);

            migrationBuilder.AlterColumn<int>(
                name: "DataCollectorId",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: false);

            migrationBuilder.AlterColumn<int>(
                name: "CorrectedById",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: false);

            migrationBuilder.AlterColumn<int>(
                name: "AcceptedById",
                schema: "nyss",
                table: "Reports",
                type: "int",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: false);

            migrationBuilder.AlterColumn<int>(
                name: "ZoneId",
                schema: "nyss",
                table: "RawReports",
                type: "int",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VillageId",
                schema: "nyss",
                table: "RawReports",
                type: "int",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NationalSocietyId",
                schema: "nyss",
                table: "RawReports",
                type: "int",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DataCollectorId",
                schema: "nyss",
                table: "RawReports",
                type: "int",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_ResetById",
                schema: "nyss",
                table: "Reports",
                column: "ResetById",
                principalSchema: "nyss",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
