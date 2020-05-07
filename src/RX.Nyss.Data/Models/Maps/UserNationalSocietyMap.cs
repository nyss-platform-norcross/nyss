using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class UserNationalSocietyMap : IEntityTypeConfiguration<UserNationalSociety>
    {
        public void Configure(EntityTypeBuilder<UserNationalSociety> builder)
        {
            builder.HasQueryFilter(uns => !uns.User.DeletedAt.HasValue);

            builder.HasKey(uns => new
            {
                uns.UserId,
                uns.NationalSocietyId
            });
            builder.HasOne(uns => uns.User).WithMany(u => u.UserNationalSocieties).HasForeignKey(uns => uns.UserId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(uns => uns.NationalSociety).WithMany(ns => ns.NationalSocietyUsers).HasForeignKey(uns => uns.NationalSocietyId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(uns => uns.Organization).WithMany(o => o.NationalSocietyUsers).HasForeignKey(uns => uns.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
