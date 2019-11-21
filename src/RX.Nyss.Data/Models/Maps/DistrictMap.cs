using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class DistrictMap : IEntityTypeConfiguration<District>
    {
        public void Configure(EntityTypeBuilder<District> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.Villages).WithOne(x => x.District).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Property(x => x.Name).HasMaxLength(100);
        }
    }
}
