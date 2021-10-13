using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NetTopologySuite.Geometries;
using NSubstitute;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Projects;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Features.Users;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Reports
{
    public class ReportServiceTests
    {
        private const int RowsPerPage = 10;

        private readonly IUserService _userService;

        private readonly IProjectService _projectService;

        private IReportService _reportService;

        private readonly INyssContext _nyssContextMock;

        private readonly INyssWebConfig _config;

        private readonly IAuthorizationService _authorizationService;

        private readonly IStringsResourcesService _stringsResourcesService;

        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly IAlertService _alertService;

        private readonly IAlertReportService _alertReportService;

        private readonly IQueueService _queueService;

        private readonly INyssContext _nyssContextInMemory;

        private readonly IStringsService _stringsService;

        private readonly int _rowsPerPage = 10;
        private readonly List<int> _reportIdsFromProject1 = Enumerable.Range(1, 13).ToList();

        private readonly List<int> _reportIdsFromProject2 = Enumerable.Range(14, 11).ToList();

        private readonly List<int> _trainingReportIds = Enumerable.Range(25, 2).ToList();

        private readonly List<int> _dcpReportIds = Enumerable.Range(125, 20).ToList();

        private readonly List<int> _unknownSenderReportIds = Enumerable.Range(145, 5).ToList();

        private readonly List<int> _errorReportIds = Enumerable.Range(150, 10).ToList();

        private readonly ServiceBusQueuesOptions _serviceBusQueuesOptions = new ServiceBusQueuesOptions
        {
            RecalculateAlertsQueue = ""
        };

        public ReportServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();

            _config = Substitute.For<INyssWebConfig>();
            _config.PaginationRowsPerPage.Returns(RowsPerPage);
            _config.ServiceBusQueues.Returns(_serviceBusQueuesOptions);

            _userService = Substitute.For<IUserService>();
            _userService.GetUserApplicationLanguageCode(Arg.Any<string>()).Returns(Task.FromResult("en"));

            _projectService = Substitute.For<IProjectService>();
            _projectService.GetHealthRiskNames(Arg.Any<int>(), Arg.Any<List<HealthRiskType>>()).Returns(
                Task.FromResult(Enumerable.Empty<HealthRiskDto>()));

            _authorizationService = Substitute.For<IAuthorizationService>();

            _stringsResourcesService = Substitute.For<IStringsResourcesService>();
            _stringsResourcesService.GetStrings("en").Returns(new StringsResourcesVault(new Dictionary<string, StringResourceValue>()));

            _dateTimeProvider = Substitute.For<IDateTimeProvider>();

            _alertService = Substitute.For<IAlertService>();
            _queueService = Substitute.For<IQueueService>();
            _stringsService = Substitute.For<IStringsService>();

            _alertReportService = new AlertReportService(
                _config,
                _nyssContextMock,
                _alertService,
                _queueService,
                _dateTimeProvider,
                _authorizationService);
            _reportService = new ReportService(
                _nyssContextMock,
                _userService,
                _projectService,
                _config,
                _authorizationService,
                _dateTimeProvider,
                _alertReportService,
                _stringsService);

            _authorizationService.IsCurrentUserInRole(Role.Supervisor).Returns(false);
            _authorizationService.GetCurrentUserName().Returns("admin@domain.com");
            _authorizationService.GetCurrentUser().Returns(new AdministratorUser { Role = Role.Administrator });

            var builder = new DbContextOptionsBuilder<NyssContext>();
            builder.UseInMemoryDatabase("InMemoryDatabase");
            _nyssContextInMemory = new NyssContext(builder.Options);

            ArrangeData();
        }

        [Fact]
        public async Task List_ShouldReturnPagedResultsFromSpecifiedProject()
        {
            //arrange
            _config.PaginationRowsPerPage.Returns(9999);

            //act
            var result = await _reportService.List(1, 1, new ReportListFilterRequestDto
            {
                Area = null,
                ErrorType = null,
                FormatCorrect = true,
                OrderBy = "",
                ReportStatus = new ReportStatusFilterDto
                {
                    Dismissed = true,
                    Kept = true,
                    NotCrossChecked = true
                },
                ReportType = new ReportTypeFilterDto(),
                SortAscending = true,
                UtcOffset = 0,
                DataCollectorType = ReportListDataCollectorType.Human,
                HealthRiskId = null
            });

            //assert
            result.Value.Data.ShouldAllBe(x => _reportIdsFromProject1.Contains(x.Id));
        }

        [Fact]
        public async Task List_ShouldReturnNumberOfRowsCorrespondingToPageSize()
        {
            //arrange
            _config.PaginationRowsPerPage.Returns(13);

            //act
            var result = await _reportService.List(2, 1, new ReportListFilterRequestDto
            {
                DataCollectorType = ReportListDataCollectorType.CollectionPoint,
                FormatCorrect = true,
                ReportStatus = new ReportStatusFilterDto
                {
                    Kept = true,
                    Dismissed = true,
                    NotCrossChecked = true,
                }
            });

            //assert
            result.Value.Data.Count.ShouldBe(13);
        }

        [Fact]
        public async Task List_WhenSelectedLastPageThatHasLessRows_ShouldReturnLessRows()
        {
            //act
            var result = await _reportService.List(2, 2, new ReportListFilterRequestDto
            {
                DataCollectorType = ReportListDataCollectorType.Human,
                TrainingStatus = TrainingStatusDto.Trained,
                FormatCorrect = true,
                ReportStatus = new ReportStatusFilterDto
                {
                    Kept = true,
                    Dismissed = true,
                    NotCrossChecked = true,
                }
            });

            //assert
            result.Value.Data.Count.ShouldBe(1);
        }

        [Fact]
        public async Task List_WhenReportTypeFilterIsDcp_ShouldReturnOnlyDcpReports()
        {
            //act
            var result = await _reportService.List(2, 1, new ReportListFilterRequestDto
            {
                DataCollectorType = ReportListDataCollectorType.CollectionPoint,
                FormatCorrect = true,
                ReportStatus = new ReportStatusFilterDto
                {
                    Kept = true,
                    Dismissed = true,
                    NotCrossChecked = true,
                }
            });

            //assert
            result.Value.Data.Count.ShouldBe(Math.Min(20, RowsPerPage));
            result.Value.TotalRows.ShouldBe(20);
        }

        [Fact]
        public async Task List_WhenListFilterIsSuccessStatus_ShouldReturnOnlySuccessReports()
        {
            //act
            var result = await _reportService.List(2, 1, new ReportListFilterRequestDto
            {
                DataCollectorType = ReportListDataCollectorType.Human,
                TrainingStatus = TrainingStatusDto.Trained,
                FormatCorrect = true,
                ReportStatus = new ReportStatusFilterDto
                {
                    Kept = true,
                    Dismissed = true,
                    NotCrossChecked = true,
                }
            });

            //assert
            result.Value.Data.Count.ShouldBe(Math.Min(11, RowsPerPage));
            result.Value.TotalRows.ShouldBe(11);
        }

        [Fact]
        public async Task List_WhenListFilterIsArea_ShouldReturnOnlyReportsFromArea()
        {
            //act
            var result = await _reportService.List(1, 1, new ReportListFilterRequestDto
            {
                DataCollectorType = ReportListDataCollectorType.Human,
                FormatCorrect = true,
                ReportStatus = new ReportStatusFilterDto
                {
                    Kept = true,
                    Dismissed = true,
                    NotCrossChecked = true,
                },
                Area = new AreaDto
                {
                    Id = 2,
                    Type = AreaType.Zone
                }
            });

            //assert
            result.Value.Data.Count.ShouldBe(Math.Min(13, RowsPerPage));
            result.Value.TotalRows.ShouldBe(13);
        }

        [Fact]
        public async Task List_WhenListFilterIsDataColletorStatusInTraining_ShouldReturnOnlyReportsInTraining()
        {
            //act
            var result = await _reportService.List(2, 1, new ReportListFilterRequestDto
            {
                DataCollectorType = ReportListDataCollectorType.Human,
                FormatCorrect = true,
                TrainingStatus = TrainingStatusDto.InTraining,
                ReportStatus = new ReportStatusFilterDto
                {
                    Kept = true,
                    Dismissed = true,
                    NotCrossChecked = true,
                },
            });

            //assert
            result.Value.Data.Count.ShouldBe(2);
        }

        [Fact]
        public async Task List_WhenListFilterIsHealthRisk_ShouldReturnOnlyReportsForHealthRisk()
        {
            //act
            var result = await _reportService.List(2, 1, new ReportListFilterRequestDto
            {
                DataCollectorType = ReportListDataCollectorType.Human,
                FormatCorrect = true,
                ReportStatus = new ReportStatusFilterDto
                {
                    Kept = true,
                    Dismissed = true,
                    NotCrossChecked = true,
                },
                HealthRiskId = 2,
            });


            //assert
            result.Value.Data.Count.ShouldBe(Math.Min(11, RowsPerPage));
            result.Value.TotalRows.ShouldBe(11);
        }

        [Theory]
        [InlineData(ReportErrorFilterType.All, 10)]
        [InlineData(ReportErrorFilterType.WrongFormat, 5)]
        [InlineData(ReportErrorFilterType.HealthRiskNotFound, 5)]
        public async Task List_WhenFilteringOnErrorType_ShouldReturnCorrespondingErrorReportsOnly(ReportErrorFilterType errorType, int numberOfReports)
        {
            // Arrange
            ArrangeData(useInMemoryDataBase: true, onlyErrorReports: true);

            var requestDto = new ReportListFilterRequestDto
            {
                Area = null,
                ErrorType = errorType,
                FormatCorrect = false,
                ReportStatus = null,
                ReportType = null,
                DataCollectorType = ReportListDataCollectorType.Human,
                HealthRiskId = null
            };

            // Act
            var res = await _reportService.List(1, 1, requestDto);

            // Assert
            res.Value.Data.Count.ShouldBe(numberOfReports);
        }

        [Fact]
        public async Task AcceptReport_WhenErrorReport_ShouldNotBeAllowed()
        {
            // Arrange
            var rawReports = new List<RawReport> { new RawReport { Id = 1 } };
            var rawReportsMockDbSet = rawReports.AsQueryable().BuildMockDbSet();
            _nyssContextMock.RawReports.Returns(rawReportsMockDbSet);

            // Act
            var result = await _reportService.AcceptReport(1);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Report.ReportNotFound);
        }

        [Fact]
        public async Task AcceptReport_WhenSuccessReport_ShouldChangeStatus()
        {
            // Arrange
            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    Id = 1,
                    Report = new Report
                    {
                        Status = ReportStatus.New,
                        Location = new Point(0, 0)
                    }
                }
            };
            var rawReportsMockDbSet = rawReports.AsQueryable().BuildMockDbSet();
            _nyssContextMock.RawReports.Returns(rawReportsMockDbSet);

            // Act
            var result = await _reportService.AcceptReport(1);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var report = _nyssContextMock.RawReports.First(r => r.Id == 1);
            report.Report.AcceptedAt.ShouldNotBeNull();
            report.Report.Status.ShouldBe(ReportStatus.Accepted);
        }

        [Fact]
        public async Task AcceptReport_WhenReportHasNoLocation_ShouldFail()
        {
            // Arrange
            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    Id = 1,
                    Report = new Report
                    {
                        Status = ReportStatus.New
                    }
                }
            };
            var rawReportsMockDbSet = rawReports.AsQueryable().BuildMockDbSet();
            _nyssContextMock.RawReports.Returns(rawReportsMockDbSet);

            // Act
            var result = await _reportService.AcceptReport(1);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Report.CannotCrossCheckReportWithoutLocation);
        }

        [Fact]
        public async Task AcceptReport_WhenAlreadyAccepted_ShouldFail()
        {
            // Arrange
            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    Id = 1,
                    Report = new Report
                    {
                        Status = ReportStatus.Accepted
                    }
                }
            };
            var rawReportsMockDbSet = rawReports.AsQueryable().BuildMockDbSet();
            _nyssContextMock.RawReports.Returns(rawReportsMockDbSet);

            // Act
            var result = await _reportService.AcceptReport(1);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Report.AlreadyCrossChecked);
        }

        [Fact]
        public async Task AcceptReport_WhenMarkedAsError_ShouldFail()
        {
            // Arrange
            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    Id = 1,
                    Report = new Report
                    {
                        MarkedAsError = true
                    }
                }
            };
            var rawReportsMockDbSet = rawReports.AsQueryable().BuildMockDbSet();
            _nyssContextMock.RawReports.Returns(rawReportsMockDbSet);

            // Act
            var result = await _reportService.AcceptReport(1);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Report.CannotCrossCheckErrorReport);
        }

        [Fact]
        public async Task DismissReport_WhenErrorReport_ShouldNotBeAllowed()
        {
            // Arrange
            var rawReports = new List<RawReport> { new RawReport { Id = 1 } };
            var rawReportsMockDbSet = rawReports.AsQueryable().BuildMockDbSet();
            _nyssContextMock.RawReports.Returns(rawReportsMockDbSet);

            // Act
            var result = await _reportService.DismissReport(1);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Report.ReportNotFound);
        }

        [Fact]
        public async Task DismissReport_WhenSuccessReport_ShouldChangeStatus()
        {
            // Arrange
            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    Id = 1,
                    Report = new Report
                    {
                        Status = ReportStatus.New,
                        Location = new Point(0, 0)
                    }
                }
            };
            var rawReportsMockDbSet = rawReports.AsQueryable().BuildMockDbSet();
            _nyssContextMock.RawReports.Returns(rawReportsMockDbSet);

            // Act
            var result = await _reportService.DismissReport(1);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var report = _nyssContextMock.RawReports.First(r => r.Id == 1);
            report.Report.RejectedAt.ShouldNotBeNull();
            report.Report.Status.ShouldBe(ReportStatus.Rejected);
        }

        [Fact]
        public async Task DismissReport_WhenReportHasNoLocation_ShouldFail()
        {
            // Arrange
            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    Id = 1,
                    Report = new Report
                    {
                        Status = ReportStatus.New
                    }
                }
            };
            var rawReportsMockDbSet = rawReports.AsQueryable().BuildMockDbSet();
            _nyssContextMock.RawReports.Returns(rawReportsMockDbSet);

            // Act
            var result = await _reportService.DismissReport(1);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Report.CannotCrossCheckReportWithoutLocation);
        }

        [Fact]
        public async Task DismissReport_WhenAlreadyDismissed_ShouldFail()
        {
            // Arrange
            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    Id = 1,
                    Report = new Report
                    {
                        Status = ReportStatus.Rejected
                    }
                }
            };
            var rawReportsMockDbSet = rawReports.AsQueryable().BuildMockDbSet();
            _nyssContextMock.RawReports.Returns(rawReportsMockDbSet);

            // Act
            var result = await _reportService.DismissReport(1);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Report.AlreadyCrossChecked);
        }

        [Fact]
        public async Task DismissReport_WhenMarkedAsError_ShouldFail()
        {
            // Arrange
            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    Id = 1,
                    Report = new Report
                    {
                        MarkedAsError = true
                    }
                }
            };
            var rawReportsMockDbSet = rawReports.AsQueryable().BuildMockDbSet();
            _nyssContextMock.RawReports.Returns(rawReportsMockDbSet);

            // Act
            var result = await _reportService.DismissReport(1);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Report.CannotCrossCheckErrorReport);
        }

        [Fact]
        public async Task EditReport_WhenUnknownSender_ShouldSucceed()
        {
            // Arrange
            ArrangeData(withUnknownSenderReports: true);
            var unknownSenderRawReports = _nyssContextMock.RawReports
                .First(rr => rr.DataCollector == null);
            var unknownSenderReport = _nyssContextMock.Reports
                .First(rr => rr.DataCollector == null && rr.Id == unknownSenderRawReports.ReportId);
            var dataCollector = _nyssContextMock.DataCollectors
                .First(dc => dc.Id == 1);

            var reportRequestDto = new ReportRequestDto
            {
                HealthRiskId = unknownSenderReport.ProjectHealthRisk.Id,
                Date = new DateTime(2020, 1, 1),
                CountFemalesBelowFive = 1,
                CountFemalesAtLeastFive = 0,
                CountMalesBelowFive = 0,
                CountMalesAtLeastFive = 0,
                CountUnspecifiedSexAndAge = 0,
                DataCollectorId = dataCollector.Id,
                ReportStatus = ReportStatus.Accepted,
                DataCollectorLocationId = 1
            };

            // Act
            var result = await _reportService.Edit(unknownSenderReport.Id, reportRequestDto);
            var reportAfterEdit = _nyssContextMock.Reports
                .First(rr => rr.Id == unknownSenderReport.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            reportAfterEdit.DataCollector.Id.ShouldBe(dataCollector.Id);
            reportAfterEdit.ReportedCase.CountFemalesBelowFive.ShouldBe(1);
            reportAfterEdit.ReportedCase.CountMalesAtLeastFive.ShouldBe(0);
            reportAfterEdit.ReportedCase.CountFemalesAtLeastFive.ShouldBe(0);
            reportAfterEdit.ReportedCase.CountMalesAtLeastFive.ShouldBe(0);
            reportAfterEdit.ReportedCase.CountUnspecifiedSexAndAge.ShouldBe(0);
            reportAfterEdit.Status.ShouldBe(ReportStatus.Accepted);
            reportAfterEdit.Location.X.ShouldBe(17.047525);
            reportAfterEdit.Location.Y.ShouldBe(52.330898);
            reportAfterEdit.RawReport.Village.Id.ShouldBe(dataCollector.DataCollectorLocations.First().Village.Id);
        }

        private static List<Report> BuildReports(
            DataCollector dataCollector,
            List<int> ids,
            ProjectHealthRisk projectHealthRisk,
            bool isTraining = false)
        {
            var reports = ids
                .Select(i => new Report
                {
                    Id = i,
                    DataCollector = dataCollector,
                    Status = ReportStatus.Pending,
                    ProjectHealthRisk = projectHealthRisk,
                    ReportedCase = new ReportCase(),
                    DataCollectionPointCase = new DataCollectionPointCase(),
                    CreatedAt = new DateTime(2020, 1, 1),
                    IsTraining = isTraining,
                    Location = GetMockPoint(52.411269, 17.025807),
                    ReportType = dataCollector.DataCollectorType == DataCollectorType.CollectionPoint
                        ? ReportType.DataCollectionPoint
                        : ReportType.Single
                })
                .ToList();
            return reports;
        }

        private static List<Report> BuildUnknownSenderReports(List<int> ids, ProjectHealthRisk projectHealthRisk)
        {
            var reports = ids
                .Select(i => new Report
                {
                    Id = i,
                    Status = ReportStatus.New,
                    ProjectHealthRisk = projectHealthRisk,
                    ReportedCase = new ReportCase(),
                    DataCollectionPointCase = new DataCollectionPointCase(),
                    CreatedAt = new DateTime(2020, 1, 1),
                    ReceivedAt = new DateTime(2020, 1, 1),
                    ReportType = ReportType.Single,
                    IsTraining = false,
                    PhoneNumber = "+523543234234"
                })
                .ToList();
            return reports;
        }

        private static List<RawReport> BuildUnknownSenderRawReports(List<Report> reports, NationalSociety nationalSociety) =>
            reports.Select(r => new RawReport
            {
                Id = r.Id,
                Report = r,
                ReportId = r.Id,
                Sender = r.PhoneNumber,
                ReceivedAt = r.ReceivedAt,
                IsTraining = r.IsTraining,
                NationalSociety = nationalSociety
            })
            .ToList();

        private static List<RawReport> BuildRawReports(List<Report> reports, Village village, Zone zone, NationalSociety nationalSociety) =>
            reports.Select(r => new RawReport
                {
                    Id = r.Id,
                    Report = r,
                    ReportId = r.Id,
                    Sender = r.PhoneNumber,
                    DataCollector = r.DataCollector,
                    ReceivedAt = r.ReceivedAt,
                    IsTraining = r.IsTraining,
                    Village = village,
                    Zone = zone,
                    NationalSociety = nationalSociety
                })
                .ToList();

        private static List<RawReport> BuildErrorRawReports(DataCollector dataCollector, List<int> reportIds) =>
            reportIds.Select(id => new RawReport
            {
                Id = id,
                ErrorType = id % 2 == 0 ? ReportErrorType.FormatError : ReportErrorType.HealthRiskNotFound,
                ReceivedAt = new DateTime(2020, 1, 1),
                IsTraining = false,
                DataCollector = dataCollector
            }).ToList();

        private static void LinkUnknownRawReportsToReports(List<Report> reports, List<RawReport> rawReports) =>
            reports.ForEach(r =>
            {
                r.RawReport = rawReports.First(rr => rr.ReportId == r.Id);
            });

        private static Point GetMockPoint(double lat, double lon) =>
            new MockPoint(lon, lat);

        private class MockPoint : Point
        {
            public MockPoint(double x, double y)
                : base(x, y)
            {
            }

            public override double Distance(Geometry g)
            {
                var firstCoordinate = new GeoCoordinate(Y, X);
                var secondCoordinate = new GeoCoordinate(g.Coordinate.Y, g.Coordinate.X);
                return firstCoordinate.GetDistanceTo(secondCoordinate);
            }
        }

        private void ArrangeData(bool withUnknownSenderReports = false, bool useInMemoryDataBase = false, bool onlyErrorReports = false)
        {
            var nationalSocieties = new List<NationalSociety>
            {
                new NationalSociety
                {
                    Id = 1,
                    NationalSocietyUsers = new List<UserNationalSociety>()
                }
            };

            var alertRules = new List<AlertRule> { new AlertRule { Id = 1 } };

            var contentLanguages = new List<ContentLanguage>
            {
                new ContentLanguage
                {
                    Id = 1,
                    DisplayName = "English",
                    LanguageCode = "en"
                }
            };

            var languageContents = new List<HealthRiskLanguageContent>
            {
                new HealthRiskLanguageContent
                {
                    Id = 1,
                    CaseDefinition = "case definition",
                    FeedbackMessage = "feedback message",
                    Name = "health risk name",
                    ContentLanguage = contentLanguages[0]
                }
            };

            var healthRisks = new List<HealthRisk>
            {
                new HealthRisk
                {
                    Id = 1,
                    HealthRiskType = HealthRiskType.Human,
                    HealthRiskCode = 1,
                    LanguageContents = languageContents,
                    AlertRule = alertRules[0]
                },
                new HealthRisk
                {
                    Id = 2,
                    HealthRiskType = HealthRiskType.Human,
                    HealthRiskCode = 1,
                    LanguageContents = languageContents,
                    AlertRule = alertRules[0]
                }
            };

            var projectHealthRisks = new List<ProjectHealthRisk>
            {
                new ProjectHealthRisk
                {
                    Id = 1,
                    HealthRisk = healthRisks[0],
                    HealthRiskId = healthRisks[0].Id,
                    Project = new Project
                    {
                        Id = 1
                    }
                },
                new ProjectHealthRisk
                {
                    Id = 2,
                    HealthRisk = healthRisks[1],
                    HealthRiskId = healthRisks[1].Id,
                    Project = new Project
                    {
                        Id = 2
                    }
                }
            };

            var regions = new List<Region>
            {
                new Region
                {
                    Id = 1,
                    Name = "Region1",
                    NationalSociety = nationalSocieties[0]
                }
            };

            var districts = new List<District>
            {
                new District
                {
                    Id = 1,
                    Name = "District1",
                    Region = regions[0]
                }
            };

            var villages = new List<Village>
            {
                new Village
                {
                    Id = 1,
                    Name = "Village1",
                    District = districts[0]
                }
            };

            var zones = new List<Zone>
            {
                new Zone
                {
                    Id = 1,
                    Name = "Zone1",
                    Village = villages[0]
                },
                new Zone
                {
                    Id = 2,
                    Name = "Zone2",
                    Village = villages[0]
                }
            };

            var projects = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    NationalSocietyId = nationalSocieties[0].Id,
                    NationalSociety = nationalSocieties[0],
                    ProjectHealthRisks = projectHealthRisks.Where(x => x.Id == 1).ToList()
                },
                new Project
                {
                    Id = 2,
                    NationalSocietyId = nationalSocieties[0].Id,
                    NationalSociety = nationalSocieties[0],
                    ProjectHealthRisks = projectHealthRisks.Where(x => x.Id == 2).ToList()
                }
            };

            var dataCollectors = new List<DataCollector>
            {
                new DataCollector
                {
                    Id = 1,
                    Project = projects[0],
                    DataCollectorLocations = new List<DataCollectorLocation>
                    {
                        new DataCollectorLocation
                        {
                            Id = 1,
                            DataCollectorId = 1,
                            Village = villages[0],
                            Location = GetMockPoint(52.330898, 17.047525)
                        }
                    },
                    DataCollectorType = DataCollectorType.Human,
                    Supervisor = new SupervisorUser
                    {
                        Name = "Super"
                    }
                },
                new DataCollector
                {
                    Id = 2,
                    Project = projects[1],
                    DataCollectorLocations = new List<DataCollectorLocation>
                    {
                        new DataCollectorLocation
                        {
                            Id = 2,
                            DataCollectorId = 2,
                            Village = villages[0]
                        }
                    },
                    DataCollectorType = DataCollectorType.Human,
                    Supervisor = new SupervisorUser
                    {
                        Name = "Super"
                    }
                },
                new DataCollector
                {
                    Id = 3,
                    Project = projects[1],
                    DataCollectorLocations = new List<DataCollectorLocation>
                    {
                        new DataCollectorLocation
                        {
                            Id = 3,
                            DataCollectorId = 3,
                            Village = villages[0]
                        }
                    },
                    DataCollectorType = DataCollectorType.CollectionPoint,
                    Supervisor = new SupervisorUser
                    {
                        Name = "Super"
                    }
                },
                new DataCollector
                {
                    Id = 4,
                    Name = "Trainee",
                    Project = projects[1],
                    DataCollectorType = DataCollectorType.Human,
                    IsInTrainingMode = true,
                    DataCollectorLocations = new List<DataCollectorLocation>
                    {
                        new DataCollectorLocation
                        {
                            Id = 4,
                            DataCollectorId = 4,
                            Village = villages[0]
                        }
                    },
                    Supervisor = new SupervisorUser
                    {
                        Name = "Super"
                    }
                }
            };

            var users = new List<User>
            {
                new AdministratorUser
                {
                    Role = Role.Administrator,
                    EmailAddress = "admin@domain.com"
                }
            };

            var (reports, rawReports) = ArrangeReports(
                dataCollectors,
                nationalSocieties,
                villages,
                zones,
                withUnknownSenderReports,
                onlyErrorReports);

            if (useInMemoryDataBase)
            {
                ArrangeInMemoryDb(
                    nationalSocieties,
                    contentLanguages,
                    languageContents,
                    healthRisks,
                    alertRules,
                    projects,
                    projectHealthRisks,
                    regions,
                    districts,
                    villages,
                    dataCollectors,
                    rawReports,
                    users);
            }
            else
            {
                ArrangeMockDb(
                    nationalSocieties,
                    contentLanguages,
                    languageContents,
                    healthRisks,
                    alertRules,
                    projects,
                    projectHealthRisks,
                    regions,
                    districts,
                    villages,
                    dataCollectors,
                    reports,
                    rawReports,
                    users);
            }
        }

        private (List<Report>, List<RawReport>) ArrangeReports(
            List<DataCollector> dataCollectors,
            List<NationalSociety> nationalSocieties,
            List<Village> villages,
            List<Zone> zones,
            bool withUnknownSenderReports,
            bool onlyErrorReports = false)
        {
            if (onlyErrorReports)
            {
                var errorRawReports = BuildErrorRawReports(dataCollectors[0], _errorReportIds).ToList();
                return (new List<Report>(), errorRawReports);
            }

            var reports1 = BuildReports(dataCollectors[0], _reportIdsFromProject1, dataCollectors[0].Project.ProjectHealthRisks.ToList()[0]);
            var rawReports1 = BuildRawReports(reports1, villages[0], zones[1], nationalSocieties[0]);

            var reports2 = BuildReports(dataCollectors[1], _reportIdsFromProject2, dataCollectors[1].Project.ProjectHealthRisks.ToList()[0]);
            var rawReports2 = BuildRawReports(reports2, villages[0], zones[0], nationalSocieties[0]);

            var inTrainingReports = BuildReports(dataCollectors[3], _trainingReportIds, dataCollectors[0].Project.ProjectHealthRisks.ToList()[0], isTraining: true);
            var inTrainingRawReports = BuildRawReports(inTrainingReports, villages[0], zones[0], nationalSocieties[0]);

            var dcpReports = BuildReports(dataCollectors[2], _dcpReportIds, dataCollectors[2].Project.ProjectHealthRisks.ToList()[0]);
            var dcpRawReports = BuildRawReports(dcpReports, villages[0], zones[0], nationalSocieties[0]);



            var reports = reports1.Concat(reports2).Concat(inTrainingReports).Concat(dcpReports).ToList();
            var rawReports = rawReports1.Concat(rawReports2).Concat(inTrainingRawReports).Concat(dcpRawReports).ToList();

            if (withUnknownSenderReports)
            {
                var unknownSenderReports = BuildUnknownSenderReports(_unknownSenderReportIds, dataCollectors[0].Project.ProjectHealthRisks.ToList()[0]);
                var unknownSenderRawReports = BuildUnknownSenderRawReports(unknownSenderReports, nationalSocieties[0]);
                LinkUnknownRawReportsToReports(unknownSenderReports, unknownSenderRawReports);

                reports = reports.Concat(unknownSenderReports).ToList();
                rawReports = rawReports.Concat(unknownSenderRawReports).ToList();
            }
            else
            {
                reports = reports.ToList();
                rawReports = rawReports.ToList();
            }

            return (reports, rawReports);
        }

        private void ArrangeMockDb(
            List<NationalSociety> nationalSocieties,
            List<ContentLanguage> contentLanguages,
            List<HealthRiskLanguageContent> languageContents,
            List<HealthRisk> healthRisks,
            List<AlertRule> alertRules,
            List<Project> projects,
            List<ProjectHealthRisk> projectHealthRisks,
            List<Region> regions,
            List<District> districts,
            List<Village> villages,
            List<DataCollector> dataCollectors,
            List<Report> reports,
            List<RawReport> rawReports,
            List<User> users)
        {
            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            var contentLanguageMockDbSet = contentLanguages.AsQueryable().BuildMockDbSet();
            var languageContentsMockDbSet = languageContents.AsQueryable().BuildMockDbSet();
            var healthRisksMockDbSet = healthRisks.AsQueryable().BuildMockDbSet();
            var alertRuleMockDbSet = alertRules.AsQueryable().BuildMockDbSet();
            var contentLanguagesMockDbSet = contentLanguages.AsQueryable().BuildMockDbSet();
            var projectsDbSet = projects.AsQueryable().BuildMockDbSet();
            var projectHealthRisksDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            var regionsDbSet = regions.AsQueryable().BuildMockDbSet();
            var districtsDbSet = districts.AsQueryable().BuildMockDbSet();
            var villagesDbSet = villages.AsQueryable().BuildMockDbSet();
            var dataCollectorsDbSet = dataCollectors.AsQueryable().BuildMockDbSet();
            var reportsDbSet = reports.AsQueryable().BuildMockDbSet();
            var rawReportsDbSet = rawReports.AsQueryable().BuildMockDbSet();
            var usersDbSet = users.AsQueryable().BuildMockDbSet();
            var dataCollectorLocationsDbSet = dataCollectors.SelectMany(dc => dc.DataCollectorLocations).AsQueryable().BuildMockDbSet();

            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesDbSet);
            _nyssContextMock.ContentLanguages.Returns(contentLanguageMockDbSet);
            _nyssContextMock.HealthRiskLanguageContents.Returns(languageContentsMockDbSet);
            _nyssContextMock.HealthRisks.Returns(healthRisksMockDbSet);
            _nyssContextMock.AlertRules.Returns(alertRuleMockDbSet);
            _nyssContextMock.ContentLanguages.Returns(contentLanguagesMockDbSet);
            _nyssContextMock.Projects.Returns(projectsDbSet);
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksDbSet);
            _nyssContextMock.Regions.Returns(regionsDbSet);
            _nyssContextMock.Districts.Returns(districtsDbSet);
            _nyssContextMock.Villages.Returns(villagesDbSet);
            _nyssContextMock.DataCollectors.Returns(dataCollectorsDbSet);
            _nyssContextMock.Reports.Returns(reportsDbSet);
            _nyssContextMock.RawReports.Returns(rawReportsDbSet);
            _nyssContextMock.Users.Returns(usersDbSet);
            _nyssContextMock.DataCollectorLocations.Returns(dataCollectorLocationsDbSet);

            _nyssContextMock.Projects.FindAsync(1).Returns(projects.Single(x => x.Id == 1));
            _nyssContextMock.Projects.FindAsync(2).Returns(projects.Single(x => x.Id == 2));
        }

        private void ArrangeInMemoryDb(
            List<NationalSociety> nationalSocieties,
            List<ContentLanguage> contentLanguages,
            List<HealthRiskLanguageContent> languageContents,
            List<HealthRisk> healthRisks,
            List<AlertRule> alertRules,
            List<Project> projects,
            List<ProjectHealthRisk> projectHealthRisks,
            List<Region> regions,
            List<District> districts,
            List<Village> villages,
            List<DataCollector> dataCollectors,
            List<RawReport> rawReports,
            List<User> users)
        {
            _nyssContextInMemory.Database.EnsureDeleted();

            _nyssContextInMemory.NationalSocieties.AddRange(nationalSocieties);
            _nyssContextInMemory.ContentLanguages.AddRange(contentLanguages);
            _nyssContextInMemory.HealthRiskLanguageContents.AddRange(languageContents);
            _nyssContextInMemory.HealthRisks.AddRange(healthRisks);
            _nyssContextInMemory.AlertRules.AddRange(alertRules);
            _nyssContextInMemory.Projects.AddRange(projects);
            _nyssContextInMemory.ProjectHealthRisks.AddRange(projectHealthRisks);
            _nyssContextInMemory.Regions.AddRange(regions);
            _nyssContextInMemory.Districts.AddRange(districts);
            _nyssContextInMemory.Villages.AddRange(villages);
            _nyssContextInMemory.DataCollectors.AddRange(dataCollectors);
            _nyssContextInMemory.RawReports.AddRange(rawReports);
            _nyssContextInMemory.Users.AddRange(users);
            _nyssContextInMemory.DataCollectorLocations.AddRange(dataCollectors.SelectMany(dc => dc.DataCollectorLocations));

            _nyssContextInMemory.SaveChanges();
            _nyssContextInMemory.Database.EnsureCreated();

            _reportService = new ReportService(
                _nyssContextInMemory,
                _userService,
                _projectService,
                _config,
                _authorizationService,
                _dateTimeProvider,
                _alertReportService,
                _stringsService);
        }
    }
}
