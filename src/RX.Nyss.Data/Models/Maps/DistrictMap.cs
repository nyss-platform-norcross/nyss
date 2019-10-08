using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class DistrictMap : IEntityTypeConfiguration<District>
    {
        public void Configure(EntityTypeBuilder<District> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Region).WithMany().IsRequired();
            builder.Property(x => x.Name).HasMaxLength(100);
        }
    }
}
