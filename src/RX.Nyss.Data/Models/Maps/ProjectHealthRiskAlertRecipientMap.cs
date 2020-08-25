using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class ProjectHealthRiskAlertRecipientMap : IEntityTypeConfiguration<ProjectHealthRiskAlertRecipient>
    {
        public void Configure(EntityTypeBuilder<ProjectHealthRiskAlertRecipient> builder)
        {
            builder.HasKey(x => new
            {
                x.ProjectHealthRiskId,
                x.AlertNotificationRecipientId
            });
            
            builder.HasOne(x => x.ProjectHealthRisk)
                .WithMany(x => x.ProjectHealthRiskAlertRecipients)
                .HasForeignKey(sar => sar.ProjectHealthRiskId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AlertNotificationRecipient)
                .WithMany(x => x.ProjectHealthRiskAlertRecipients)
                .HasForeignKey(sar => sar.AlertNotificationRecipientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
