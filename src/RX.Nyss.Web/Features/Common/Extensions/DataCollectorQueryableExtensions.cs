using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class DataCollectorQueryableExtensions
    {
        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByType(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, FiltersRequestDto.ReportsTypeDto reportsType) =>
            reportsType switch
            {
                FiltersRequestDto.ReportsTypeDto.DataCollector =>
                dataCollectors.Where(dc => dc.DataCollectorType == DataCollectorType.Human),

                FiltersRequestDto.ReportsTypeDto.DataCollectionPoint =>
                dataCollectors.Where(dc => dc.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                dataCollectors
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByArea(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, FiltersRequestDto.AreaDto area) =>
            area?.Type switch
            {
                FiltersRequestDto.AreaTypeDto.Region =>
                dataCollectors.Where(dc => dc.Village.District.Region.Id == area.Id),

                FiltersRequestDto.AreaTypeDto.District =>
                dataCollectors.Where(dc => dc.Village.District.Id == area.Id),

                FiltersRequestDto.AreaTypeDto.Village =>
                dataCollectors.Where(dc => dc.Village.Id == area.Id),

                FiltersRequestDto.AreaTypeDto.Zone =>
                dataCollectors.Where(dc => dc.Zone.Id == area.Id),

                _ =>
                dataCollectors
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByProject(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, int projectId) =>
            dataCollectors.Where(dc => dc.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByTrainingMode(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, bool isInTraining) =>
            dataCollectors.Where(dc => (isInTraining ? dc.IsInTrainingMode : !dc.IsInTrainingMode));

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterOnlyNotDeleted(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors) =>
            dataCollectors.Where(dc => !dc.DeletedAt.HasValue);
    }
}
