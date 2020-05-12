using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class AlertNotificationRecipientMap : IEntityTypeConfiguration<AlertNotificationRecipient>
    {
        public void Configure(EntityTypeBuilder<AlertNotificationRecipient> builder)
        {
            builder.HasKey(anr => anr.Id);
            builder.Property(anr => anr.Email).HasMaxLength(100);
            builder.Property(anr => anr.PhoneNumber).HasMaxLength(100);
            builder.Property(anr => anr.Organization).HasMaxLength(100).IsRequired();
            builder.Property(anr => anr.Role).HasMaxLength(100).IsRequired();
            builder.Property(anr => anr.ProjectId).IsRequired();
        }
    }
}
