using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class SupervisorUserMap : IEntityTypeConfiguration<SupervisorUser>
    {
        public void Configure(EntityTypeBuilder<SupervisorUser> builder)
        {
            builder.HasBaseType<User>();
            builder.Property(u => u.Sex).HasConversion<string>().HasMaxLength(10).IsRequired();
            builder.Property(u => u.DecadeOfBirth).IsRequired();
            builder.HasOne(su => su.DataManagerUser).WithMany().OnDelete(DeleteBehavior.Restrict);
        }
    }
}
