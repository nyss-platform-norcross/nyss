using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class ProjectHealthRiskMap : IEntityTypeConfiguration<ProjectHealthRisk>
    {
        public void Configure(EntityTypeBuilder<ProjectHealthRisk> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FeedbackMessage)
                .HasMaxLength(160);

            builder.Property(x => x.CaseDefinition)
                .HasMaxLength(500);

            builder.HasOne(x => x.Project)
                .WithMany(x => x.ProjectHealthRisks)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HealthRisk)
                .WithMany()
                .HasForeignKey(phr => phr.HealthRiskId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AlertRule)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
