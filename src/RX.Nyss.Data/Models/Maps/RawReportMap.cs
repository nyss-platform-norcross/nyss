using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class RawReportMap : IEntityTypeConfiguration<RawReport>
    {
        public void Configure(EntityTypeBuilder<RawReport> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Sender).HasMaxLength(20);
            builder.Property(x => x.Timestamp).HasMaxLength(14);
            builder.Property(x => x.ReceivedAt).IsRequired();
            builder.Property(x => x.Text).HasMaxLength(160);
            builder.Property(x => x.IncomingMessageId);
            builder.Property(x => x.OutgoingMessageId);
            builder.Property(x => x.ModemNumber);
            builder.Property(x => x.IsTraining);
            builder.Property(x => x.ApiKey).HasMaxLength(100).IsRequired();
            builder.HasOne(x => x.Report).WithOne(x => x.RawReport).HasForeignKey<RawReport>(x => x.ReportId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.NationalSociety).WithMany(x => x.RawReports).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DataCollector).WithMany(x => x.RawReports).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
