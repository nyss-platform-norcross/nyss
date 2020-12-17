using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RX.Nyss.Common.Configuration;
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
        Task CalculateStats();
    }

    public class StatsService : IStatsService
    {
        private readonly INyssContext _nyssContext;
        private readonly IDataBlobService _dataBlobService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly List<int> _nationalSocietiesToExclude;

        public StatsService(INyssContext nyssContext, IDataBlobService dataBlobService, IDateTimeProvider dateTimeProvider, INyssReportApiConfig config)
        {
            _nyssContext = nyssContext;
            _dataBlobService = dataBlobService;
            _dateTimeProvider = dateTimeProvider;
            _nationalSocietiesToExclude = config.NationalSocietiesToExcludeFromPublicStats
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(ns => int.Parse(ns))
                .ToList();
        }

        public async Task CalculateStats()
        {
            var activeThreshold = _dateTimeProvider.UtcNow.AddDays(-7);
            var activeDataCollectors = await _nyssContext.DataCollectors
                .Where(dc => !_nationalSocietiesToExclude.Contains(dc.Project.NationalSocietyId))
                .Where(dc => dc.RawReports.Any(r => r.ReceivedAt > activeThreshold))
                .CountAsync();
            var escalatedAlerts = await _nyssContext.Alerts
                .Where(a => !_nationalSocietiesToExclude.Contains(a.ProjectHealthRisk.Project.NationalSocietyId))
                .Where(a => a.Status == AlertStatus.Escalated)
                .CountAsync();
            var allProjects = await _nyssContext.Projects
                .Where(p => !_nationalSocietiesToExclude.Contains(p.NationalSocietyId))
                .Select(p => p.State)
                .ToListAsync();

            var stats = new NyssStats
            {
                EscalatedAlerts = escalatedAlerts,
                ActiveDataCollectors = activeDataCollectors,
                ActiveProjects = allProjects.Count(state => state == ProjectState.Open),
                TotalProjects = allProjects.Count()
            };

            await _dataBlobService.StorePublicStats(JsonConvert.SerializeObject(stats));
        }
    }
}
