using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.AlertEvents;
using RX.Nyss.Web.Features.AlertEvents.Dto;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.AlertEvents
{
    public class AlertEventsServiceTests
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly AlertEventsService _alertEventsService;
        private List<Alert> _alerts;
        private List<AlertEventLog> _alertEventLogItems;
        private List<AlertEventType> _alertEventTypes;
        private readonly User _currentUser = new AdministratorUser() { Id = 1 };

        public AlertEventsServiceTests()
        {

            _nyssContext = Substitute.For<INyssContext>();
            _authorizationService = Substitute.For<IAuthorizationService>();

            _alertEventsService = new AlertEventsService(_nyssContext, _authorizationService);

            RunTestData();
        }

        internal void RunTestData()
        {
            var nationalSocieties = new List<NationalSociety>
            {
                new NationalSociety
                {
                    Id = 1,
                    NationalSocietyUsers = new List<UserNationalSociety>(),
                    ContentLanguage = new ContentLanguage
                    {
                        Id = 4,
                        LanguageCode = "en"
                    }
                }
            };

            var userNationalSociety = new List<UserNationalSociety>
            {
                new UserNationalSociety
                {
                    Organization = new Organization { Id = 1 },
                    UserId = 1,
                    NationalSociety = nationalSocieties[0]
                }
            };

            var nationalSocietiesDbSet = nationalSocieties.AsQueryable()
                .BuildMockDbSet();
            _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);

            var unsDbSet = userNationalSociety.AsQueryable()
                .BuildMockDbSet();
            _nyssContext.UserNationalSocieties.Returns(unsDbSet);

            _alerts = TestData.GetAlerts(nationalSocieties[0]);
            var alertsDbSet = _alerts.AsQueryable()
                .BuildMockDbSet();
            _nyssContext.Alerts.Returns(alertsDbSet);

            _alertEventLogItems = TestData.GetEventLog();
            var alertEventLogsDbSet = _alertEventLogItems.AsQueryable()
                .BuildMockDbSet();
            _nyssContext.AlertEventLogs.Returns(alertEventLogsDbSet);

            _authorizationService.GetCurrentUser()
                .Returns(_currentUser);

        }


        [Fact]
        public async Task GetAlertEventLogs_ShouldOrderItemsByDate()
        {
            var reportAcceptedAt = new DateTime(2020, 1, 5);
            var escalatedAt = new DateTime(2020, 1, 10);
            var closedAt = new DateTime(2020, 1, 15);
            var notInvestigatedAt = new DateTime(2021, 6, 22);
            var detailsRegisteredAt = new DateTime(2021, 6, 23);

            _alerts.First().Status = AlertStatus.Closed;
            _alerts.First().EscalatedAt = escalatedAt;
            _alerts.First().EscalatedBy = _currentUser;
            _alerts.First().ClosedAt = closedAt;
            _alerts.First().ClosedBy = _currentUser;

            _alerts.First().AlertReports.First().Report.Status = ReportStatus.Accepted;
            _alerts.First().AlertReports.First().Report.AcceptedAt = reportAcceptedAt;
            _alerts.First().AlertReports.First().Report.AcceptedBy = _currentUser;

            var result = await _alertEventsService.GetLogItems(TestData.AlertId, 1);

            result.IsSuccess.ShouldBeTrue();
            result.Value.LogItems.Count().ShouldBe(6);
            result.Value.LogItems.ElementAt(0).Date.ShouldBe(TestData.AlertCreatedAt.AddHours(1));
            result.Value.LogItems.ElementAt(1).Date.ShouldBe(reportAcceptedAt.AddHours(1));
            result.Value.LogItems.ElementAt(2).Date.ShouldBe(escalatedAt.AddHours(1));
            result.Value.LogItems.ElementAt(3).Date.ShouldBe(closedAt.AddHours(1));
            result.Value.LogItems.ElementAt(4).Date.ShouldBe(notInvestigatedAt.AddHours(1));
            result.Value.LogItems.ElementAt(5).Date.ShouldBe(detailsRegisteredAt.AddHours(1));
        }


        [Fact]
        public async Task GetAlertLogs_ShouldMapAlertCreation()
        {
            var result = await _alertEventsService.GetLogItems(TestData.AlertId, 0);

            result.IsSuccess.ShouldBeTrue();
            result.Value.LogItems.ElementAt(0).Date.ShouldBe(TestData.AlertCreatedAt);
            result.Value.LogItems.ElementAt(0).LogType.ShouldBe(AlertEventsLogResponseDto.LogType.TriggeredAlert);
            result.Value.LogItems.ElementAt(0).LoggedBy.ShouldBe(null);
        }

        [Fact]
        public async Task GetAlertLogs_ShouldMapReportAcceptance()
        {
            var date = new DateTime(2020, 1, 5);
            var userName = "User";
            var user = new GlobalCoordinatorUser { Name = userName };

            _alerts.First().AlertReports.First().Report.Status = ReportStatus.Accepted;
            _alerts.First().AlertReports.First().Report.AcceptedAt = date;
            _alerts.First().AlertReports.First().Report.AcceptedBy = user;

            var result = await _alertEventsService.GetLogItems(TestData.AlertId, 0);

            result.IsSuccess.ShouldBeTrue();
            result.Value.LogItems.ElementAt(1).Date.ShouldBe(date);
            result.Value.LogItems.ElementAt(1).LogType.ShouldBe(AlertEventsLogResponseDto.LogType.AcceptedReport);
            result.Value.LogItems.ElementAt(1).LoggedBy.ShouldBe(userName);
        }

        [Fact]
        public async Task GetAlertLogs_ShouldMapReportDismissal()
        {
            var date = new DateTime(2020, 1, 5);
            var userName = "User";
            var user = new GlobalCoordinatorUser { Name = userName };

            _alerts.First().AlertReports.First().Report.Status = ReportStatus.Rejected;
            _alerts.First().AlertReports.First().Report.RejectedAt = date;
            _alerts.First().AlertReports.First().Report.RejectedBy = user;

            var result = await _alertEventsService.GetLogItems(TestData.AlertId, 0);

            result.IsSuccess.ShouldBeTrue();
            result.Value.LogItems.ElementAt(1).Date.ShouldBe(date);
            result.Value.LogItems.ElementAt(1).LogType.ShouldBe(AlertEventsLogResponseDto.LogType.RejectedReport);
            result.Value.LogItems.ElementAt(1).LoggedBy.ShouldBe(userName);
        }

        [Fact]
        public async Task GetAlertLogs_ShouldMapAlertEscalation()
        {
            var date = new DateTime(2020, 1, 5);
            var userName = "User";
            var user = new GlobalCoordinatorUser { Name = userName };

            _alerts.First().Status = AlertStatus.Escalated;
            _alerts.First().EscalatedAt = date;
            _alerts.First().EscalatedBy = user;

            var result = await _alertEventsService.GetLogItems(TestData.AlertId, 0);

            result.IsSuccess.ShouldBeTrue();
            result.Value.LogItems.ElementAt(1).Date.ShouldBe(date);
            result.Value.LogItems.ElementAt(1).LogType.ShouldBe(AlertEventsLogResponseDto.LogType.EscalatedAlert);
            result.Value.LogItems.ElementAt(1).LoggedBy.ShouldBe(userName);
        }

        [Fact]
        public async Task GetAlertLogs_ShouldMapAlertDismissal()
        {
            var date = new DateTime(2020, 1, 5);
            var userName = "User";
            var user = new GlobalCoordinatorUser { Name = userName };

            _alerts.First().Status = AlertStatus.Dismissed;
            _alerts.First().DismissedAt = date;
            _alerts.First().DismissedBy = user;

            var result = await _alertEventsService.GetLogItems(TestData.AlertId, 0);

            result.IsSuccess.ShouldBeTrue();
            result.Value.LogItems.ElementAt(1).Date.ShouldBe(date);
            result.Value.LogItems.ElementAt(1).LogType.ShouldBe(AlertEventsLogResponseDto.LogType.DismissedAlert);
            result.Value.LogItems.ElementAt(1).LoggedBy.ShouldBe(userName);
        }

        [Fact]
        public async Task GetAlertLogs_ShouldMapAlertClosure()
        {
            var date = new DateTime(2020, 1, 5);
            var userName = "User";
            var user = new GlobalCoordinatorUser { Name = userName };

            _alerts.First().Status = AlertStatus.Closed;
            _alerts.First().ClosedAt = date;
            _alerts.First().ClosedBy = user;

            var result = await _alertEventsService.GetLogItems(TestData.AlertId, 0);

            result.IsSuccess.ShouldBeTrue();
            result.Value.LogItems.ElementAt(1).Date.ShouldBe(date);
            result.Value.LogItems.ElementAt(1).LogType.ShouldBe(AlertEventsLogResponseDto.LogType.ClosedAlert);
            result.Value.LogItems.ElementAt(1).LoggedBy.ShouldBe(userName);
        }

        internal static class TestData
        {
            public const int AlertId = 1;
            public const int ReportId = 23;
            public const int ContentLanguageId = 4;
            public const string ContentLanguageCode = "en";
            public const string ApiKey = "123";
            public static readonly DateTime AlertCreatedAt = new DateTime(2020, 1, 5);
            public static readonly ManagerUser DefaultUser = new ManagerUser() { Name = "DefaultManager" };

            public const int AlertEventTypeId = 1;
            public const int AlertEventSubtypeId = 2;
            public const string AlertEventTypeDetails = "Details";
            public const string AlertEventTypeInvestigation = "Investigation";
            public const string AlertEventSubtypeInvestigated = "Investigated";
            public const string AlertEventSubtypeNotInvestigated = "NotInvestigated";

            public static readonly DateTime Event1CreatedAt = new DateTime(2021, 6, 22);
            public static readonly DateTime Event2CreatedAt = new DateTime(2021, 6, 23);

            public static List<Alert> GetAlerts(NationalSociety nationalSociety) =>
                new List<Alert>
                {
                    new Alert
                    {
                        Id = AlertId,
                        Status = AlertStatus.Escalated,
                        CreatedAt = AlertCreatedAt,
                        EscalatedBy = DefaultUser,
                        DismissedBy = DefaultUser,
                        ClosedBy = DefaultUser,
                        AlertReports = new List<AlertReport>
                        {
                            new AlertReport
                            {
                                Report = new Report
                                {
                                    Id = ReportId,
                                    Status = ReportStatus.Pending,
                                    AcceptedBy = DefaultUser,
                                    RejectedBy = DefaultUser,
                                    ResetBy = DefaultUser,
                                    RawReport = new RawReport
                                    {
                                        ApiKey = ApiKey,
                                        Village = new Village
                                        {
                                            Name = "Village 1",
                                            District = new District
                                            {
                                                Name = "District 9",
                                                Region = new Region { Name = "Region 1" }
                                            }
                                        }
                                    },
                                    DataCollector = new DataCollector { Supervisor = new SupervisorUser { SupervisorAlertRecipients = new List<SupervisorUserAlertRecipient>() } }
                                }
                            }
                        },
                        ProjectHealthRisk = new ProjectHealthRisk
                        {
                            AlertRule = new AlertRule(),
                            HealthRisk = new HealthRisk
                            {
                                LanguageContents = new List<HealthRiskLanguageContent>
                                {
                                    new HealthRiskLanguageContent
                                    {
                                        ContentLanguage = new ContentLanguage
                                        {
                                            Id = ContentLanguageId,
                                            LanguageCode = ContentLanguageCode
                                        }
                                    }
                                }
                            },
                            Project = new Project
                            {
                                AlertNotificationRecipients = new List<AlertNotificationRecipient>
                                {
                                    new AlertNotificationRecipient
                                    {
                                        Email = "aaaa@example.com",
                                        PhoneNumber = "+423423223"
                                    }
                                },
                                NationalSociety = nationalSociety
                            }
                        }
                    }
                };

            public static List<AlertEventLog> GetEventLog() =>
                new List<AlertEventLog>
                {
                    new AlertEventLog
                    {
                        Id = 1,
                        AlertId = AlertId,
                        AlertEventType = new AlertEventType { Name = AlertEventTypeInvestigation, Id = AlertEventTypeId},
                        AlertEventSubtype = new AlertEventSubtype { Name = AlertEventSubtypeInvestigated, AlertEventTypeId = AlertEventTypeId},
                        Text = "The case was not investigated.",
                        CreatedAt = Event1CreatedAt,
                        LoggedBy = DefaultUser
                    },
                    new AlertEventLog
                    {
                        Id = 2,
                        AlertId = AlertId,
                        AlertEventType = new AlertEventType { Name = AlertEventTypeDetails },
                        AlertEventSubtype = new AlertEventSubtype { Name = null },
                        Text = "The monkey died of corona and the danger is over.",
                        CreatedAt = Event2CreatedAt,
                        LoggedBy = DefaultUser
                    }
                };

        }
    }
}
