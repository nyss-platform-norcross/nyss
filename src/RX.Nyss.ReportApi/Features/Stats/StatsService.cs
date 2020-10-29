using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Services;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
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
        private const int DEMO_NS = 4;

        public StatsService(INyssContext nyssContext, IDataBlobService dataBlobService)
        {
            _nyssContext = nyssContext;
            _dataBlobService = dataBlobService;
        }

        public async Task CalculateStats()
        {
            var activeDataCollectors = await _nyssContext.DataCollectors
                .Where(dc => dc.Project.NationalSocietyId != DEMO_NS)
                .Where(dc => dc.RawReports.Any(r => r.ReceivedAt > DateTime.UtcNow.AddDays(-7)))
                .CountAsync();
            var escalatedAlerts = await _nyssContext.Alerts
                .Where(a => a.ProjectHealthRisk.Project.NationalSocietyId != DEMO_NS)
                .Where(a => a.Status == AlertStatus.Escalated)
                .CountAsync();
            var allProjects = await _nyssContext.Projects
                .Where(p => p.NationalSocietyId != DEMO_NS)
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
