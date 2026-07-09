using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RX.Nyss.Web.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>, IDataProtectionKeyContext
{
    public ApplicationDbContext(
        DbContextOptions options) : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.Seed();
    }
}
