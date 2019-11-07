using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class HealthRiskLanguageContentMap : IEntityTypeConfiguration<HealthRiskLanguageContent>
    {
        public void Configure(EntityTypeBuilder<HealthRiskLanguageContent> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.CaseDefinition).HasMaxLength(500).IsRequired();
            builder.Property(x => x.FeedbackMessage).HasMaxLength(160).IsRequired();
            builder.HasOne(x => x.HealthRisk).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ContentLanguage).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}
