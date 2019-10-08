using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class AlertRuleMap : IEntityTypeConfiguration<AlertRule>
    {
        public void Configure(EntityTypeBuilder<AlertRule> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CountThreshold).IsRequired();
            builder.Property(x => x.HoursThreshold);
            builder.Property(x => x.MetersThreshold);
        }
    }
}
