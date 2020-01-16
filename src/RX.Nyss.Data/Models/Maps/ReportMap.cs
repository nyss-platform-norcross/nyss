using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class ReportMap : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.ReportGroupLabel);
            builder.HasIndex(x => x.ReceivedAt);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.ReceivedAt).IsRequired();
            builder.Property(x => x.ModifiedAt);
            builder.Property(x => x.AcceptedAt);
            builder.Property(x => x.RejectedAt);
            builder.Property(x => x.ModifiedBy).HasMaxLength(100);
            builder.Property(x => x.IsTraining).IsRequired();
            builder.Property(x => x.EpiWeek).IsRequired();
            builder.Property(x => x.EpiYear).IsRequired().HasDefaultValue(2020);
            builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Location).IsRequired();
            builder.Property(x => x.ReportType).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.ReportGroupLabel).IsRequired();
            builder.Property(x => x.ReportedCaseCount).IsRequired();
            builder.OwnsOne(x => x.ReportedCase).Property(x => x.CountFemalesBelowFive);
            builder.OwnsOne(x => x.ReportedCase).Property(x => x.CountFemalesAtLeastFive);
            builder.OwnsOne(x => x.ReportedCase).Property(x => x.CountMalesBelowFive);
            builder.OwnsOne(x => x.ReportedCase).Property(x => x.CountMalesAtLeastFive);
            builder.OwnsOne(x => x.KeptCase).Property(x => x.CountFemalesBelowFive);
            builder.OwnsOne(x => x.KeptCase).Property(x => x.CountFemalesAtLeastFive);
            builder.OwnsOne(x => x.KeptCase).Property(x => x.CountMalesBelowFive);
            builder.OwnsOne(x => x.KeptCase).Property(x => x.CountMalesAtLeastFive);
            builder.OwnsOne(x => x.DataCollectionPointCase).Property(x => x.ReferredCount);
            builder.OwnsOne(x => x.DataCollectionPointCase).Property(x => x.DeathCount);
            builder.OwnsOne(x => x.DataCollectionPointCase).Property(x => x.FromOtherVillagesCount);
            builder.HasOne(x => x.RawReport).WithOne(x => x.Report).HasForeignKey<RawReport>(x => x.ReportId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DataCollector).WithMany(x => x.Reports).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ProjectHealthRisk).WithMany(x => x.Reports).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Village).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Zone).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AcceptedBy).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RejectedBy).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.ReceivedAt);
            builder.HasIndex(x => x.EpiWeek);
        }
    }
}
