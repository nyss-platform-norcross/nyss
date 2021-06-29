using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class AlertEventTypeMap : IEntityTypeConfiguration<AlertEventType>
    {
        public void Configure(EntityTypeBuilder<AlertEventType> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasMany(x => x.AlertEventSubtypes)
                .WithOne(x => x.AlertEventType);
        }
    }
}
