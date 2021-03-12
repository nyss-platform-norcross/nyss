using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class AlertNotHandledNotificationRecipientMap : IEntityTypeConfiguration<AlertNotHandledNotificationRecipient>
    {
        public void Configure(EntityTypeBuilder<AlertNotHandledNotificationRecipient> builder)
        {
            builder.HasKey(x => new
            {
                x.UserId,
                x.ProjectId
            });

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Project)
                .WithMany(p => p.AlertNotHandledNotificationRecipients)
                .HasForeignKey(x => x.ProjectId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
