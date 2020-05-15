using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class NationalSocietyMap : IEntityTypeConfiguration<NationalSociety>
    {
        public void Configure(EntityTypeBuilder<NationalSociety> builder)
        {
            builder.HasKey(ns => ns.Id);
            builder.HasIndex(ns => ns.Name).IsUnique();
            builder.Property(ns => ns.Name).HasMaxLength(100);
            builder.Property(ns => ns.StartDate).IsRequired();
            builder.Property(ns => ns.IsArchived).IsRequired();
            builder.Property(ns => ns.RegionCustomName).HasMaxLength(100);
            builder.Property(ns => ns.DistrictCustomName).HasMaxLength(100);
            builder.Property(ns => ns.VillageCustomName).HasMaxLength(100);
            builder.Property(ns => ns.ZoneCustomName).HasMaxLength(100);
            builder.HasOne(ns => ns.ContentLanguage).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(ns => ns.Country).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(ns => ns.RawReports).WithOne(rr => rr.NationalSociety).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(ns => ns.DefaultOrganization).WithMany().HasForeignKey(x => x.DefaultOrganizationId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ns => ns.IsArchived);
            builder.HasIndex(ns => ns.StartDate);
        }
    }
}
