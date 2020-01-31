using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class GlobalCoordinatorUserMap : IEntityTypeConfiguration<GlobalCoordinatorUser>
    {
        public void Configure(EntityTypeBuilder<GlobalCoordinatorUser> builder)
        {
            builder.HasBaseType<User>();
        }
    }
}
