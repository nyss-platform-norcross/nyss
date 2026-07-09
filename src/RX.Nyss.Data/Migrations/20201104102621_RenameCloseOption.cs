using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class RenameCloseOption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EscalatedOutcome",
                schema: "nyss",
                table: "Alerts",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql(@"
            UPDATE Alert SET Alert.EscalatedOutcome = Alert.CloseOption
            FROM nyss.Alerts AS Alert
            ");

            migrationBuilder.DropColumn(
                name: "CloseOption",
                schema: "nyss",
                table: "Alerts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CloseOption",
                schema: "nyss",
                table: "Alerts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql(@"
            UPDATE Alert SET Alert.CloseOption = Alert.EscalatedOutcome
            FROM nyss.Alerts AS Alert
            ");

            migrationBuilder.DropColumn(
                name: "EscalatedOutcome",
                schema: "nyss",
                table: "Alerts");

        }
    }
}
