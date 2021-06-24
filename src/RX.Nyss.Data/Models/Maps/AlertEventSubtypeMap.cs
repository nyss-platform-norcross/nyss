using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class AlertEventSubtypeMap : IEntityTypeConfiguration<AlertEventSubtype>
    {
        public void Configure(EntityTypeBuilder<AlertEventSubtype> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.AlertEventType)
                .WithMany(x => x.AlertEventSubtypes)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}

