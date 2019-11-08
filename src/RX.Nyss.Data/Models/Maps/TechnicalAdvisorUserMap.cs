using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class TechnicalAdvisorUserMap : IEntityTypeConfiguration<TechnicalAdvisorUser>
    {
        public void Configure(EntityTypeBuilder<TechnicalAdvisorUser> builder) => builder.HasBaseType<User>();
    }
}
