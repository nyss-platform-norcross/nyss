using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class EmailAlertRecipientMap : IEntityTypeConfiguration<EmailAlertRecipient>
    {
        public void Configure(EntityTypeBuilder<EmailAlertRecipient> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EmailAddress).IsRequired().HasMaxLength(100);
            builder.HasOne(x => x.Project).WithMany(x => x.EmailAlertRecipients).IsRequired().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
