using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Features.Project;
using RX.Nyss.Web.Features.Project.Dto;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.ProjectDashboard
{
    public interface IProjectDashboardService
    {
        Task<Result<ProjectDashboardFiltersResponseDto>> GetDashboardFiltersData(int projectId);
        Task<Result<ProjectDashboardResponseDto>> GetDashboardData(int projectId, FiltersRequestDto filtersDto);
    }

    public class ProjectDashboardService : IProjectDashboardService
    {
        private readonly INyssContext _nyssContext;
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;
        private readonly IProjectService _projectService;

        public ProjectDashboardService(
            INyssContext nyssContext,
            INationalSocietyStructureService nationalSocietyStructureService,
            IProjectService projectService)
        {
            _nyssContext = nyssContext;
            _nationalSocietyStructureService = nationalSocietyStructureService;
            _projectService = projectService;
        }

        public async Task<Result<ProjectDashboardFiltersResponseDto>> GetDashboardFiltersData(int projectId)
        {
            var nationalSocietyId = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.NationalSocietyId)
                .SingleAsync();

            var structure = await _nationalSocietyStructureService.GetStructure(nationalSocietyId);

            if (!structure.IsSuccess)
            {
                return Error<ProjectDashboardFiltersResponseDto>(structure.Message.Key);
            }

            var projectHealthRiskNames = await _projectService.GetProjectHealthRiskNames(projectId);

            var dto = new ProjectDashboardFiltersResponseDto
            {
                Regions = structure.Value.Regions,
                HealthRisks = projectHealthRiskNames
                    .Select(p => new ProjectDashboardFiltersResponseDto.HealthRiskDto { Id = p.Id, Name = p.Name })
            };

            return Success(dto);
        }

        public async Task<Result<ProjectDashboardResponseDto>> GetDashboardData(int projectId, FiltersRequestDto filtersDto)
        {
            var dashboardDataDto = new ProjectDashboardResponseDto
            {
                Summary = await GetSummaryData(projectId, filtersDto),
                ReportsGroupedByDate  = await GetReportsGroupedByDate(projectId, filtersDto)
            };

            return Success(dashboardDataDto);
        }

        private async Task<ProjectSummaryResponseDto> GetSummaryData(int projectId, FiltersRequestDto filtersDto)
        {
            var reports = GetFilteredReports(projectId, filtersDto);

            return await _nyssContext.Projects
                .Where(ph => ph.Id == projectId)
                .Select(p => new ProjectSummaryResponseDto
                {
                    ReportCount = reports.Count(),
                    ActiveDataCollectorCount = reports
                        .Select(r => r.DataCollector.Id)
                        .Distinct().Count(),
                    InactiveDataCollectorCount = reports
                        .Where(r => r.DataCollector.CreatedAt < filtersDto.StartDate.Date && (!r.DataCollector.DeletedAt.HasValue || r.DataCollector.DeletedAt >= filtersDto.StartDate.Date))
                        .Select(r => r.DataCollector.Id)
                        .Distinct().Count(),
                    InTrainingDataCollectorCount = reports
                        .Where(r => r.IsTraining)
                        .Select(r => r.DataCollector.Id)
                        .Distinct().Count(),
                })
                .FirstOrDefaultAsync();
        }

        private async Task<IEnumerable<ReportByDateResponseDto>> GetReportsGroupedByDate(int projectId, FiltersRequestDto filtersDto)
        {
            var reports = GetFilteredReports(projectId, filtersDto);

            return await GroupReportsByDay(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date);
        }

        private static async Task<IEnumerable<ReportByDateResponseDto>> GroupReportsByDay(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .GroupBy(r => r.ReceivedAt.Date)
                .Select(grouping => new { Period = grouping.Key, Count = grouping.Count() })
                .ToListAsync();

            var missingDays = Enumerable
                .Range(0, endDate.Subtract(startDate).Days + 1)
                .Select(i => startDate.AddDays(i))
                .Where(day => !groupedReports.Any(r => r.Period == day))
                .Select(day => new { Period = day, Count = 0 });

            return groupedReports
                .Union(missingDays)
                .OrderBy(r => r.Period)
                .Select(x => new ReportByDateResponseDto { Period = x.Period.ToString("dd/MM"), Count = x.Count})
                .ToList();
        }

        private static async Task<IEnumerable<ReportByDateResponseDto>> GroupReportsByWeek(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .GroupBy(r => new { r.ReceivedAt.Year, r.EpiWeek })
                .Select(grouping => new { EpiPeriod = grouping.Key, Count = grouping.Count() })
                .ToListAsync();

            var missingWeeks = Enumerable
                .Range(0, (endDate.Subtract(startDate).Days / 7) + 1)
                .Select(w => startDate.AddDays(w * 7))
                .Where(day => !groupedReports.Any(r => r.EpiPeriod.Year == day.Year && r.EpiPeriod.EpiWeek == GetEpiWeek(day)))
                .Select(day => new { EpiPeriod = new { day.Year, EpiWeek = GetEpiWeek(day) }, Count = 0 });

            return groupedReports
                .Union(missingWeeks)
                .OrderBy(r => r.EpiPeriod.Year)
                .ThenBy(r => r.EpiPeriod.EpiWeek)
                .Select(x => new ReportByDateResponseDto { Period = x.EpiPeriod.EpiWeek.ToString(), Count = x.Count })
                .ToList();
        }

        private IQueryable<Report> GetFilteredReports(int projectId, FiltersRequestDto filtersDto)
        {
            var startDate = filtersDto.StartDate.Date;
            var endDate = filtersDto.EndDate.Date.AddDays(1);

            var reports = _nyssContext.Reports
                .Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate
                    && r.DataCollector.Project.Id == projectId
                    && (!filtersDto.HealthRiskId.HasValue || r.ProjectHealthRisk.HealthRiskId == filtersDto.HealthRiskId.Value));

            return FilterReportsByRegion(reports, filtersDto.Area);
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

        private static int GetEpiWeek(DateTime startDate) => 0;
    }
}
