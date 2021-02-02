using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class LocalizationMap : IEntityTypeConfiguration<Localization>
    {
        public void Configure(EntityTypeBuilder<Localization> builder)
        {
            builder.HasOne(u => u.ApplicationLanguage)
                .WithMany()
                .HasForeignKey(u => u.ApplicationLanguageId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasKey(loc => new
            {
                loc.ApplicationLanguageId,
                loc.Key
            });

            builder.Property(loc => loc.Value)
                .HasMaxLength(100);
        }
    }
}
