using System.Linq;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data
{
    public class NyssContext : DbContext
    {
        public NyssContext(DbContextOptions<NyssContext> options)
            : base(options)
        {
        }

        public DbSet<NationalSociety> NationalSocieties { get; set; }
        public DbSet<ContentLanguage> ContentLanguages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NyssContext).Assembly);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
