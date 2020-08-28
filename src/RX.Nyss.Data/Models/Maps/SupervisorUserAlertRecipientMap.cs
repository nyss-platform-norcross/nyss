using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class SupervisorUserAlertRecipientMap : IEntityTypeConfiguration<SupervisorUserAlertRecipient>
    {
        public void Configure(EntityTypeBuilder<SupervisorUserAlertRecipient> builder)
        {
            builder.HasKey(x => new
            {
                x.SupervisorId,
                x.AlertNotificationRecipientId
            });
            
            builder.HasOne(x => x.Supervisor)
                .WithMany(x => x.SupervisorAlertRecipients)
                .HasForeignKey(sar => sar.SupervisorId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AlertNotificationRecipient)
                .WithMany(x => x.SupervisorAlertRecipients)
                .HasForeignKey(sar => sar.AlertNotificationRecipientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
