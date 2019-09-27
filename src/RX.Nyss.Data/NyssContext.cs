using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace RX.Nyss.Data
{
    public class NyssContext : DbContext
    {
        public DbSet<NationalSociety> NationalSocieties { get; set; }

        public NyssContext(DbContextOptions<NyssContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NationalSociety>().HasIndex(ns => ns.Name).IsUnique();
        }
    }

    public class NationalSociety
    {
        public int NationalSocietyId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }
    }
}