using System;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class DataCollectorQueries
    {
        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByNationalSociety(this IQueryable<Nyss.Data.Models.DataCollector> reports, int nationalSocietyId) =>
            reports.Where(dc => dc.Project.NationalSocietyId == nationalSocietyId);

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByDataCollectorType(this IQueryable<Nyss.Data.Models.DataCollector> reports, DataCollectorType? dataCollectorType) =>
            dataCollectorType switch
            {
                DataCollectorType.Human =>
                reports.Where(dc => dc.DataCollectorType == DataCollectorType.Human),

                DataCollectorType.CollectionPoint =>
                reports.Where(dc => dc.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                reports
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByType(this IQueryable<Nyss.Data.Models.DataCollector> reports, DataCollectorType? dataCollectorType) =>
            dataCollectorType switch
            {
                DataCollectorType.Human =>
                reports.Where(dc => dc.DataCollectorType == DataCollectorType.Human),

                DataCollectorType.CollectionPoint =>
                reports.Where(dc => dc.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                reports
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByArea(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, AreaDto area) =>
            area?.Type switch
            {
                AreaDto.AreaType.Region =>
                dataCollectors.Where(dc => dc.Village.District.Region.Id == area.Id),

                AreaDto.AreaType.District =>
                dataCollectors.Where(dc => dc.Village.District.Id == area.Id),

                AreaDto.AreaType.Village =>
                dataCollectors.Where(dc => dc.Village.Id == area.Id),

                AreaDto.AreaType.Zone =>
                dataCollectors.Where(dc => dc.Zone.Id == area.Id),

                _ =>
                dataCollectors
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByTrainingMode(this IQueryable<Nyss.Data.Models.DataCollector> reports, bool isInTraining) =>
            reports.Where(dc => (isInTraining ? dc.IsInTrainingMode : !dc.IsInTrainingMode));

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByProject(this IQueryable<Nyss.Data.Models.DataCollector> reports, int projectId) =>
            reports.Where(dc => dc.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterOnlyNotDeletedBefore(this IQueryable<Nyss.Data.Models.DataCollector> reports, DateTime startDate) =>
            reports.Where(dc => dc.DeletedAt == null || dc.DeletedAt > startDate);
    }
}
