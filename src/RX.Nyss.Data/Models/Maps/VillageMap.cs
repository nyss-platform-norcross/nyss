using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class VillageMap : IEntityTypeConfiguration<Village>
    {
        public void Configure(EntityTypeBuilder<Village> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.District).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict).OnDelete(DeleteBehavior.Restrict);
            builder.Property(x => x.Name).HasMaxLength(100);
        }
    }
}
