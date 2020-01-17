using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;
using RX.Nyss.Web.Features.Projects.Dto;

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

        public async Task<ProjectSummaryResponseDto> GetSummaryData(int projectId, FiltersRequestDto filtersDto)
        {
            var assignedRawReports = GetAssignedRawReports(projectId, filtersDto);
            var validReports = GetValidReports(projectId, filtersDto);
            var dataCollectionPointReports = validReports.Where(r => r.DataCollectionPointCase != null);

            return await _nyssContext.Projects
                .Where(ph => ph.Id == projectId)
                .Select(ph => new
                {
                    allDataCollectorCount = AllDataCollectorCount(filtersDto, projectId),
                    activeDataCollectorCount = assignedRawReports.Select(r => r.DataCollector.Id).Distinct().Count()
                })
                .Select(data => new ProjectSummaryResponseDto
                {
                    ReportCount = validReports.Sum(r => r.ReportedCaseCount),
                    ActiveDataCollectorCount = data.activeDataCollectorCount,
                    InactiveDataCollectorCount = data.allDataCollectorCount - data.activeDataCollectorCount,
                    ErrorReportCount = assignedRawReports.Count() - validReports.Count(),
                    DataCollectionPointSummary = new DataCollectionPointsSummaryResponse
                    {
                        FromOtherVillagesCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.FromOtherVillagesCount ?? 0),
                        ReferredToHospitalCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.ReferredCount ?? 0),
                        DeathCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.DeathCount ?? 0),
                    },
                    AlertsSummary = AlertsSummary(projectId, filtersDto)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IList<ReportByDateResponseDto>> GetReportsGroupedByDate(int projectId, FiltersRequestDto filtersDto)
        {
            var reports = GetValidReports(projectId, filtersDto);

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
            var reports = GetValidReports(projectId, filtersDto)
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
            var reports = GetValidReports(projectId, filtersDto);

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

            var reports = GetValidReports(projectId, filtersDto)
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

        private static async Task<IList<DataCollectionPointsReportsByDateDto>> GroupReportsByDataCollectionPointFeaturesAndDay(IQueryable<Report> reports, DateTime startDate,
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

        private async Task<IList<DataCollectionPointsReportsByDateDto>> GroupReportsByDataCollectionPointFeaturesAndWeek(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
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
            var reports = GetValidReports(projectId, filtersDto);

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
            var reports = GetValidReports(projectId, filtersDto);

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

        private static async Task<IList<ReportByFeaturesAndDateResponseDto>> GroupReportsByFeaturesAndDay(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
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

        private async Task<IList<ReportByFeaturesAndDateResponseDto>> GroupReportsByFeaturesAndWeek(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
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

        private async Task<ReportByVillageAndDateResponseDto> GroupReportsByVillageAndDay(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
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

        private async Task<ReportByVillageAndDateResponseDto> GroupReportsByVillageAndWeek(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
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
                    Periods = x.Data.GroupBy(v => v.Period).OrderBy(g => g.Key.Year).ThenBy(g => g.Key.EpiWeek)
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

        private static async Task<IList<ReportByDateResponseDto>> GroupReportsByDay(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
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

        private async Task<IList<ReportByDateResponseDto>> GroupReportsByWeek(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
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

        private IQueryable<RawReport> GetAssignedRawReports(int projectId, FiltersRequestDto filtersDto) =>
            _nyssContext.RawReports
                .FilterByTrainingMode(filtersDto.IsTraining)
                .FromKnownDataCollector()
                .FilterByArea(filtersDto.Area)
                .FilterByDataCollectorType(MapToDataCollectorType(filtersDto.ReportsType))
                .FilterByProject(projectId)
                .FilterByDate(filtersDto.StartDate.Date, filtersDto.EndDate.Date.AddDays(1))
                .FilterByHealthRisk(filtersDto.HealthRiskId);

        private IQueryable<Report> GetValidReports(int projectId, FiltersRequestDto filtersDto) =>
            GetAssignedRawReports(projectId, filtersDto)
                .AllSuccessfulReports()
                .Select(r => r.Report)
                .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity)
                .Where(r => !r.MarkedAsError);

        private int AllDataCollectorCount(FiltersRequestDto filtersDto, int projectId) =>
            _nyssContext.DataCollectors
                .FilterByArea(filtersDto.Area)
                .FilterByType(MapToDataCollectorType(filtersDto.ReportsType))
                .FilterByProject(projectId)
                .FilterByTrainingMode(filtersDto.IsTraining)
                .FilterOnlyNotDeletedBefore(filtersDto.StartDate)
                .Count();

        private AlertsSummaryResponseDto AlertsSummary(int projectId, FiltersRequestDto filtersDto)
        {
            var startDate = filtersDto.StartDate;
            var endDate = filtersDto.EndDate.AddDays(1);

            var alerts = _nyssContext.Alerts
                .FilterByProject(projectId)
                .FilterByDateAndStatus(startDate, endDate);

            return new AlertsSummaryResponseDto
            {
                Escalated = alerts.Count(a => a.Status == AlertStatus.Escalated),
                Dismissed = alerts.Count(a => a.Status == AlertStatus.Dismissed),
                Closed = alerts.Count(a => a.Status == AlertStatus.Closed)
            };
        }

        private DataCollectorType? MapToDataCollectorType(FiltersRequestDto.ReportsTypeDto reportsType) =>
            reportsType switch
            {
                FiltersRequestDto.ReportsTypeDto.DataCollector => DataCollectorType.Human,
                FiltersRequestDto.ReportsTypeDto.DataCollectionPoint => DataCollectorType.CollectionPoint,
                _ => null as DataCollectorType?
            };
    }
}
