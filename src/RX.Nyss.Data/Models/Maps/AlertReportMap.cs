using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class AlertReportMap : IEntityTypeConfiguration<AlertReport>
    {
        public void Configure(EntityTypeBuilder<AlertReport> builder)
        {
            builder.HasKey(ar => new {ar.AlertId, ar.ReportId});
            builder.HasOne(ar => ar.Alert).WithMany(a => a.AlertReports).HasForeignKey(ar => ar.AlertId).IsRequired();
            builder.HasOne(ar => ar.Report).WithMany(r => r.ReportAlerts).HasForeignKey(ar => ar.ReportId).IsRequired();
        }
    }
}
