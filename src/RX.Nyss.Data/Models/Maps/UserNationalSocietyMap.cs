using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class UserNationalSocietyMap : IEntityTypeConfiguration<UserNationalSociety>
    {
        public void Configure(EntityTypeBuilder<UserNationalSociety> builder)
        {
            builder.HasKey(uns => new { uns.UserId, uns.NationalSocietyId });
            builder.HasOne(uns => uns.User).WithMany(u => u.UserNationalSocieties).HasForeignKey(uns => uns.UserId).IsRequired();
            builder.HasOne(uns => uns.NationalSociety).WithMany(ns => ns.NationalSocietyUsers).HasForeignKey(uns => uns.NationalSocietyId).IsRequired();
        }
    }
}
