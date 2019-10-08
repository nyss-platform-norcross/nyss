using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class DataManagerUserMap : IEntityTypeConfiguration<DataManagerUser>
    {
        public void Configure(EntityTypeBuilder<DataManagerUser> builder)
        {
            builder.HasBaseType<User>();
            builder.Property(dmu => dmu.IsDataOwner);
            builder.Property(dmu => dmu.HasConsented);
            builder.Property(dmu => dmu.ConsentedAt);
            builder.HasOne(dmu => dmu.NationalSociety);
        }
    }
}
