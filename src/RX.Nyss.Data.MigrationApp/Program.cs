using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Web.Data;

namespace RX.Nyss.Data.MigrationApp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("About to migrate NyssContext database");

            if (args.Length == 0)
            {
                throw new ArgumentException("Missing DbConnection string!");
            }

            var dbConnectionString = args[0];
            var createDemoData = args.Any(a => a == "createDemoData");
            var password = args.FirstOrDefault(a => a.StartsWith("password="))?.Split("=")[1];
            var adminPassword = args.FirstOrDefault(a => a.StartsWith("adminPassword="))?.Split("=")[1];

            try
            {
                MigrateNyssContext(dbConnectionString);
                MigrateApplicationDbContext(dbConnectionString, adminPassword);

                if (createDemoData)
                {
                    Console.WriteLine("About to add demo data...");
                    DemoDataCreator.CreateDemoData(dbConnectionString, password);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Didn't succeed in applying migrations: {ex.Message}");

                throw new Exception("Failed to apply migrations!", ex);
            }
        }

        private static void MigrateNyssContext(string dbConnectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NyssContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, x => x.UseNetTopologySuite());

            using (var context = new NyssContext(optionsBuilder.Options))
            {
                context.Database.Migrate();
            }

            Console.WriteLine("Successfully migrated NyssContext");
        }

        private static void MigrateApplicationDbContext(string dbConnectionString, string adminPassword)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(dbConnectionString);

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                context.Database.Migrate();
                var hasher = new PasswordHasher<IdentityUser>();

                if (!string.IsNullOrEmpty(adminPassword))
                {
                    Console.WriteLine("Updating admin pwd...");

                    var adminUser = context.Users.Single(u => u.Id == "9c1071c1-fa69-432a-9cd0-2c4baa703a67");
                    adminUser.PasswordHash = hasher.HashPassword(adminUser, adminPassword);

                    context.SaveChanges();
                }
            }

            Console.WriteLine("Successfully migrated ApplicationDbContext");
        }
    }
}
