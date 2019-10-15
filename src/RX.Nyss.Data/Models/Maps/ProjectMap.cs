using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class ProjectMap : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(200);
            builder.Property(x => x.State).HasMaxLength(50).HasConversion<string>();
            builder.Property(x => x.TimeZone).HasMaxLength(50);
            builder.HasOne(x => x.NationalSociety).WithMany().IsRequired();
            builder.HasOne(x => x.ContentLanguage).WithMany().IsRequired();
        }
    }
}
