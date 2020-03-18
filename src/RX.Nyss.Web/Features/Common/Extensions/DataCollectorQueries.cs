using System;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class DataCollectorQueries
    {
        public static IQueryable<DataCollector> FilterByNationalSociety(this IQueryable<DataCollector> dataCollectors, int nationalSocietyId) =>
            dataCollectors.Where(dc => dc.Project.NationalSocietyId == nationalSocietyId);

        public static IQueryable<DataCollector> FilterByType(this IQueryable<DataCollector> dataCollectors, DataCollectorType? dataCollectorType) =>
            dataCollectorType switch
            {
                DataCollectorType.Human =>
                dataCollectors.Where(dc => dc.DataCollectorType == DataCollectorType.Human),

                DataCollectorType.CollectionPoint =>
                dataCollectors.Where(dc => dc.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                dataCollectors
            };

        public static IQueryable<DataCollector> FilterByArea(this IQueryable<DataCollector> dataCollectors, AreaDto area) =>
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

        public static IQueryable<DataCollector> FilterByArea(this IQueryable<DataCollector> dataCollectors, Area area) =>
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

        public static IQueryable<DataCollector> FilterByTrainingMode(this IQueryable<DataCollector> dataCollectors, bool isInTraining) =>
            dataCollectors.Where(dc => isInTraining
                ? dc.IsInTrainingMode
                : !dc.IsInTrainingMode);

        public static IQueryable<DataCollector> FilterByTrainingMode(this IQueryable<DataCollector> dataCollectors, TrainingStatusDto? trainingStatus) =>
            trainingStatus switch
            {
                TrainingStatusDto.InTraining => dataCollectors.Where(dc => dc.IsInTrainingMode),
                TrainingStatusDto.Trained => dataCollectors.Where(dc => !dc.IsInTrainingMode),
                _ => dataCollectors
            };

        public static IQueryable<DataCollector> FilterByProject(this IQueryable<DataCollector> dataCollectors, int projectId) =>
            dataCollectors.Where(dc => dc.Project.Id == projectId);

        public static IQueryable<DataCollector> FilterOnlyNotDeletedBefore(this IQueryable<DataCollector> dataCollectors, DateTime startDate) =>
            dataCollectors.Where(dc => dc.DeletedAt == null || dc.DeletedAt > startDate);

        public static IQueryable<DataCollector> FilterBySupervisor(this IQueryable<DataCollector> dataCollectors, int? supervisorId) =>
            dataCollectors.Where(dc => !supervisorId.HasValue || dc.Supervisor.Id == supervisorId);

        public static IQueryable<DataCollector> FilterBySex(this IQueryable<DataCollector> dataCollectors, SexDto? sexDto) =>
            sexDto switch
            {
                SexDto.Male => dataCollectors.Where(dc => dc.Sex == Sex.Male),
                SexDto.Female => dataCollectors.Where(dc => dc.Sex == Sex.Female),
                SexDto.Other => dataCollectors.Where(dc => dc.Sex == Sex.Other),
                _ => dataCollectors
            };
    }
}
