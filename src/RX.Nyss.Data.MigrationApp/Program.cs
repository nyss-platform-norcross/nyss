using System;
using System.Linq;
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

            try
            {
                MigrateNyssContext(dbConnectionString);
                MigrateApplicationDbContext(dbConnectionString);

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

        private static void MigrateApplicationDbContext(string dbConnectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(dbConnectionString);

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                context.Database.Migrate();
            }

            Console.WriteLine("Successfully migrated ApplicationDbContext");
        }
    }
}
