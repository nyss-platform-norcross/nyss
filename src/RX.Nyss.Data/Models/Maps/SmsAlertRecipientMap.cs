using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class SmsAlertRecipientMap : IEntityTypeConfiguration<SmsAlertRecipient>
    {
        public void Configure(EntityTypeBuilder<SmsAlertRecipient> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
            builder.HasOne(x => x.Project).WithMany(x => x.SmsAlertRecipients).IsRequired().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
