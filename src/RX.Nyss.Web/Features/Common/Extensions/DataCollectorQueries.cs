using System;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class DataCollectorQueries
    {
        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByNationalSociety(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, int nationalSocietyId) =>
            dataCollectors.Where(dc => dc.Project.NationalSocietyId == nationalSocietyId);

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByDataCollectorType(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, DataCollectorType? dataCollectorType) =>
            dataCollectorType switch
            {
                DataCollectorType.Human =>
                dataCollectors.Where(dc => dc.DataCollectorType == DataCollectorType.Human),

                DataCollectorType.CollectionPoint =>
                dataCollectors.Where(dc => dc.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                dataCollectors
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByType(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, DataCollectorType? dataCollectorType) =>
            dataCollectorType switch
            {
                DataCollectorType.Human =>
                dataCollectors.Where(dc => dc.DataCollectorType == DataCollectorType.Human),

                DataCollectorType.CollectionPoint =>
                dataCollectors.Where(dc => dc.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                dataCollectors
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByArea(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, AreaDto area) =>
            area?.Type switch
            {
                AreaType.Region =>
                dataCollectors.Where(dc => dc.Village.District.Region.Id == area.Id),

                AreaType.District =>
                dataCollectors.Where(dc => dc.Village.District.Id == area.Id),

                AreaType.Village =>
                dataCollectors.Where(dc => dc.Village.Id == area.Id),

                AreaType.Zone =>
                dataCollectors.Where(dc => dc.Zone.Id == area.Id),

                _ =>
                dataCollectors
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByArea(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, Area area) =>
            area?.AreaType switch
            {
                AreaType.Region =>
                dataCollectors.Where(dc => dc.Village.District.Region.Id == area.AreaId),

                AreaType.District =>
                dataCollectors.Where(dc => dc.Village.District.Id == area.AreaId),

                AreaType.Village =>
                dataCollectors.Where(dc => dc.Village.Id == area.AreaId),

                AreaType.Zone =>
                dataCollectors.Where(dc => dc.Zone.Id == area.AreaId),

                _ =>
                dataCollectors
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByTrainingMode(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, bool isInTraining) =>
            dataCollectors.Where(dc => (isInTraining ? dc.IsInTrainingMode : !dc.IsInTrainingMode));

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByProject(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, int projectId) =>
            dataCollectors.Where(dc => dc.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterOnlyNotDeletedBefore(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, DateTime startDate) =>
            dataCollectors.Where(dc => dc.DeletedAt == null || dc.DeletedAt > startDate);
    }
}
