using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class HealthRiskMap : IEntityTypeConfiguration<HealthRisk>
    {
        public void Configure(EntityTypeBuilder<HealthRisk> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.HealthRiskType)
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(x => x.HealthRiskCode)
                .IsRequired();

            builder.HasOne(x => x.AlertRule)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.LanguageContents)
                .WithOne(x => x.HealthRisk)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.HealthRiskType);
        }
    }
}
