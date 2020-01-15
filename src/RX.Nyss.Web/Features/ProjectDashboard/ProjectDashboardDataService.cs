using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Project.Dto;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;

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
        Task<ReportByVillageAndDateResponseDto> GetReportsGroupedByVillageAndDate(int projectId, FiltersRequestDto filtersDto);
    }

    public class ProjectDashboardDataService : IProjectDashboardDataService
    {
        private readonly INyssContext _nyssContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INyssWebConfig _config;

        public ProjectDashboardDataService(
            INyssContext nyssContext,
            IDateTimeProvider dateTimeProvider,
            INyssWebConfig config)
        {
            _nyssContext = nyssContext;
            _dateTimeProvider = dateTimeProvider;
            _config = config;
        }

        public Task<ProjectSummaryResponseDto> GetSummaryData(int projectId, FiltersRequestDto filtersDto)
        {
            var allReports = GetAllReportsForProject(projectId, filtersDto);
            // var allErrorReports = GetAllReportsForProject(projectId, filtersDto);
            var healthRiskReports = GetFilteredReports(projectId, filtersDto)
                .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity);
            var dataCollectionPointReports = healthRiskReports.Where(r => r.DataCollectionPointCase != null);

            return _nyssContext.Projects
                .Where(ph => ph.Id == projectId)
                .Select(p => new ProjectSummaryResponseDto
                {
                    ReportCount = healthRiskReports.Sum(r => r.ReportedCaseCount),
                    ActiveDataCollectorCount = allReports
                        .Where(r => r.DataCollector.DeletedAt == null)
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

        public async Task<ReportByVillageAndDateResponseDto> GetReportsGroupedByVillageAndDate(int projectId, FiltersRequestDto filtersDto)
        {
            var reports = GetFilteredReports(projectId, filtersDto)
                .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity);

            return filtersDto.GroupingType switch
            {
                FiltersRequestDto.GroupingTypeDto.Day =>
                await GroupReportsByVillageAndDay(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date),

                FiltersRequestDto.GroupingTypeDto.Week =>
                await GroupReportsByVillageAndWeek(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date),

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
                    Period = x.Period.ToString("dd/MM", CultureInfo.InvariantCulture),
                    ReferredCount = x.ReferredCount,
                    DeathCount = x.DeathCount,
                    FromOtherVillagesCount = x.FromOtherVillagesCount
                })
                .ToList();
        }

        private async Task<IList<DataCollectionPointsReportsByDateDto>> GroupReportsByDataCollectionPointFeaturesAndWeek(IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new { r.EpiYear, r.EpiWeek, r.DataCollectionPointCase.ReferredCount, r.DataCollectionPointCase.DeathCount, r.DataCollectionPointCase.FromOtherVillagesCount })
                .GroupBy(r => new { r.EpiYear, r.EpiWeek })
                .Select(grouping => new
                {
                    EpiPeriod = grouping.Key,
                    ReferredCount = (int)grouping.Sum(g => g.ReferredCount),
                    DeathCount = (int)grouping.Sum(g => g.DeathCount),
                    FromOtherVillagesCount = (int)grouping.Sum(g => g.FromOtherVillagesCount),
                })
                .ToListAsync();

            var missingWeeks = _dateTimeProvider.GetEpiWeeksRange(startDate, endDate)
                .Where(epiDate => !groupedReports.Any(r => r.EpiPeriod.EpiYear == epiDate.EpiYear && r.EpiPeriod.EpiWeek == epiDate.EpiWeek))
                .Select(epiDate => new { EpiPeriod = new { epiDate.EpiYear, epiDate.EpiWeek }, ReferredCount = 0, DeathCount = 0, FromOtherVillagesCount = 0 });

            return groupedReports
                .Union(missingWeeks)
                .OrderBy(r => r.EpiPeriod.EpiYear)
                .ThenBy(r => r.EpiPeriod.EpiWeek)
                .Select(x => new DataCollectionPointsReportsByDateDto
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
                    ReportsCount = grouping.Sum(g => g.ReportedCaseCount),
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
                    Total = r.ReportedCaseCount,
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
                    Period = x.Period.ToString("dd/MM", CultureInfo.InvariantCulture),
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
                .Select(r => new { r.EpiYear, r.EpiWeek, r.ReportedCase.CountFemalesAtLeastFive, r.ReportedCase.CountFemalesBelowFive, r.ReportedCase.CountMalesAtLeastFive, r.ReportedCase.CountMalesBelowFive })
                .GroupBy(r => new { r.EpiYear, r.EpiWeek })
                .Select(grouping => new
                {
                    EpiPeriod = grouping.Key,
                    CountFemalesAtLeastFive = (int)grouping.Sum(g => g.CountFemalesAtLeastFive),
                    CountFemalesBelowFive = (int)grouping.Sum(g => g.CountFemalesBelowFive),
                    CountMalesAtLeastFive = (int)grouping.Sum(g => g.CountMalesAtLeastFive),
                    CountMalesBelowFive = (int)grouping.Sum(g => g.CountMalesBelowFive)
                })
                .ToListAsync();

            var missingWeeks = _dateTimeProvider.GetEpiWeeksRange(startDate, endDate)
                .Where(epiDate => !groupedReports.Any(r => r.EpiPeriod.EpiYear == epiDate.EpiYear && r.EpiPeriod.EpiWeek == epiDate.EpiWeek))
                .Select(epiDate => new { EpiPeriod = new { epiDate.EpiYear, epiDate.EpiWeek }, CountFemalesAtLeastFive = 0, CountFemalesBelowFive = 0, CountMalesAtLeastFive = 0, CountMalesBelowFive = 0 });

            return groupedReports
                .Union(missingWeeks)
                .OrderBy(r => r.EpiPeriod.EpiYear)
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

        private async Task<ReportByVillageAndDateResponseDto> GroupReportsByVillageAndDay(IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .GroupBy(r => new { r.ReceivedAt.Date, VillageId = r.Village.Id, VillageName = r.Village.Name })
                .Select(grouping => new
                {
                    Period = grouping.Key.Date,
                    Count = grouping.Sum(g => g.ReportedCaseCount),
                    grouping.Key.VillageId,
                    grouping.Key.VillageName
                })
                .Where(g => g.Count > 0)
                .ToListAsync();

            var reportsGroupedByVillages = groupedReports
                .GroupBy(r => new { r.VillageId, r.VillageName })
                .OrderByDescending(g => g.Sum(w => w.Count))
                .Select(g => new
                {
                    Village = g.Key,
                    Data = g.ToList()
                })
                .ToList();

            var maxVillageCount = _config.View.NumberOfGroupedVillagesInProjectDashboard;

            var truncatedVillagesList = reportsGroupedByVillages
                .Take(maxVillageCount)
                .Union(reportsGroupedByVillages
                    .Skip(maxVillageCount)
                    .SelectMany(_ => _.Data)
                    .GroupBy(_ => true)
                    .Select(g => new
                    {
                        Village = new { VillageId = 0, VillageName = "(rest)" },
                        Data = g.ToList()
                    })
                )
                .Select(x => new ReportByVillageAndDateResponseDto.VillageDto
                {
                    Name = x.Village.VillageName,
                    Periods = x.Data.GroupBy(v => v.Period).OrderBy(v => v.Key)
                        .Select(g => new ReportByVillageAndDateResponseDto.PeriodDto
                        {
                            Period = g.Key.ToString("dd/MM", CultureInfo.InvariantCulture),
                            Count = g.Sum(w => w.Count)
                        })
                        .ToList()
                })
                .ToList();

            var allPeriods = Enumerable
                .Range(0, endDate.Subtract(startDate).Days + 1)
                .Select(i => startDate.AddDays(i).ToString("dd/MM", CultureInfo.InvariantCulture))
                .ToList();

            return new ReportByVillageAndDateResponseDto
            {
                Villages = truncatedVillagesList,
                AllPeriods = allPeriods
            };
        }

        private async Task<ReportByVillageAndDateResponseDto> GroupReportsByVillageAndWeek(IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .GroupBy(r => new { r.EpiWeek, ReceivedAt = r.ReceivedAt.Date, VillageId = r.Village.Id, VillageName = r.Village.Name })
                .Select(grouping => new
                {
                    Period = new
                    {
                        grouping.Key.EpiWeek,
                        grouping.Key.ReceivedAt.Year
                    },
                    Count = grouping.Sum(g => g.ReportedCaseCount),
                    grouping.Key.VillageId,
                    grouping.Key.VillageName
                })
                .Where(g => g.Count > 0)
                .ToListAsync();

            var reportsGroupedByVillages = groupedReports
                .GroupBy(r => new { r.VillageId, r.VillageName })
                .OrderByDescending(g => g.Sum(w => w.Count))
                .Select(g => new
                {
                    Village = g.Key,
                    Data = g.ToList()
                })
                .ToList();

            var maxVillageCount = _config.View.NumberOfGroupedVillagesInProjectDashboard;

            var truncatedVillagesList = reportsGroupedByVillages
                .Take(maxVillageCount)
                .Union(reportsGroupedByVillages
                    .Skip(maxVillageCount)
                    .SelectMany(_ => _.Data)
                    .GroupBy(_ => true)
                    .Select(g => new
                    {
                        Village = new { VillageId = 0, VillageName = "(rest)" },
                        Data = g.ToList()
                    })
                )
                .Select(x => new ReportByVillageAndDateResponseDto.VillageDto
                {
                    Name = x.Village.VillageName,
                    Periods = x.Data.GroupBy(v => v.Period).OrderBy(v => v.Key.Year).ThenBy(x => x.Key.EpiWeek)
                        .Select(g => new ReportByVillageAndDateResponseDto.PeriodDto
                        {
                            Period = g.Key.EpiWeek.ToString(),
                            Count = g.Sum(w => w.Count)
                        })
                        .ToList()
                })
                .ToList();

            var allPeriods = _dateTimeProvider.GetEpiWeeksRange(startDate, endDate)
                .Select(day => day.EpiWeek.ToString());

            return new ReportByVillageAndDateResponseDto
            {
                Villages = truncatedVillagesList,
                AllPeriods = allPeriods
            };
        }

        private static async Task<IList<ReportByDateResponseDto>> GroupReportsByDay(IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new
                {
                    r.ReceivedAt,
                    Total = r.ReportedCaseCount,
                })
                .GroupBy(r => r.ReceivedAt.Date)
                .Select(grouping => new
                {
                    Period = grouping.Key,
                    Count = grouping.Sum(r => r.Total)
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
                    Period = x.Period.ToString("dd/MM", CultureInfo.InvariantCulture),
                    Count = x.Count
                })
                .ToList();
        }

        private async Task<IList<ReportByDateResponseDto>> GroupReportsByWeek(IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new { r.EpiYear, r.EpiWeek, Total = r.ReportedCaseCount })
                .GroupBy(r => new { r.EpiYear, r.EpiWeek })
                .Select(grouping => new { EpiPeriod = grouping.Key, Count = grouping.Sum(r => r.Total) })
                .ToListAsync();

            var missingWeeks = _dateTimeProvider.GetEpiWeeksRange(startDate, endDate)
                .Where(epiDate => !groupedReports.Any(r => r.EpiPeriod.EpiYear == epiDate.EpiYear && r.EpiPeriod.EpiWeek == epiDate.EpiWeek))
                .Select(epiDate => new { EpiPeriod = new { epiDate.EpiYear, epiDate.EpiWeek }, Count = 0 });

            return groupedReports
                .Union(missingWeeks)
                .OrderBy(r => r.EpiPeriod.EpiYear)
                .ThenBy(r => r.EpiPeriod.EpiWeek)
                .Select(x => new ReportByDateResponseDto { Period = x.EpiPeriod.EpiWeek.ToString(), Count = x.Count })
                .ToList();
        }

        private IQueryable<Nyss.Data.Models.Report> GetFilteredReports(int projectId, FiltersRequestDto filtersDto) =>
            GetAllReportsForProject(projectId, filtersDto)
                .AllSuccessfulReports()
                .FilterReportsByRegion(filtersDto.Area)
                .Where(r => !r.MarkedAsError)
                .FilterReportsByHealthRisk(filtersDto.HealthRiskId);

        private IQueryable<Nyss.Data.Models.RawReport> GetAllReportsForProject(int projectId, FiltersRequestDto filtersDto)
        {
            var startDate = filtersDto.StartDate.Date;
            var endDate = filtersDto.EndDate.Date.AddDays(1);
            var reports = _nyssContext.RawReports
                .Where(r => r.IsTraining.HasValue && r.IsTraining == filtersDto.IsTraining);

            return reports
                .FromKnownDataCollector()
                .FilterByDataCollectorType(filtersDto.ReportsType)
                .FilterReportsByProject(projectId)
                .FilterReportsByDate(startDate, endDate);
        }

        private static IQueryable<Nyss.Data.Models.Report> FilterReportsByRegion(IQueryable<Nyss.Data.Models.Report> reports, AreaDto area) =>
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

        private int InactiveDataCollectorCount(int projectId, IQueryable<Nyss.Data.Models.RawReport> reports, FiltersRequestDto filtersDto)
        {
            var activeDataCollectors = reports
                .Select(r => r.DataCollector.Id)
                .Distinct();

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

        public static IQueryable<Nyss.Data.Models.Report> FilterReportsByRegion(this IQueryable<Nyss.Data.Models.Report> reports, AreaDto area) =>
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

        public static IQueryable<Nyss.Data.Models.RawReport> FilterByDataCollectorType(this IQueryable<Nyss.Data.Models.RawReport> reports, FiltersRequestDto.ReportsTypeDto reportsType) =>
            reportsType switch
            {
                FiltersRequestDto.ReportsTypeDto.DataCollector =>
                reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.Human),

                FiltersRequestDto.ReportsTypeDto.DataCollectionPoint =>
                reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                reports
            };

        public static IQueryable<Nyss.Data.Models.Report> AllSuccessfulReports(this IQueryable<Nyss.Data.Models.RawReport> reports) =>
            reports.Where(r => r.Report != null).Select(r => r.Report);


        public static IQueryable<Nyss.Data.Models.RawReport> FilterReportsByDate(this IQueryable<Nyss.Data.Models.RawReport> reports, DateTime startDate, DateTime endDate) =>
            reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

        public static IQueryable<Nyss.Data.Models.RawReport> FilterReportsByProject(this IQueryable<Nyss.Data.Models.RawReport> reports, int projectId) =>
            reports.Where(r => r.DataCollector.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.RawReport> OnlyErrorReports(this IQueryable<Nyss.Data.Models.RawReport> reports) =>
            reports.Where(r => r.ReportId == null);

        public static IQueryable<Nyss.Data.Models.RawReport> FromKnownDataCollector(this IQueryable<Nyss.Data.Models.RawReport> reports) =>
            reports.Where(r => r.DataCollector != null);

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

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterDataCollectorsByArea(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, AreaDto area) =>
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

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterDataCollectorsByProject(this IQueryable<Nyss.Data.Models.DataCollector> reports, int projectId) =>
            reports.Where(dc => dc.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterDataCollectorsByTrainingMode(this IQueryable<Nyss.Data.Models.DataCollector> reports, bool isInTraining) =>
            reports.Where(dc => (isInTraining ? dc.IsInTrainingMode : !dc.IsInTrainingMode));

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterOnlyNotDeleted(this IQueryable<Nyss.Data.Models.DataCollector> reports) =>
            reports.Where(dc => !dc.DeletedAt.HasValue);
    }
}
