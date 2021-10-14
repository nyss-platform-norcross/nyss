using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using Newtonsoft.Json;
using NSubstitute;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Configuration;
using RX.Nyss.ReportApi.Features.Stats;
using RX.Nyss.ReportApi.Features.Stats.Contracts;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Features.Stats
{
    public class StatsServiceTests
    {
        private readonly INyssContext _nyssContext;

        private readonly IStatsService _statsService;

        private readonly IDataBlobService _dataBlobService;

        public StatsServiceTests()
        {
            var config = Substitute.For<INyssReportApiConfig>();
            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            dateTimeProvider.UtcNow.Returns(new DateTime(2020, 2, 6));

            _nyssContext = Substitute.For<INyssContext>();
            _dataBlobService = Substitute.For<IDataBlobService>();

            _statsService = new StatsService(_nyssContext, _dataBlobService, dateTimeProvider, config);
        }

        [Fact]
        public async Task CalculateStats_ShouldReturnCorrectNumbers()
        {
            // Arrange
            GenerateTestData();

            // Act
            await _statsService.CalculateStats();

            // Assert
            var result = new NyssStats
            {
                ActiveProjects = 2,
                EscalatedAlerts = 1,
                TotalProjects = 3,
                ActiveDataCollectors = 1
            };

            await _dataBlobService.Received(1).StorePublicStats(Arg.Is(JsonConvert.SerializeObject(result)));
        }

        private void GenerateTestData()
        {
            var nationalSocieties = new List<NationalSociety> { new NationalSociety { } };
            var projects = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    State = ProjectState.Open,
                    NationalSocietyId = 1
                },
                new Project
                {
                    Id = 2,
                    State = ProjectState.Open,
                    NationalSocietyId = 1
                },
                new Project
                {
                    Id = 3,
                    State = ProjectState.Closed,
                    NationalSocietyId = 1
                }
            };

            var dataCollectors = new List<DataCollector>
            {
                new DataCollector
                {
                    Id = 1,
                    Project = projects[0],
                    RawReports = new List<RawReport> { new RawReport { ReceivedAt = new DateTime(2020, 2, 2) } }
                },
                new DataCollector
                {
                    Id = 2,
                    Project = projects[0],
                    RawReports = new List<RawReport>()
                }
            };
            var alerts = new List<Data.Models.Alert>
            {
                new Data.Models.Alert
                {
                    EscalatedAt = new DateTime(2020, 2, 22, 10, 55, 43),
                    ProjectHealthRisk = new ProjectHealthRisk { Project = new Project() }
                }
            };

            var rawReports = new[]
            {
                new RawReport
                {
                    DataCollector = new DataCollector { Project = new Project { Id = 1 } },
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Project = new Project { Id = 2 } },
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Project = new Project { Id = 3 } },
                },
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            var projectsMockDbSet = projects.AsQueryable().BuildMockDbSet();
            var dataCollectorsMockDbSet = dataCollectors.AsQueryable().BuildMockDbSet();
            var alertsMockDbSet = alerts.AsQueryable().BuildMockDbSet();
            var rawReportsMockDbSet = rawReports.AsQueryable().BuildMockDbSet();

            _nyssContext.NationalSocieties.Returns(nationalSocietiesMockDbSet);
            _nyssContext.Projects.Returns(projectsMockDbSet);
            _nyssContext.DataCollectors.Returns(dataCollectorsMockDbSet);
            _nyssContext.Alerts.Returns(alertsMockDbSet);
            _nyssContext.RawReports.Returns(rawReportsMockDbSet);
        }
    }
}
