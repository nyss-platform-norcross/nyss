using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class AlertMap : IEntityTypeConfiguration<Alert>
    {
        public void Configure(EntityTypeBuilder<Alert> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.Comments).HasMaxLength(500);
            builder.Property(x => x.Status).HasConversion<string>().IsRequired();
            builder.HasOne(x => x.ProjectHealthRisk).WithMany().IsRequired();
        }
    }
}
