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
            builder.Property(ns => ns.RegionCustomName).HasMaxLength(100);
            builder.Property(ns => ns.DistrictCustomName).HasMaxLength(100);
            builder.Property(ns => ns.VillageCustomName).HasMaxLength(100);
            builder.Property(ns => ns.ZoneCustomName).HasMaxLength(100);
            builder.HasOne(ns => ns.ContentLanguage);
        }
    }
}
