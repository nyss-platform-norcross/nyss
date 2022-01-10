using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class Renamealertstatuspending : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE a SET a.[Status]='Open'
                FROM nyss.[Alerts] as a
                WHERE a.[Status]='Pending'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE a SET a.[Status]='Pending'
                FROM nyss.[Alerts] as a
                WHERE a.[Status]='Open'
            ");
        }
    }
}
