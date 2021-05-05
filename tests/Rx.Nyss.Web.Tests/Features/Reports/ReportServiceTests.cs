using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
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
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly IReportService _reportService;
        private readonly INyssContext _nyssContextMock;
        private readonly INyssWebConfig _config;
        private readonly IAuthorizationService _authorizationService;
        private readonly IExcelExportService _excelExportService;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly int _rowsPerPage = 10;
        private readonly List<int> _reportIdsFromProject1 = Enumerable.Range(1, 13).ToList();
        private readonly List<int> _reportIdsFromProject2 = Enumerable.Range(14, 11).ToList();
        private readonly List<int> _trainingReportIds = Enumerable.Range(15, 100).ToList();
        private readonly List<int> _dcpReportIds = Enumerable.Range(115, 20).ToList();

        public ReportServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();

            _config = Substitute.For<INyssWebConfig>();
            _config.PaginationRowsPerPage.Returns(_rowsPerPage);

            _userService = Substitute.For<IUserService>();
            _userService.GetUserApplicationLanguageCode(Arg.Any<string>()).Returns(Task.FromResult("en"));

            _projectService = Substitute.For<IProjectService>();
            _projectService.GetHealthRiskNames(Arg.Any<int>(), Arg.Any<List<HealthRiskType>>()).Returns(Task.FromResult(Enumerable.Empty<HealthRiskDto>()));

            _authorizationService = Substitute.For<IAuthorizationService>();

            _excelExportService = Substitute.For<IExcelExportService>();
            _stringsResourcesService = Substitute.For<IStringsResourcesService>();
            _stringsResourcesService.GetStringsResources("en").Returns(Task.FromResult(new Result<IDictionary<string, StringResourceValue>>(new Dictionary<string, StringResourceValue>(), true)));

            _dateTimeProvider = Substitute.For<IDateTimeProvider>();

            _reportService = new ReportService(_nyssContextMock, _userService, _projectService, _config, _authorizationService, _stringsResourcesService, _dateTimeProvider);

            _authorizationService.IsCurrentUserInRole(Role.Supervisor).Returns(false);
            _authorizationService.GetCurrentUserName().Returns("admin@domain.com");
            _authorizationService.GetCurrentUser().Returns(new AdministratorUser { Role = Role.Administrator });
            ArrangeData();
        }


        private void ArrangeData()
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
                    HealthRiskId = healthRisks[0].Id
                },
                new ProjectHealthRisk
                {
                    Id = 2,
                    HealthRisk = healthRisks[1],
                    HealthRiskId = healthRisks[1].Id
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
                    Id = 2,
                    Project = projects[1],
                    DataCollectorLocations = new List<DataCollectorLocation>
                    {
                        new DataCollectorLocation
                        {
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
                            Village = villages[0]
                        }
                    },
                    DataCollectorType = DataCollectorType.CollectionPoint,
                    Supervisor = new SupervisorUser
                    {
                        Name = "Super"
                    }
                }
            };

            var reports1 = BuildReports(dataCollectors[0], _reportIdsFromProject1, dataCollectors[0].Project.ProjectHealthRisks.ToList()[0]);
            var rawReports1 = BuildRawReports(reports1, villages[0], zones[1], nationalSocieties[0]);

            var reports2 = BuildReports(dataCollectors[1], _reportIdsFromProject2, dataCollectors[1].Project.ProjectHealthRisks.ToList()[0]);
            var rawReports2 = BuildRawReports(reports2, villages[0], zones[0], nationalSocieties[0]);

            var trainingReports = BuildReports(dataCollectors[0], _trainingReportIds, dataCollectors[0].Project.ProjectHealthRisks.ToList()[0], isTraining: true);
            var trainingRawReports = BuildRawReports(trainingReports, villages[0], zones[0], nationalSocieties[0]);

            var dcpReports = BuildReports(dataCollectors[2], _dcpReportIds, dataCollectors[2].Project.ProjectHealthRisks.ToList()[0]);
            var dcpRawReports = BuildRawReports(dcpReports, villages[0], zones[0], nationalSocieties[0]);

            var reports = reports1.Concat(reports2).Concat(trainingReports).Concat(dcpReports).ToList();
            var rawReports = rawReports1.Concat(rawReports2).Concat(trainingRawReports).Concat(dcpRawReports).ToList();

            var users = new List<User>
            {
                new AdministratorUser
                {
                    Role = Role.Administrator,
                    EmailAddress = "admin@domain.com"
                }
            };

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

            _nyssContextMock.Projects.FindAsync(1).Returns(projects.Single(x => x.Id == 1));
            _nyssContextMock.Projects.FindAsync(2).Returns(projects.Single(x => x.Id == 2));
        }


        [Fact]
        public async Task List_ShouldReturnPagedResultsFromSpecifiedProject()
        {
            //arrange
            _config.PaginationRowsPerPage.Returns(9999);

            //act
            var result = await _reportService.List(1, 1, new ReportListFilterRequestDto());

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
                    Training = false
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
                FormatCorrect = true,
                ReportStatus = new ReportStatusFilterDto
                {
                    Kept = true,
                    Dismissed = true,
                    NotCrossChecked = true,
                    Training = false
                }
            });

            //assert
            result.Value.Data.Count.ShouldBe(1);
        }

        [Fact]
        public async Task List_WhenListFilterIsTraining_ShouldReturnOnlyTraining()
        {
            //act
            var result = await _reportService.List(1, 1, new ReportListFilterRequestDto
            {
                DataCollectorType = ReportListDataCollectorType.Human,
                FormatCorrect = true,
                ReportStatus = new ReportStatusFilterDto
                {
                    Kept = false,
                    Dismissed = false,
                    NotCrossChecked = false,
                    Training = true
                }
            });

            //assert
            result.Value.Data.Count.ShouldBe(Math.Min(100, _rowsPerPage));
            result.Value.TotalRows.ShouldBe(100);
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
                    Training = false
                }
            });

            //assert
            result.Value.Data.Count.ShouldBe(Math.Min(20, _rowsPerPage));
            result.Value.TotalRows.ShouldBe(20);
        }

        [Fact]
        public async Task List_WhenListFilterIsSuccessStatus_ShouldReturnOnlySuccessReports()
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
                    Training = false
                }
            });

            //assert
            result.Value.Data.Count.ShouldBe(Math.Min(11, _rowsPerPage));
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
                    Training = false
                },
                Area = new AreaDto
                {
                    Id = 2,
                    Type = AreaType.Zone
                }
            });

            //assert
            result.Value.Data.Count.ShouldBe(Math.Min(13, _rowsPerPage));
            result.Value.TotalRows.ShouldBe(13);
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
                    Training = false
                },
                HealthRiskId = 2
            });


            //assert
            result.Value.Data.Count.ShouldBe(Math.Min(11, _rowsPerPage));
            result.Value.TotalRows.ShouldBe(11);
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
                        Status = ReportStatus.New
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
                        Status = ReportStatus.New
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

        private static List<Report> BuildReports(DataCollector dataCollector, List<int> ids, ProjectHealthRisk projectHealthRisk, bool? isTraining = false)
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
                    IsTraining = isTraining ?? false,
                    ReportType = dataCollector.DataCollectorType == DataCollectorType.CollectionPoint
                        ? ReportType.DataCollectionPoint
                        : ReportType.Single
                })
                .ToList();
            return reports;
        }

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
    }
}
