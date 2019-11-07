using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class ZoneMap : IEntityTypeConfiguration<Zone>
    {
        public void Configure(EntityTypeBuilder<Zone> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Village).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict).OnDelete(DeleteBehavior.Restrict);
            builder.Property(x => x.Name).HasMaxLength(100);
        }
    }
}
