﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class ReportMap : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RawContent).HasMaxLength(160).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.ReceivedAt).IsRequired();
            builder.Property(x => x.ModifiedAt);
            builder.Property(x => x.ModifiedBy).HasMaxLength(100);
            builder.Property(x => x.IsValid).IsRequired();
            builder.Property(x => x.IsTraining).IsRequired();
            builder.Property(x => x.Location).IsRequired();
            builder.Property(x => x.ReportType).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
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
            builder.HasOne(x => x.DataCollector).WithMany().IsRequired();
            builder.HasOne(x => x.ProjectHealthRisk).WithMany().IsRequired();
        }
    }
}