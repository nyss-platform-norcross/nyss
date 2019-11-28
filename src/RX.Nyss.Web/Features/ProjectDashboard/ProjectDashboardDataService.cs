using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
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

        public async Task<ProjectSummaryResponseDto> GetSummaryData(int projectId, FiltersRequestDto filtersDto)
        {
            var reports = GetFilteredReports(projectId, filtersDto);

            return await _nyssContext.Projects
                .Where(ph => ph.Id == projectId)
                .Select(p => new ProjectSummaryResponseDto
                {
                    ReportCount = (int)reports.Sum(r => r.ReportedCase.CountFemalesAtLeastFive + r.ReportedCase.CountFemalesBelowFive + r.ReportedCase.CountMalesAtLeastFive + r.ReportedCase.CountMalesBelowFive),
                    ActiveDataCollectorCount = reports
                        .Select(r => r.DataCollector.Id)
                        .Distinct().Count(),
                    InactiveDataCollectorCount = reports
                        .Where(r => r.DataCollector.CreatedAt < filtersDto.StartDate.Date &&
                            (!r.DataCollector.DeletedAt.HasValue || r.DataCollector.DeletedAt >= filtersDto.StartDate.Date))
                        .Select(r => r.DataCollector.Id)
                        .Distinct().Count(),
                    InTrainingDataCollectorCount = reports
                        .Where(r => r.IsTraining)
                        .Select(r => r.DataCollector.Id)
                        .Distinct().Count(),
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IList<ReportByDateResponseDto>> GetReportsGroupedByDate(int projectId, FiltersRequestDto filtersDto)
        {
            var reports = GetFilteredReports(projectId, filtersDto);

            // TODO: add grouping by EpiWeek when the reports flow is done
            return await GroupReportsByDay(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date);
        }

        public async Task<IList<ReportByFeaturesAndDateResponseDto>> GetReportsGroupedByFeaturesAndDate(int projectId, FiltersRequestDto filtersDto)
        {
            var reports = GetFilteredReports(projectId, filtersDto);

            // TODO: add grouping by EpiWeek when the reports flow is done
            return await GroupReportsByFeaturesAndDay(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date);
        }

        private static async Task<IList<ReportByFeaturesAndDateResponseDto>> GroupReportsByFeaturesAndDay(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new
                {
                    r.ReceivedAt,
                    r.ReportedCase.CountFemalesAtLeastFive,
                    r.ReportedCase.CountFemalesBelowFive,
                    r.ReportedCase.CountMalesAtLeastFive,
                    r.ReportedCase.CountMalesBelowFive
                })
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
                .Select(day => new
                {
                    Period = day,
                    CountFemalesAtLeastFive = 0,
                    CountFemalesBelowFive = 0,
                    CountMalesAtLeastFive = 0,
                    CountMalesBelowFive = 0
                });

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

        private static async Task<IList<ReportByDateResponseDto>> GroupReportsByDay(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new
                {
                    r.ReceivedAt,
                    Total = r.ReportedCase.CountFemalesAtLeastFive + r.ReportedCase.CountFemalesBelowFive + r.ReportedCase.CountMalesAtLeastFive + r.ReportedCase.CountMalesBelowFive,
                })
                .GroupBy(r => r.ReceivedAt.Date)
                .Select(grouping => new
                {
                    Period = grouping.Key, Count = (int)grouping.Sum(r => r.Total)
                })
                .ToListAsync();

            var missingDays = Enumerable
                .Range(0, endDate.Subtract(startDate).Days + 1)
                .Select(i => startDate.AddDays(i))
                .Where(day => !groupedReports.Any(r => r.Period == day))
                .Select(day => new
                {
                    Period = day, Count = 0
                });

            return groupedReports
                .Union(missingDays)
                .OrderBy(r => r.Period)
                .Select(x => new ReportByDateResponseDto
                {
                    Period = x.Period.ToString("dd/MM"), Count = x.Count
                })
                .ToList();
        }

        private async Task<IList<ReportByDateResponseDto>> GroupReportsByWeek(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .GroupBy(r => new
                {
                    r.ReceivedAt.Year, r.EpiWeek
                })
                .Select(grouping => new
                {
                    EpiPeriod = grouping.Key, Count = (int)grouping.Sum(r => r.ReportedCase.CountFemalesAtLeastFive + r.ReportedCase.CountFemalesBelowFive + r.ReportedCase.CountMalesAtLeastFive + r.ReportedCase.CountMalesBelowFive)
                })
                .ToListAsync();

            var missingWeeks = Enumerable
                .Range(0, (endDate.Subtract(startDate).Days / 7) + 1)
                .Select(w => startDate.AddDays(w * 7))
                .Where(day => !groupedReports.Any(r => r.EpiPeriod.Year == day.Year && r.EpiPeriod.EpiWeek == _dateTimeProvider.GetEpiWeek(day)))
                .Select(day => new
                {
                    EpiPeriod = new
                    {
                        day.Year, EpiWeek = _dateTimeProvider.GetEpiWeek(day)
                    },
                    Count = 0
                });

            return groupedReports
                .Union(missingWeeks)
                .OrderBy(r => r.EpiPeriod.Year)
                .ThenBy(r => r.EpiPeriod.EpiWeek)
                .Select(x => new ReportByDateResponseDto
                {
                    Period = x.EpiPeriod.EpiWeek.ToString(), Count = x.Count
                })
                .ToList();
        }

        private IQueryable<Report> GetFilteredReports(int projectId, FiltersRequestDto filtersDto)
        {
            var startDate = filtersDto.StartDate.Date;
            var endDate = filtersDto.EndDate.Date.AddDays(1);

            return FilterReportsByRegion(_nyssContext.Reports, filtersDto.Area)
                .Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate
                    && r.DataCollector.Project.Id == projectId
                    && (!filtersDto.HealthRiskId.HasValue || r.ProjectHealthRisk.HealthRiskId == filtersDto.HealthRiskId.Value));
        }

        private static IQueryable<Report> FilterReportsByRegion(IQueryable<Report> reports, FiltersRequestDto.AreaDto area) =>
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
    }
}
