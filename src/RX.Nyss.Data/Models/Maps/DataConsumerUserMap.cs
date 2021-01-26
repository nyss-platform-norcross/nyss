using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class DataConsumerUserMap : IEntityTypeConfiguration<DataConsumerUser>
    {
        public void Configure(EntityTypeBuilder<DataConsumerUser> builder) => builder.HasBaseType<User>();
    }
}
