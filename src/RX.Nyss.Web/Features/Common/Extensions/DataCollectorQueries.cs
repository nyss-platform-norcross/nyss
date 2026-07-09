using System;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;

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
            area != null && area.RegionIds.Any()
                ? dataCollectors.Where(dc => dc.DataCollectorLocations.Any(dcl =>
                        area.VillageIds.Contains(dcl.Village.Id)
                        && (dcl.Zone == null || area.ZoneIds.Contains(dcl.Zone.Id))
                        && area.DistrictIds.Contains(dcl.Village.District.Id)
                        && area.RegionIds.Contains(dcl.Village.District.Region.Id))
                    || (area.IncludeUnknownLocation && !dc.DataCollectorLocations.Any()))
                : dataCollectors;

        public static IQueryable<DataCollector> FilterOnlyDeployed(this IQueryable<DataCollector> dataCollectors) =>
            dataCollectors.Where(dc => dc.Deployed);

        public static IQueryable<DataCollector> FilterByDeployedMode(this IQueryable<DataCollector> dataCollectors, DeployedModeDto? deployedMode) =>
            deployedMode switch
            {
                DeployedModeDto.Deployed => dataCollectors.Where(dc => dc.Deployed),
                DeployedModeDto.NotDeployed => dataCollectors.Where(dc => !dc.Deployed),
                _ => dataCollectors
            };

        public static IQueryable<DataCollector> FilterByTrainingMode(this IQueryable<DataCollector> dataCollectors, TrainingStatusDto? trainingStatus) =>
            trainingStatus switch
            {
                TrainingStatusDto.InTraining => dataCollectors.Where(dc => dc.IsInTrainingMode),
                TrainingStatusDto.Trained => dataCollectors.Where(dc => !dc.IsInTrainingMode),
                _ => dataCollectors
            };

        public static IQueryable<DataCollector> FilterByProject(this IQueryable<DataCollector> dataCollectors, int projectId) =>
            dataCollectors.Where(dc => dc.Project.Id == projectId);

        public static IQueryable<DataCollector> FilterOnlyNotDeletedBefore(this IQueryable<DataCollector> dataCollectors, DateTimeOffset startDate) =>
            dataCollectors.Where(dc => dc.DeletedAt == null || dc.DeletedAt > startDate);

        public static IQueryable<DataCollector> FilterOnlyNotDeleted(this IQueryable<DataCollector> dataCollectors) =>
            dataCollectors.Where(dc => !dc.DeletedAt.HasValue);

        public static IQueryable<DataCollector> FilterBySupervisor(this IQueryable<DataCollector> dataCollectors, int? supervisorId) =>
            dataCollectors.Where(dc => !supervisorId.HasValue || dc.HeadSupervisor.Id == supervisorId || dc.Supervisor.Id == supervisorId);

        public static IQueryable<DataCollector> FilterBySex(this IQueryable<DataCollector> dataCollectors, SexDto? sexDto) =>
            sexDto switch
            {
                SexDto.Male => dataCollectors.Where(dc => dc.Sex == Sex.Male),
                SexDto.Female => dataCollectors.Where(dc => dc.Sex == Sex.Female),
                SexDto.Other => dataCollectors.Where(dc => dc.Sex == Sex.Other),
                _ => dataCollectors
            };

        public static IQueryable<DataCollector> FilterByOrganization(this IQueryable<DataCollector> dataCollectors, Organization organization) =>
            organization == null
                ? dataCollectors
                : dataCollectors.Where(dc =>
                    dc.Supervisor.UserNationalSocieties.Any(uns => uns.Organization == organization)
                    || dc.HeadSupervisor.UserNationalSocieties.Any(uns => uns.Organization == organization));

        public static IQueryable<DataCollector> FilterByName(this IQueryable<DataCollector> dataCollectors, string name) =>
            string.IsNullOrEmpty(name)
                ? dataCollectors
                : dataCollectors.Where(dc => dc.Name.ToLower().Contains(name.ToLower()) || dc.DisplayName.ToLower().Contains(name.ToLower()));
    }
}
