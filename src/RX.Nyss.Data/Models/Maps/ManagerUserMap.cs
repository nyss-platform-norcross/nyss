using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class ManagerUserMap : IEntityTypeConfiguration<ManagerUser>
    {
        public void Configure(EntityTypeBuilder<ManagerUser> builder)
        {
            builder.HasBaseType<User>();
            builder.Property(dmu => dmu.IsDataOwner);
            builder.Property(dmu => dmu.HasConsented);
            builder.Property(dmu => dmu.ConsentedAt);
        }
    }
}
