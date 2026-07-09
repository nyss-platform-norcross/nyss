using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class DataCollectorNotDeployedMap : IEntityTypeConfiguration<DataCollectorNotDeployed>
    {
        public void Configure(EntityTypeBuilder<DataCollectorNotDeployed> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.StartDate)
                .IsRequired();
        }
    }
}
