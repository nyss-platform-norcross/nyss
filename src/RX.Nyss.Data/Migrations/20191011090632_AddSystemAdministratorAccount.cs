using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Data.Migrations
{
    public partial class AddSystemAdministratorAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT [dbo].[Users] ([IdentityUserId], [Name], [Role], [EmailAddress], [PhoneNumber], [AdditionalPhoneNumber], [Organization], [IsFirstLogin], [ApplicationLanguageId], [IsDataOwner], [HasConsented], [ConsentedAt], [NationalSocietyId], [Sex], [SupervisorUser_NationalSocietyId], [VillageId], [ZoneId], [DataManagerUserId]) 
                                    VALUES(N'9c1071c1-fa69-432a-9cd0-2c4baa703a67', N'Administrator', N'Administrator', N'admin@domain.com', N'', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [dbo].[Users] WHERE [IdentityUserId]='9c1071c1-fa69-432a-9cd0-2c4baa703a67'");
        }
    }
}
