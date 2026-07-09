using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class TechnicalAdvisorUserGatewayModemMap : IEntityTypeConfiguration<TechnicalAdvisorUserGatewayModem>
    {
        public void Configure(EntityTypeBuilder<TechnicalAdvisorUserGatewayModem> builder)
        {
            builder.HasKey(x => new
            {
                x.TechnicalAdvisorUserId,
                x.GatewayModemId
            });

            builder.HasOne(x => x.TechnicalAdvisorUser).WithMany(ta => ta.TechnicalAdvisorUserGatewayModems).IsRequired();
        }
    }
}
