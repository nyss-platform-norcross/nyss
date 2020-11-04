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
            builder.Property(x => x.EscalatedAt);
            builder.Property(x => x.DismissedAt);
            builder.Property(x => x.ClosedAt);
            builder.Property(x => x.Comments).HasMaxLength(500);
            builder.Property(x => x.EscalatedOutcome).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.HasOne(x => x.ProjectHealthRisk).WithMany(x => x.Alerts).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EscalatedBy).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DismissedBy).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ClosedBy).WithMany().OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.Status);
        }
    }
}
