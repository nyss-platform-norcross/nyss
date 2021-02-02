using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class HeadSupervisorUserAlertRecipientMap : IEntityTypeConfiguration<HeadSupervisorUserAlertRecipient>
    {
        public void Configure(EntityTypeBuilder<HeadSupervisorUserAlertRecipient> builder)
        {
            builder.HasKey(x => new
            {
                x.HeadSupervisorId,
                x.AlertNotificationRecipientId
            });
            builder.HasOne(x => x.HeadSupervisor)
                .WithMany(x => x.HeadSupervisorUserAlertRecipients)
                .HasForeignKey(sar => sar.HeadSupervisorId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AlertNotificationRecipient)
                .WithMany(x => x.HeadSupervisorUserAlertRecipients)
                .HasForeignKey(sar => sar.AlertNotificationRecipientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
