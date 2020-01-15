using System;
using System.Linq;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class ReportQueryableExtensions
    {
        public static IQueryable<Nyss.Data.Models.Report> FilterByDate(this IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate) =>
            reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

        public static IQueryable<Nyss.Data.Models.Report> FilterByProject(this IQueryable<Nyss.Data.Models.Report> reports, int projectId) =>
            reports.Where(r => r.DataCollector.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.Report> FilterByHealthRisk(this IQueryable<Nyss.Data.Models.Report> reports, int? healthRiskId) =>
            reports.Where(r => !healthRiskId.HasValue || r.ProjectHealthRisk.HealthRiskId == healthRiskId.Value);

        public static IQueryable<Nyss.Data.Models.Report> FilterByArea(this IQueryable<Nyss.Data.Models.Report> reports, AreaDto area) =>
            area?.Type switch
            {
                AreaDto.AreaType.Region =>
                reports.Where(r => r.Village.District.Region.Id == area.Id),

                AreaDto.AreaType.District =>
                reports.Where(r => r.Village.District.Id == area.Id),

                AreaDto.AreaType.Village =>
                reports.Where(r => r.Village.Id == area.Id),

                AreaDto.AreaType.Zone =>
                reports.Where(r => r.Zone.Id == area.Id),

                _ =>
                reports
            };
    }
}
