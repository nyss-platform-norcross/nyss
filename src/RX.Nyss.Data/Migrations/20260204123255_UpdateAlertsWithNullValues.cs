using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RX.Nyss.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAlertsWithNullValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EscalatedById",
                schema: "nyss",
                table: "Alerts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DismissedById",
                schema: "nyss",
                table: "Alerts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ClosedById",
                schema: "nyss",
                table: "Alerts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EscalatedById",
                schema: "nyss",
                table: "Alerts",
                type: "int",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DismissedById",
                schema: "nyss",
                table: "Alerts",
                type: "int",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ClosedById",
                schema: "nyss",
                table: "Alerts",
                type: "int",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
