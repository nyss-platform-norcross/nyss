using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Project.Dto;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.ProjectDashboard
{
    public interface IProjectDashboardDataService
    {
        Task<ProjectSummaryResponseDto> GetSummaryData(int projectId, FiltersRequestDto filtersDto);
        Task<IList<ReportByDateResponseDto>> GetReportsGroupedByDate(int projectId, FiltersRequestDto filtersDto);
        Task<IList<ReportByFeaturesAndDateResponseDto>> GetReportsGroupedByFeaturesAndDate(int projectId, FiltersRequestDto filtersDto);
        Task<IEnumerable<ProjectSummaryMapResponseDto>> GetProjectSummaryMap(int projectId, FiltersRequestDto filtersDto);
        Task<IEnumerable<ProjectSummaryReportHealthRiskResponseDto>> GetProjectReportHealthRisks(int projectId, double latitude, double longitude, FiltersRequestDto filtersDto);
        Task<IList<DataCollectionPointsReportsByDateDto>> GetDataCollectionPointReports(int projectId, FiltersRequestDto filtersDto);
    }

    public class ProjectDashboardDataService : IProjectDashboardDataService
    {
        private readonly INyssContext _nyssContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ProjectDashboardDataService(
            INyssContext nyssContext,
            IDateTimeProvider dateTimeProvider)
        {
            _nyssContext = nyssContext;
            _dateTimeProvider = dateTimeProvider;
        }

        public Task<ProjectSummaryResponseDto> GetSummaryData(int projectId, FiltersRequestDto filtersDto)
        {
            var allReports = GetFilteredReportsForAllHealthRisks(projectId, filtersDto);
            var healthRiskReports = allReports
                .FilterReportsByHealthRisk(filtersDto.HealthRiskId)
                .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity);
            var dataCollectionPointReports = healthRiskReports.Where(r => r.DataCollectionPointCase != null);

            return _nyssContext.Projects
                .Where(ph => ph.Id == projectId)
                .Select(p => new ProjectSummaryResponseDto
                {
                    ReportCount = (int)healthRiskReports.Sum(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType == HealthRiskType.Human ? r.ReportedCase.CountFemalesAtLeastFive + r.ReportedCase.CountFemalesBelowFive + r.ReportedCase.CountMalesAtLeastFive + r.ReportedCase.CountMalesBelowFive : 1),
                    ActiveDataCollectorCount = allReports
                        .Where(r => r.DataCollector.Name != Anonymization.Text && r.DataCollector.DeletedAt == null)
                        .Select(r => r.DataCollector.Id)
                        .Distinct().Count(),
                    InactiveDataCollectorCount = InactiveDataCollectorCount(projectId, allReports, filtersDto),
                    DataCollectionPointSummary = new DataCollectionPointsSummaryResponse
                        {
                            FromOtherVillagesCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.FromOtherVillagesCount ?? 0),
                            ReferredToHospitalCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.ReferredCount ?? 0),
                            DeathCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.DeathCount ?? 0),
                        }
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IList<ReportByDateResponseDto>> GetReportsGroupedByDate(int projectId, FiltersRequestDto filtersDto)
        {
            var reports = GetFilteredReports(projectId, filtersDto)
                .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity);

            return filtersDto.GroupingType switch
            {
                FiltersRequestDto.GroupingTypeDto.Day =>
                    await GroupReportsByDay(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date),

                FiltersRequestDto.GroupingTypeDto.Week =>
                    await GroupReportsByWeek(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date),

                _ =>
                    throw new InvalidOperationException()
            };
        }

        public async Task<IList<ReportByFeaturesAndDateResponseDto>> GetReportsGroupedByFeaturesAndDate(int projectId, FiltersRequestDto filtersDto)
        {
            var reports = GetFilteredReports(projectId, filtersDto)
                .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType == HealthRiskType.Human);

            return filtersDto.GroupingType switch
            {
                FiltersRequestDto.GroupingTypeDto.Day =>
                    await GroupReportsByFeaturesAndDay(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date),

                FiltersRequestDto.GroupingTypeDto.Week =>
                    await GroupReportsByFeaturesAndWeek(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date),

                _ =>
                    throw new InvalidOperationException()
            };
        }

        public async Task<IList<DataCollectionPointsReportsByDateDto>> GetDataCollectionPointReports(int projectId, FiltersRequestDto filtersDto)
        {
            if (filtersDto.ReportsType != FiltersRequestDto.ReportsTypeDto.DataCollectionPoint)
            {
                return new List<DataCollectionPointsReportsByDateDto>();
            }

            var reports = GetFilteredReports(projectId, filtersDto)
                .Where(r => r.ReportType == ReportType.DataCollectionPoint);

            return filtersDto.GroupingType switch
            {
                FiltersRequestDto.GroupingTypeDto.Day =>
                await GroupReportsByDataCollectionPointFeaturesAndDay(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date),

                FiltersRequestDto.GroupingTypeDto.Week =>
                await GroupReportsByDataCollectionPointFeaturesAndWeek(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date),

                _ =>
                throw new InvalidOperationException()
            };
        }

        private static async Task<IList<DataCollectionPointsReportsByDateDto>> GroupReportsByDataCollectionPointFeaturesAndDay(IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate,
            DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new { r.ReceivedAt, r.DataCollectionPointCase.ReferredCount, r.DataCollectionPointCase.DeathCount, r.DataCollectionPointCase.FromOtherVillagesCount })
                .GroupBy(r => r.ReceivedAt.Date)
                .Select(grouping => new
                {
                    Period = grouping.Key,
                    ReferredCount = (int)grouping.Sum(g => g.ReferredCount),
                    DeathCount = (int)grouping.Sum(g => g.DeathCount),
                    FromOtherVillagesCount = (int)grouping.Sum(g => g.FromOtherVillagesCount),
                })
                .ToListAsync();

            var missingDays = Enumerable
                .Range(0, endDate.Subtract(startDate).Days + 1)
                .Select(i => startDate.AddDays(i))
                .Where(day => !groupedReports.Any(r => r.Period == day))
                .Select(day => new { Period = day, ReferredCount = 0, DeathCount = 0, FromOtherVillagesCount = 0 });

            return groupedReports
                .Union(missingDays)
                .OrderBy(r => r.Period)
                .Select(x => new DataCollectionPointsReportsByDateDto()
                {
                    Period = x.Period.ToString("dd/MM"),
                    ReferredCount = x.ReferredCount,
                    DeathCount = x.DeathCount,
                    FromOtherVillagesCount = x.FromOtherVillagesCount
                })
                .ToList();
        }

        private async Task<IList<DataCollectionPointsReportsByDateDto>> GroupReportsByDataCollectionPointFeaturesAndWeek(IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate,
            DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new { r.ReceivedAt.Year, r.EpiWeek, r.DataCollectionPointCase.ReferredCount, r.DataCollectionPointCase.DeathCount, r.DataCollectionPointCase.FromOtherVillagesCount })
                .GroupBy(r => new { r.Year, r.EpiWeek })
                .Select(grouping => new
                {
                    EpiPeriod = grouping.Key,
                    ReferredCount = (int)grouping.Sum(g => g.ReferredCount),
                    DeathCount = (int)grouping.Sum(g => g.DeathCount),
                    FromOtherVillagesCount = (int)grouping.Sum(g => g.FromOtherVillagesCount),
                })
                .ToListAsync();

            var missingWeeks = Enumerable
                .Range(0, (endDate.Subtract(startDate).Days / 7) + 1)
                .Select(w => startDate.AddDays(w * 7))
                .Where(day => !groupedReports.Any(r => r.EpiPeriod.Year == day.Year && r.EpiPeriod.EpiWeek == _dateTimeProvider.GetEpiWeek(day)))
                .Select(day => new { EpiPeriod = new { day.Year, EpiWeek = _dateTimeProvider.GetEpiWeek(day) }, ReferredCount = 0, DeathCount = 0, FromOtherVillagesCount = 0 });

            return groupedReports
                .Union(missingWeeks)
                .OrderBy(r => r.EpiPeriod.Year)
                .ThenBy(r => r.EpiPeriod.EpiWeek)
                .Select(x => new DataCollectionPointsReportsByDateDto()
                {
                    Period = x.EpiPeriod.EpiWeek.ToString(),
                    ReferredCount = x.ReferredCount,
                    DeathCount = x.DeathCount,
                    FromOtherVillagesCount = x.FromOtherVillagesCount
                })
                .ToList();
        }


        public async Task<IEnumerable<ProjectSummaryMapResponseDto>> GetProjectSummaryMap(int projectId, FiltersRequestDto filtersDto)
        {
            var reports = GetFilteredReports(projectId, filtersDto)
                .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity);

            var groupedByLocation = await reports
                .GroupBy(report => new { report.Location.X, report.Location.Y })
                .Select(grouping => new ProjectSummaryMapResponseDto
                {
                    ReportsCount = grouping.Count(),
                    Location = new ProjectSummaryMapResponseDto.MapReportLocation
                    {
                        Latitude = grouping.Key.Y,
                        Longitude = grouping.Key.X
                    }
                })
                .ToListAsync();

            return groupedByLocation;
        }

        public async Task<IEnumerable<ProjectSummaryReportHealthRiskResponseDto>> GetProjectReportHealthRisks(int projectId, double latitude, double longitude, FiltersRequestDto filtersDto)
        {
            var reports = GetFilteredReports(projectId, filtersDto)
                .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity);

            return await reports
                .Where(r => r.Location.X == longitude && r.Location.Y == latitude)
                .Select(r => new
                {
                    r.ProjectHealthRisk.HealthRiskId,
                    HealthRiskName = r.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == r.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name).FirstOrDefault(),
                    Total = (int)(r.ProjectHealthRisk.HealthRisk.HealthRiskType == HealthRiskType.Human ? r.ReportedCase.CountFemalesAtLeastFive + r.ReportedCase.CountFemalesBelowFive + r.ReportedCase.CountMalesAtLeastFive + r.ReportedCase.CountMalesBelowFive : 1),
                })
                .Where(r => r.Total > 0)
                .GroupBy(r => r.HealthRiskId)
                .Select(grouping => new ProjectSummaryReportHealthRiskResponseDto
                {
                    Name = _nyssContext.HealthRiskLanguageContents.Where(f => f.HealthRisk.Id == grouping.Key).Select(s => s.Name).FirstOrDefault(),
                    Value = grouping.Sum(r => r.Total),
                })
                .ToListAsync();
        }

        private static async Task<IList<ReportByFeaturesAndDateResponseDto>> GroupReportsByFeaturesAndDay(IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new { r.ReceivedAt, r.ReportedCase.CountFemalesAtLeastFive, r.ReportedCase.CountFemalesBelowFive, r.ReportedCase.CountMalesAtLeastFive, r.ReportedCase.CountMalesBelowFive })
                .GroupBy(r => r.ReceivedAt.Date)
                .Select(grouping => new
                {
                    Period = grouping.Key,
                    CountFemalesAtLeastFive = (int)grouping.Sum(g => g.CountFemalesAtLeastFive),
                    CountFemalesBelowFive = (int)grouping.Sum(g => g.CountFemalesBelowFive),
                    CountMalesAtLeastFive = (int)grouping.Sum(g => g.CountMalesAtLeastFive),
                    CountMalesBelowFive = (int)grouping.Sum(g => g.CountMalesBelowFive)
                })
                .ToListAsync();

            var missingDays = Enumerable
                .Range(0, endDate.Subtract(startDate).Days + 1)
                .Select(i => startDate.AddDays(i))
                .Where(day => !groupedReports.Any(r => r.Period == day))
                .Select(day => new { Period = day, CountFemalesAtLeastFive = 0, CountFemalesBelowFive = 0, CountMalesAtLeastFive = 0, CountMalesBelowFive = 0 });

            return groupedReports
                .Union(missingDays)
                .OrderBy(r => r.Period)
                .Select(x => new ReportByFeaturesAndDateResponseDto
                {
                    Period = x.Period.ToString("dd/MM"),
                    CountFemalesAtLeastFive = x.CountFemalesAtLeastFive,
                    CountFemalesBelowFive = x.CountFemalesBelowFive,
                    CountMalesAtLeastFive = x.CountMalesAtLeastFive,
                    CountMalesBelowFive = x.CountMalesBelowFive
                })
                .ToList();
        }

        private async Task<IList<ReportByFeaturesAndDateResponseDto>> GroupReportsByFeaturesAndWeek(IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new { r.ReceivedAt.Year, r.EpiWeek, r.ReportedCase.CountFemalesAtLeastFive, r.ReportedCase.CountFemalesBelowFive, r.ReportedCase.CountMalesAtLeastFive, r.ReportedCase.CountMalesBelowFive })
                .GroupBy(r => new { r.Year, r.EpiWeek })
                .Select(grouping => new
                {
                    EpiPeriod = grouping.Key,
                    CountFemalesAtLeastFive = (int)grouping.Sum(g => g.CountFemalesAtLeastFive),
                    CountFemalesBelowFive = (int)grouping.Sum(g => g.CountFemalesBelowFive),
                    CountMalesAtLeastFive = (int)grouping.Sum(g => g.CountMalesAtLeastFive),
                    CountMalesBelowFive = (int)grouping.Sum(g => g.CountMalesBelowFive)
                })
                .ToListAsync();

            var missingWeeks = Enumerable
                .Range(0, (endDate.Subtract(startDate).Days / 7) + 1)
                .Select(w => startDate.AddDays(w * 7))
                .Where(day => !groupedReports.Any(r => r.EpiPeriod.Year == day.Year && r.EpiPeriod.EpiWeek == _dateTimeProvider.GetEpiWeek(day)) && !_dateTimeProvider.IsFirstWeekOfNextYear(day))
                .Select(day => new { EpiPeriod = new { day.Year, EpiWeek = _dateTimeProvider.GetEpiWeek(day) }, CountFemalesAtLeastFive = 0, CountFemalesBelowFive = 0, CountMalesAtLeastFive = 0, CountMalesBelowFive = 0 });

            return groupedReports
                .Union(missingWeeks)
                .OrderBy(r => r.EpiPeriod.Year)
                .ThenBy(r => r.EpiPeriod.EpiWeek)
                .Select(x => new ReportByFeaturesAndDateResponseDto
                {
                    Period = x.EpiPeriod.EpiWeek.ToString(),
                    CountFemalesAtLeastFive = x.CountFemalesAtLeastFive,
                    CountFemalesBelowFive = x.CountFemalesBelowFive,
                    CountMalesAtLeastFive = x.CountMalesAtLeastFive,
                    CountMalesBelowFive = x.CountMalesBelowFive
                })
                .ToList();
        }

        private static async Task<IList<ReportByDateResponseDto>> GroupReportsByDay(IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new
                {
                    r.ReceivedAt,
                    Total = r.ProjectHealthRisk.HealthRisk.HealthRiskType == HealthRiskType.Human ? r.ReportedCase.CountFemalesAtLeastFive + r.ReportedCase.CountFemalesBelowFive + r.ReportedCase.CountMalesAtLeastFive + r.ReportedCase.CountMalesBelowFive : 1,
                })
                .GroupBy(r => r.ReceivedAt.Date)
                .Select(grouping => new
                {
                    Period = grouping.Key,
                    Count = (int)grouping.Sum(r => r.Total)
                })
                .ToListAsync();

            var missingDays = Enumerable
                .Range(0, endDate.Subtract(startDate).Days + 1)
                .Select(i => startDate.AddDays(i))
                .Where(day => !groupedReports.Any(r => r.Period == day))
                .Select(day => new
                {
                    Period = day,
                    Count = 0
                });

            return groupedReports
                .Union(missingDays)
                .OrderBy(r => r.Period)
                .Select(x => new ReportByDateResponseDto
                {
                    Period = x.Period.ToString("dd/MM"),
                    Count = x.Count
                })
                .ToList();
        }

        private async Task<IList<ReportByDateResponseDto>> GroupReportsByWeek(IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new
                {
                    r.ReceivedAt.Year,
                    r.EpiWeek,
                    Total = r.ProjectHealthRisk.HealthRisk.HealthRiskType == HealthRiskType.Human ? r.ReportedCase.CountFemalesAtLeastFive + r.ReportedCase.CountFemalesBelowFive + r.ReportedCase.CountMalesAtLeastFive + r.ReportedCase.CountMalesBelowFive : 1,
                })
                .GroupBy(r => new { r.Year, r.EpiWeek })
                .Select(grouping => new { EpiPeriod = grouping.Key, Count = (int)grouping.Sum(r => r.Total) })
                .ToListAsync();

            var missingWeeks = Enumerable
                .Range(0, (endDate.Subtract(startDate).Days / 7) + 1)
                .Select(w => startDate.AddDays(w * 7))
                .Where(day => !groupedReports.Any(r => r.EpiPeriod.Year == day.Year && r.EpiPeriod.EpiWeek == _dateTimeProvider.GetEpiWeek(day)) && !_dateTimeProvider.IsFirstWeekOfNextYear(day))
                .Select(day => new { EpiPeriod = new { day.Year, EpiWeek = _dateTimeProvider.GetEpiWeek(day) }, Count = 0 });

            return groupedReports
                .Union(missingWeeks)
                .OrderBy(r => r.EpiPeriod.Year)
                .ThenBy(r => r.EpiPeriod.EpiWeek)
                .Select(x => new ReportByDateResponseDto { Period = x.EpiPeriod.EpiWeek.ToString(), Count = x.Count })
                .ToList();
        }

        private IQueryable<Nyss.Data.Models.Report> GetFilteredReports(int projectId, FiltersRequestDto filtersDto) =>
            GetFilteredReportsForAllHealthRisks(projectId, filtersDto)
                .FilterReportsByHealthRisk(filtersDto.HealthRiskId);

        private IQueryable<Nyss.Data.Models.Report> GetFilteredReportsForAllHealthRisks(int projectId, FiltersRequestDto filtersDto)
        {
            var startDate = filtersDto.StartDate.Date;
            var endDate = filtersDto.EndDate.Date.AddDays(1);
            var reports = _nyssContext.Reports
                .Where(r => filtersDto.IsTraining ?
                    r.IsTraining :
                    !r.IsTraining);

            return FilterReportsByRegion(reports, filtersDto.Area)
                .FilterByDataCollectorType(filtersDto.ReportsType)
                .FilterReportsByDate(startDate, endDate)
                .FilterReportsByProject(projectId);
        }

        private static IQueryable<Nyss.Data.Models.Report> FilterReportsByRegion(IQueryable<Nyss.Data.Models.Report> reports, FiltersRequestDto.AreaDto area) =>
            area?.Type switch
            {
                FiltersRequestDto.AreaTypeDto.Region =>
                reports.Where(r => r.DataCollector.Village.District.Region.Id == area.Id),

                FiltersRequestDto.AreaTypeDto.District =>
                reports.Where(r => r.DataCollector.Village.District.Id == area.Id),

                FiltersRequestDto.AreaTypeDto.Village =>
                reports.Where(r => r.DataCollector.Village.Id == area.Id),

                FiltersRequestDto.AreaTypeDto.Zone =>
                reports.Where(r => r.DataCollector.Zone.Id == area.Id),

                _ =>
                reports
            };

        private int InactiveDataCollectorCount(int projectId, IQueryable<Nyss.Data.Models.Report> reports, FiltersRequestDto filtersDto)
        {
            var activeDataCollectors = reports.Select(r => r.DataCollector.Id).Distinct();

            return _nyssContext.DataCollectors
                .FilterDataCollectorsByArea(filtersDto.Area)
                .FilterByDataCollectorType(filtersDto.ReportsType)
                .FilterDataCollectorsByProject(projectId)
                .FilterDataCollectorsByTrainingMode(filtersDto.IsTraining)
                .FilterOnlyNotDeleted()
                .Count(dc => !activeDataCollectors.Contains(dc.Id));
        }
    }

    public static class DataCollectorQueryableExtensions
    {
        public static IQueryable<Nyss.Data.Models.Report> FilterReportsByDate(this IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate) =>
            reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

        public static IQueryable<Nyss.Data.Models.Report> FilterReportsByProject(this IQueryable<Nyss.Data.Models.Report> reports, int projectId) =>
            reports.Where(r => r.DataCollector.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.Report> FilterReportsByHealthRisk(this IQueryable<Nyss.Data.Models.Report> reports, int? healthRiskId) =>
            reports.Where(r => !healthRiskId.HasValue || r.ProjectHealthRisk.HealthRiskId == healthRiskId.Value);

        public static IQueryable<Nyss.Data.Models.Report> FilterByDataCollectorType(this IQueryable<Nyss.Data.Models.Report> reports, FiltersRequestDto.ReportsTypeDto reportsType) =>
            reportsType switch
            {
                FiltersRequestDto.ReportsTypeDto.DataCollector =>
                reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.Human),

                FiltersRequestDto.ReportsTypeDto.DataCollectionPoint =>
                reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                reports
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByDataCollectorType(this IQueryable<Nyss.Data.Models.DataCollector> reports, FiltersRequestDto.ReportsTypeDto reportsType) =>
            reportsType switch
            {
                FiltersRequestDto.ReportsTypeDto.DataCollector =>
                reports.Where(dc => dc.DataCollectorType == DataCollectorType.Human),

                FiltersRequestDto.ReportsTypeDto.DataCollectionPoint =>
                reports.Where(dc => dc.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                reports
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterDataCollectorsByArea(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, FiltersRequestDto.AreaDto area) =>
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

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterDataCollectorsByProject(this IQueryable<Nyss.Data.Models.DataCollector> reports, int projectId) =>
            reports.Where(dc => dc.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterDataCollectorsByTrainingMode(this IQueryable<Nyss.Data.Models.DataCollector> reports, bool isInTraining) =>
            reports.Where(dc => (isInTraining ? dc.IsInTrainingMode : !dc.IsInTrainingMode));

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterOnlyNotDeleted(this IQueryable<Nyss.Data.Models.DataCollector> reports) =>
            reports.Where(dc => !dc.DeletedAt.HasValue);
    }
}
