using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class AdministratorUserMap : IEntityTypeConfiguration<AdministratorUser>
    {
        public void Configure(EntityTypeBuilder<AdministratorUser> builder)
        {
            builder.HasBaseType<User>();
        }
    }
}
