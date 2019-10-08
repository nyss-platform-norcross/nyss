using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class LocalizedTemplateMap : IEntityTypeConfiguration<LocalizedTemplate>
    {
        public void Configure(EntityTypeBuilder<LocalizedTemplate> builder)
        {
            builder.HasOne(u => u.ApplicationLanguage).WithMany()
                .HasForeignKey(u => u.ApplicationLanguageId).IsRequired();
            builder.HasKey(loc => new { loc.ApplicationLanguageId, loc.Key });
            builder.Property(lt => lt.Value).HasMaxLength(2000);
        }
    }
}
