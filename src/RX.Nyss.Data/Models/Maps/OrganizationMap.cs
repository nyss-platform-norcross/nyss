using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class OrganizationMap : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.HasKey(o => o.Id);
            builder.HasIndex(o => new
            {
                o.Name,
                o.NationalSocietyId
            }).IsUnique();
            builder.Property(o => o.Name).HasMaxLength(100).IsRequired();
            builder.HasOne(o => o.NationalSociety).WithMany(ns => ns.Organizations).HasForeignKey(o => o.NationalSocietyId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(uns => uns.HeadManager).WithMany().HasForeignKey(x => x.HeadManagerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(uns => uns.PendingHeadManager).WithMany().OnDelete(DeleteBehavior.Restrict);
        }
    }
}
