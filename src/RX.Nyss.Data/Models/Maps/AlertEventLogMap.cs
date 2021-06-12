using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class AlertEventLogMap : IEntityTypeConfiguration<AlertEventLog>
    {
        public void Configure(EntityTypeBuilder<AlertEventLog> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Alert)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AlertEventType)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AlertEventSubtype)
                .WithMany()
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Textfield);

            builder.Property(x => x.CreatedAt);

            builder.HasOne(x => x.LoggedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
