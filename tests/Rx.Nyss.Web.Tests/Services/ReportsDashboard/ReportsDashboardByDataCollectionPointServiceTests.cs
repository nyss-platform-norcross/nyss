using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Services.ReportsDashboard
{
    public class ReportsDashboardByDataCollectionPointServiceTests
    {
        private readonly ReportsDashboardByDataCollectionPointService _reportsDashboardByDataCollectionPointService;
        private readonly List<Report> _reports;
        private readonly IReportService _reportService;

        public ReportsDashboardByDataCollectionPointServiceTests()
        {
            _reports = new List<Report>();
            var reportsDbSet = _reports.AsQueryable().BuildMockDbSet();

            var dateTimeProvider = new DateTimeProvider();

            _reportService = Substitute.For<IReportService>();
            _reportsDashboardByDataCollectionPointService = new ReportsDashboardByDataCollectionPointService(_reportService, dateTimeProvider);

            _reportService.GetDashboardHealthRiskEventReportsQuery(Arg.Any<ReportsFilter>()).Returns(reportsDbSet);
        }

        [Fact]
        public async Task GetDataCollectionPointReports_ShouldCorrectlyCountCases()
        {
            _reports.AddRange(new[]
            {
                new Report
                {
                    ReportType = ReportType.DataCollectionPoint,
                    ReceivedAt = new DateTime(2020, 1, 1),
                    DataCollectionPointCase = new DataCollectionPointCase
                    {
                        DeathCount = 1,
                        ReferredCount = 2,
                        FromOtherVillagesCount = 3
                    }
                },
                new Report
                {
                    ReportType = ReportType.DataCollectionPoint,
                    ReceivedAt = new DateTime(2020, 1, 2),
                    DataCollectionPointCase = new DataCollectionPointCase
                    {
                        DeathCount = 4,
                        ReferredCount = 5,
                        FromOtherVillagesCount = 6
                    }
                },
                new Report
                {
                    ReportType = ReportType.Single,
                    ReceivedAt = new DateTime(2020, 1, 2),
                    DataCollectionPointCase = new DataCollectionPointCase
                    {
                        DeathCount = 4,
                        ReferredCount = 5,
                        FromOtherVillagesCount = 6
                    }
                }
            });

            var result = await _reportsDashboardByDataCollectionPointService.GetDataCollectionPointReports(new ReportsFilter(), DatesGroupingType.Day, DayOfWeek.Sunday);

            result.Sum(r => r.ReferredCount).ShouldBe(7);
        }

        [Fact]
        public async Task GetDataCollectionPointReports_ShouldCorrectlyGroupValuesByDay()
        {
            _reports.AddRange(new[]
            {
                new Report
                {
                    ReportType = ReportType.DataCollectionPoint,
                    ReceivedAt = new DateTime(2020, 1, 1),
                    DataCollectionPointCase = new DataCollectionPointCase { DeathCount = 1 }
                },
                new Report
                {
                    ReportType = ReportType.DataCollectionPoint,
                    ReceivedAt = new DateTime(2020, 1, 2),
                    DataCollectionPointCase = new DataCollectionPointCase { DeathCount = 2 }
                },
                new Report
                {
                    ReportType = ReportType.DataCollectionPoint,
                    ReceivedAt = new DateTime(2020, 1, 2),
                    DataCollectionPointCase = new DataCollectionPointCase { DeathCount = 3 }
                }
            });

            var result = await _reportsDashboardByDataCollectionPointService.GetDataCollectionPointReports(new ReportsFilter(), DatesGroupingType.Day, DayOfWeek.Sunday);
        }

        [Fact]
        public async Task GetDataCollectionPointReports_ShouldCorrectlyGroupValuesByWeek()
        {
            _reports.AddRange(new[]
            {
                new Report
                {
                    ReportType = ReportType.DataCollectionPoint,
                    ReceivedAt = new DateTime(2020, 1, 1),
                    EpiWeek = 1,
                    EpiYear = 2020,
                    DataCollectionPointCase = new DataCollectionPointCase { DeathCount = 1 }
                },
                new Report
                {
                    ReportType = ReportType.DataCollectionPoint,
                    ReceivedAt = new DateTime(2020, 1, 2),
                    EpiWeek = 53,
                    EpiYear = 2019,
                    DataCollectionPointCase = new DataCollectionPointCase { DeathCount = 2 }
                },
                new Report
                {
                    ReportType = ReportType.DataCollectionPoint,
                    ReceivedAt = new DateTime(2019, 12, 27),
                    EpiWeek = 53,
                    EpiYear = 2019,
                    DataCollectionPointCase = new DataCollectionPointCase { DeathCount = 3 }
                }
            });

            var result = await _reportsDashboardByDataCollectionPointService.GetDataCollectionPointReports(new ReportsFilter(), DatesGroupingType.Week, DayOfWeek.Sunday);
        }
    }
}
