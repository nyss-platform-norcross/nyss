using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class HeadManagerConsentMap : IEntityTypeConfiguration<HeadManagerConsent>
    {
        public void Configure(EntityTypeBuilder<HeadManagerConsent> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.NationalSocietyId).IsRequired();
            builder.Property(u => u.UserEmailAddress).HasMaxLength(100).IsRequired();
            builder.Property(u => u.UserPhoneNumber).HasMaxLength(20).IsRequired();
            builder.Property(x => x.ConsentedFrom).IsRequired();
            builder.Property(x => x.ConsentedUntil);
            builder.Property(x => x.ConsentDocument).HasMaxLength(50);
        }
    }
}
