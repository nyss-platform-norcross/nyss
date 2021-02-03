using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class GatewayModemMap : IEntityTypeConfiguration<GatewayModem>
    {
        public void Configure(EntityTypeBuilder<GatewayModem> builder)
        {
            builder.HasKey(gm => gm.Id);
            builder.Property(gm => gm.ModemId).IsRequired();
            builder.Property(gm => gm.Name).HasMaxLength(100).IsRequired();
        }
    }
}
