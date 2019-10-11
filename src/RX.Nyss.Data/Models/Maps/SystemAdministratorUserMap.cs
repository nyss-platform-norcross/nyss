using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class SystemAdministratorUserMap : IEntityTypeConfiguration<SystemAdministratorUser>
    {
        public void Configure(EntityTypeBuilder<SystemAdministratorUser> builder)
        {
            builder.HasBaseType<User>();
        }
    }
}