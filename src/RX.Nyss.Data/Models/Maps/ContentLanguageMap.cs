using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class ContentLanguageMap : IEntityTypeConfiguration<ContentLanguage>
    {
        public void Configure(EntityTypeBuilder<ContentLanguage> builder)
        {
            builder.HasKey(cl => cl.Id);
            builder.Property(al => al.LanguageCode).HasMaxLength(10);
            builder.HasIndex(al => al.DisplayName).IsUnique();
            builder.Property(al => al.DisplayName).IsRequired().HasMaxLength(100);
        }
    }
}
