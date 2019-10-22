using Microsoft.EntityFrameworkCore.Migrations;

namespace RX.Nyss.Web.Migrations
{
    public partial class ApplicationDbContext_AddRolesAndSystemAdministratorAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT [identity].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'b0091a03-ffaf-44f4-ac70-df9fcd295457', N'Administrator', N'ADMINISTRATOR', N'd22b1211-071e-4e29-8b73-9264cd5dc187')");
            migrationBuilder.Sql("INSERT [identity].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'b583d8c9-3feb-4e08-834c-c604e7481fae', N'DataConsumer', N'DATACONSUMER', N'adc021a7-51c9-4a29-918e-8a562253074c')");
            migrationBuilder.Sql("INSERT [identity].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'cca2c07b-c90f-46d2-8413-1dc6ee51e63a', N'GlobalCoordinator', N'GLOBALCOORDINATOR', N'f0f67ee3-a803-4d1d-af27-966db72d5746')");
            migrationBuilder.Sql("INSERT [identity].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'cef5482a-383c-4eaa-a23f-16577fc8d34b', N'Supervisor', N'SUPERVISOR', N'dd7ea34c-cffe-47ec-aa98-2dd668668070')");
            migrationBuilder.Sql("INSERT [identity].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'f6afa341-af08-4b8a-a6fd-7ca33382a440', N'TechnicalAdvisor', N'TECHNICALADVISOR', N'0b5dddf5-5008-48b3-8c9d-1831a1b7149a')");
            migrationBuilder.Sql("INSERT [identity].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'fb23c271-059a-4537-ae3e-7f9fd5b305c6', N'DataManager', N'DATAMANAGER', N'8d27b561-f2ad-47cf-bd3a-e614995fffce')");

            migrationBuilder.Sql("INSERT [identity].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'9c1071c1-fa69-432a-9cd0-2c4baa703a67', N'admin@domain.com', N'ADMIN@DOMAIN.COM', N'admin@domain.com', N'ADMIN@DOMAIN.COM', 1, N'AQAAAAEAACcQAAAAECR5Ja6EyNSJUjBYPQPOJjW5JP2XoVuOx6MsCjcntc5XANwVwvwPUjsHvNG8qhcO3g==', N'3Q4P4PMC46O7CGQNZDPNQZDLOT23NLRV', N'6ac53d5d-db24-4b4a-bacf-947b456dbe64', NULL, 0, 0, NULL, 1, 0)");

            migrationBuilder.Sql("INSERT [identity].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'9c1071c1-fa69-432a-9cd0-2c4baa703a67', N'b0091a03-ffaf-44f4-ac70-df9fcd295457')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [identity].[AspNetRoles] WHERE [Id]='b0091a03-ffaf-44f4-ac70-df9fcd295457'");
            migrationBuilder.Sql("DELETE FROM [identity].[AspNetRoles] WHERE [Id]='b583d8c9-3feb-4e08-834c-c604e7481fae'");
            migrationBuilder.Sql("DELETE FROM [identity].[AspNetRoles] WHERE [Id]='cca2c07b-c90f-46d2-8413-1dc6ee51e63a'");
            migrationBuilder.Sql("DELETE FROM [identity].[AspNetRoles] WHERE [Id]='cef5482a-383c-4eaa-a23f-16577fc8d34b'");
            migrationBuilder.Sql("DELETE FROM [identity].[AspNetRoles] WHERE [Id]='f6afa341-af08-4b8a-a6fd-7ca33382a440'");
            migrationBuilder.Sql("DELETE FROM [identity].[AspNetRoles] WHERE [Id]='fb23c271-059a-4537-ae3e-7f9fd5b305c6'");

            migrationBuilder.Sql("DELETE FROM [identity].[AspNetUsers] WHERE [Id]='9c1071c1-fa69-432a-9cd0-2c4baa703a67'");

            migrationBuilder.Sql("DELETE FROM [identity].[AspNetUserRoles] WHERE [UserId]='9c1071c1-fa69-432a-9cd0-2c4baa703a67' AND [RoleId]='b0091a03-ffaf-44f4-ac70-df9fcd295457'");
        }
    }
}
