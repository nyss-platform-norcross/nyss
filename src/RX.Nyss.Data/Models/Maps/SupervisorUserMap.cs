using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class SupervisorUserMap : IEntityTypeConfiguration<SupervisorUser>
    {
        public void Configure(EntityTypeBuilder<SupervisorUser> builder)
        {
            builder.HasBaseType<User>();
            builder.Property(u => u.Sex).HasMaxLength(20).IsRequired();
            builder.HasOne(dmu => dmu.NationalSociety).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(su => su.Village).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(su => su.Zone).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(su => su.DataManagerUser).WithMany().OnDelete(DeleteBehavior.Restrict);
        }
    }
}
