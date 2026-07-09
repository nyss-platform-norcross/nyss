using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class SuspectedDiseaseLanguageContentMap : IEntityTypeConfiguration<SuspectedDiseaseLanguageContent>
    {
        public void Configure(EntityTypeBuilder<SuspectedDiseaseLanguageContent> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne(x => x.ContentLanguage)
                .WithMany()
                .HasForeignKey(x => x.ContentLanguageId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SuspectedDisease)
                .WithMany(x => x.LanguageContents)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
