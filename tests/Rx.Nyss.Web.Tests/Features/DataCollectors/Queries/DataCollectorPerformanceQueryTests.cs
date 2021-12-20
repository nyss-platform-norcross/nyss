using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.DataCollectors.Queries;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataCollectors.Queries
{
    public class DataCollectorPerformanceQueryTests
    {
        private const int ProjectId = 1;
        private const int DataCollectorWithReportsId = 1;
        private const int DataCollectorWithoutReportsId = 2;
        private const string DataCollectorWithReportsName = "HasReported";
        private const string DataCollectorWithoutReportsName = "NoReports";
        private const DayOfWeek EpiWeekStartDay = DayOfWeek.Sunday;
        private readonly DateTime _startDate;
        private readonly INyssContext _nyssContextMock;
        private readonly DataCollectorPerformanceQuery.Handler _handler;

        public DataCollectorPerformanceQueryTests()
        {
            _startDate = new DateTime(2021, 12, 10);
            var _authorizationService = Substitute.For<IAuthorizationService>();
            var dataCollectorServiceMock = Substitute.For<IDataCollectorService>();
            var dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
            var configMock = Substitute.For<INyssWebConfig>();
            var dataCollectorPerformanceServiceMock = new DataCollectorPerformanceService(dateTimeProviderMock);
            _nyssContextMock = Substitute.For<INyssContext>();

            ArrangeEpiWeekStandardData();

            var dataCollectorsQueryable = ArrangeDataCollectorData();
            configMock.PaginationRowsPerPage.Returns(10);
            _authorizationService.GetCurrentUserName().Returns("admin@example.com");
            dateTimeProviderMock.UtcNow.Returns(_startDate.AddDays(7 * 8));
            dateTimeProviderMock.GetEpiDateRange(Arg.Any<DateTime>(), Arg.Any<DateTime>(), EpiWeekStartDay).Returns(new List<EpiDate>
            {
                new (49, 2021),
                new (50, 2021),
                new (51, 2021),
                new (52, 2021),
                new (1, 2022),
                new (2, 2022),
                new (3, 2022),
                new (4, 2022)
            });
            dateTimeProviderMock.GetEpiDate(_startDate.AddDays(3), Arg.Any<DayOfWeek>()).Returns(new EpiDate(50, 2021));
            dateTimeProviderMock.GetEpiDate(_startDate.AddDays(16), Arg.Any<DayOfWeek>()).Returns(new EpiDate(52, 2021));
            dateTimeProviderMock.GetFirstDateOfEpiWeek(2021, 50, Arg.Any<DayOfWeek>()).Returns(new DateTime(2021, 12, 12));
            dateTimeProviderMock.GetFirstDateOfEpiWeek(2021, 52, Arg.Any<DayOfWeek>()).Returns(new DateTime(2021, 12, 26));
            dataCollectorServiceMock.GetDataCollectorsForCurrentUserInProject(ProjectId).Returns(dataCollectorsQueryable);

            _handler = new DataCollectorPerformanceQuery.Handler(_nyssContextMock, dataCollectorServiceMock, dataCollectorPerformanceServiceMock, dateTimeProviderMock, configMock);
        }

        [Fact]
        public async Task GetDataCollectorPerformance_WhenDataCollectorsHaveReported_ShouldReturnCorrectStatus()
        {
            // Arrange
            var query = new DataCollectorPerformanceQuery(ProjectId, new DataCollectorPerformanceFiltersRequestDto { TrainingStatus = TrainingStatusDto.Trained, PageNumber = 1 });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Performance.Data
                .Where(dc => dc.Name == DataCollectorWithReportsName)
                .SelectMany(dc => dc.PerformanceInEpiWeeks)
                .Where(p => p.EpiWeek == 50)
                .Select(p => p.ReportingStatus)
                .First()
                .ShouldBe(ReportingStatus.ReportingWithErrors);
        }

        [Fact]
        public async Task GetDataCollectorPerformance_WhenDataCollectorsHaveReported_ShouldReturnCorrectCompleteness()
        {
            // Arrange
            var query = new DataCollectorPerformanceQuery(ProjectId, new DataCollectorPerformanceFiltersRequestDto { TrainingStatus = TrainingStatusDto.Trained, PageNumber = 1 });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var completenessInWeek50 = result.Value.Completeness
                .First(c => c.EpiWeek == 50);
            var completenessInWeek52 = result.Value.Completeness
                .First(c => c.EpiWeek == 52);
            completenessInWeek50.TotalDataCollectors.ShouldBe(1);
            completenessInWeek50.ActiveDataCollectors.ShouldBe(1);
            completenessInWeek52.TotalDataCollectors.ShouldBe(2);
            completenessInWeek52.ActiveDataCollectors.ShouldBe(1);
        }

        private IQueryable<DataCollector> ArrangeDataCollectorData()
        {
            var nationalSocieties = new List<NationalSociety>
            {
                new NationalSociety
                {
                    Id = 1,
                    EpiWeekStartDay = EpiWeekStartDay,
                    NationalSocietyUsers = new List<UserNationalSociety> { new UserNationalSociety { User = new AdministratorUser { EmailAddress = "admin@example.com" } } }
                }
            };
            var projects = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    NationalSociety = nationalSocieties[0]
                }
            };
            var dataCollectors = new List<DataCollector>
            {
                new DataCollector
                {
                    Id = DataCollectorWithReportsId,
                    Name = DataCollectorWithReportsName,
                    Project = projects[0],
                    RawReports = new List<RawReport>
                    {
                        new RawReport
                        {
                            ReceivedAt = _startDate.AddDays(3),
                            IsTraining = false
                        },
                        new RawReport
                        {
                            ReceivedAt = _startDate.AddDays(16),
                            IsTraining = false
                        }
                    },
                    DatesNotDeployed = new List<DataCollectorNotDeployed>(),
                    DataCollectorLocations = new List<DataCollectorLocation>
                    {
                        new DataCollectorLocation
                        {
                            Village = new Village()
                        }
                    }
                },
                new DataCollector
                {
                    Id = DataCollectorWithoutReportsId,
                    Name = DataCollectorWithoutReportsName,
                    Project = projects[0],
                    RawReports = new List<RawReport>(),
                    DatesNotDeployed = new List<DataCollectorNotDeployed>
                    {
                        new DataCollectorNotDeployed
                        {
                            StartDate = _startDate,
                            EndDate = _startDate.AddDays(14)
                        }
                    },
                    DataCollectorLocations = new List<DataCollectorLocation>
                    {
                        new DataCollectorLocation
                        {
                            Village = new Village()
                        }
                    }
                }
            };

            var rawReports = dataCollectors[0].RawReports;
            foreach (var rawReport in rawReports)
            {
                rawReport.DataCollector = dataCollectors[0];
            }

            return dataCollectors.AsQueryable().BuildMockDbSet();
        }

        private void ArrangeEpiWeekStandardData()
        {
            var nationalSocieties = new List<NationalSociety> { new () { EpiWeekStartDay = EpiWeekStartDay } };
            var projects = new List<Project>
            {
                new()
                {
                    Id = ProjectId,
                    NationalSociety = nationalSocieties[0]
                }
            };

            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            var projectsDbSet = projects.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesDbSet);
            _nyssContextMock.Projects.Returns(projectsDbSet);
        }
    }
}
