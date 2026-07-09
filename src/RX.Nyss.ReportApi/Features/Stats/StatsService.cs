using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.ReportApi.Configuration;
using RX.Nyss.ReportApi.Features.Stats.Contracts;

namespace RX.Nyss.ReportApi.Features.Stats
{
    public interface IStatsService
    {
        Task<NyssStats> CalculateStats();
    }

    public class StatsService : IStatsService
    {
        private readonly INyssContext _nyssContext;

        private readonly IDataBlobService _dataBlobService;

        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly List<int> _nationalSocietiesToExclude;

        public StatsService(
            INyssContext nyssContext,
            IDataBlobService dataBlobService,
            IDateTimeProvider dateTimeProvider,
            INyssReportApiConfig config)
        {
            _nyssContext = nyssContext;
            _dataBlobService = dataBlobService;
            _dateTimeProvider = dateTimeProvider;
            _nationalSocietiesToExclude = config.NationalSocietiesToExcludeFromPublicStats
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();
        }

        public async Task<NyssStats> CalculateStats()
        {
            var activeThreshold = _dateTimeProvider.UtcNow.AddDays(-7);

            var activeDataCollectors = await _nyssContext.DataCollectors
                .AsNoTracking()
                .Where(dc => !_nationalSocietiesToExclude.Contains(dc.Project.NationalSocietyId))
                .Where(dc => dc.RawReports.Any(r => r.ReceivedAt > activeThreshold))
                .CountAsync();

            var escalatedAlerts = await _nyssContext.Alerts
                .AsNoTracking()
                .Where(a => !_nationalSocietiesToExclude.Contains(a.ProjectHealthRisk.Project.NationalSocietyId))
                .Where(a => a.EscalatedAt.HasValue)
                .CountAsync();

            var activeProjectIds = await _nyssContext.RawReports
                .AsNoTracking()
                // Only include reports received within the last month
                .Where(rr => rr.ReceivedAt > DateTime.UtcNow.AddMonths(-1))
                .Include(r => r.DataCollector)
                .ThenInclude(dc => dc.Project)
                .Where(r => r.DataCollector != null)
                .Select(r => r.DataCollector.Project.Id)
                .ToListAsync();

            var activeProjects = (await _nyssContext.Projects
                .AsNoTracking()
                .Where(p => p.State == ProjectState.Open)
                .Where(p => !_nationalSocietiesToExclude.Contains(p.NationalSocietyId) && activeProjectIds.Contains(p.Id))
                .ToListAsync()).Count;

            var withReportsProjectIds = await _nyssContext.RawReports
                .AsNoTracking()
                .Include(r => r.DataCollector)
                .ThenInclude(dc => dc.Project)
                .Where(r => r.DataCollector != null)
                .Select(r => r.DataCollector.Project.Id)
                .ToListAsync();

            var totalProjects = (await _nyssContext.Projects
                .AsNoTracking()
                .Where(p => !_nationalSocietiesToExclude.Contains(p.NationalSocietyId) && withReportsProjectIds.Contains(p.Id))
                .ToListAsync()).Count;

            var stats = new NyssStats
            {
                EscalatedAlerts = escalatedAlerts,
                ActiveDataCollectors = activeDataCollectors,
                ActiveProjects = activeProjects,
                TotalProjects = totalProjects,
            };

            await _dataBlobService.StorePublicStats(JsonConvert.SerializeObject(stats));

            return stats;
        }
    }
}
