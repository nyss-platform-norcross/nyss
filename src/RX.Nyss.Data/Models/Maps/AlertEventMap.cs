using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class AlertEventMap : IEntityTypeConfiguration<AlertEvent>
    {
        public void Configure(EntityTypeBuilder<AlertEvent> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Operation).HasMaxLength(100).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.HasOne(x => x.Alert).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.User).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}
