using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace RX.Nyss.Web.Data
{
    public static class ApplicationDbContextSeeder
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            SeedRoles(modelBuilder);
            SeedUsers(modelBuilder);
            SeedUserRoles(modelBuilder);
        }

        private static void SeedUserRoles(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                UserId = "9c1071c1-fa69-432a-9cd0-2c4baa703a67", RoleId = "b0091a03-ffaf-44f4-ac70-df9fcd295457"
            });

        private static void SeedUsers(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<IdentityUser>().HasData(
                new IdentityUser
                {
                    Id = "9c1071c1-fa69-432a-9cd0-2c4baa703a67",
                    UserName = "admin@domain.com",
                    NormalizedUserName = "ADMIN@DOMAIN.COM",
                    Email = "admin@domain.com",
                    NormalizedEmail = "ADMIN@DOMAIN.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAEAACcQAAAAECR5Ja6EyNSJUjBYPQPOJjW5JP2XoVuOx6MsCjcntc5XANwVwvwPUjsHvNG8qhcO3g==",
                    SecurityStamp = "3Q4P4PMC46O7CGQNZDPNQZDLOT23NLRV",
                    ConcurrencyStamp = "6ac53d5d-db24-4b4a-bacf-947b456dbe64",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                });

        private static void SeedRoles(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "b0091a03-ffaf-44f4-ac70-df9fcd295457",
                    Name = "Administrator",
                    NormalizedName = "ADMINISTRATOR",
                    ConcurrencyStamp = "d22b1211-071e-4e29-8b73-9264cd5dc187"
                },
                new IdentityRole
                {
                    Id = "b583d8c9-3feb-4e08-834c-c604e7481fae",
                    Name = "DataConsumer",
                    NormalizedName = "DATACONSUMER",
                    ConcurrencyStamp = "adc021a7-51c9-4a29-918e-8a562253074c"
                },
                new IdentityRole
                {
                    Id = "cca2c07b-c90f-46d2-8413-1dc6ee51e63a",
                    Name = "GlobalCoordinator",
                    NormalizedName = "GLOBALCOORDINATOR",
                    ConcurrencyStamp = "f0f67ee3-a803-4d1d-af27-966db72d5746"
                },
                new IdentityRole
                {
                    Id = "cef5482a-383c-4eaa-a23f-16577fc8d34b", Name = "Supervisor", NormalizedName = "SUPERVISOR", ConcurrencyStamp = "dd7ea34c-cffe-47ec-aa98-2dd668668070"
                },
                new IdentityRole
                {
                    Id = "f6afa341-af08-4b8a-a6fd-7ca33382a440",
                    Name = "TechnicalAdvisor",
                    NormalizedName = "TECHNICALADVISOR",
                    ConcurrencyStamp = "0b5dddf5-5008-48b3-8c9d-1831a1b7149a"
                },
                new IdentityRole
                {
                    Id = "fb23c271-059a-4537-ae3e-7f9fd5b305c6", Name = "Manager", NormalizedName = "MANAGER", ConcurrencyStamp = "8d27b561-f2ad-47cf-bd3a-e614995fffce"
                });
    }
}
