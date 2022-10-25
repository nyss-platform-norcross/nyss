using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class SuspectedDiseaseMap : IEntityTypeConfiguration<SuspectedDisease>
    {
        public void Configure(EntityTypeBuilder<SuspectedDisease> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SuspectedDiseaseCode)
                .IsRequired();

            builder.HasMany(x => x.LanguageContents)
                .WithOne(x => x.SuspectedDisease)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
