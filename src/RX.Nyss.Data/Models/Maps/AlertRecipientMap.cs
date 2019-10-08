using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class AlertRecipientMap : IEntityTypeConfiguration<AlertRecipient>
    {
        public void Configure(EntityTypeBuilder<AlertRecipient> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EmailAddress).HasMaxLength(100);
            builder.HasOne(x => x.AlertRule).WithMany().IsRequired();
        }
    }
}
