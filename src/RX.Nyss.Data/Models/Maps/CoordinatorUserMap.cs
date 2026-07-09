using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class CoordinatorUserMap : IEntityTypeConfiguration<CoordinatorUser>
    {
        public void Configure(EntityTypeBuilder<CoordinatorUser> builder) => builder.HasBaseType<User>();
    }
}
