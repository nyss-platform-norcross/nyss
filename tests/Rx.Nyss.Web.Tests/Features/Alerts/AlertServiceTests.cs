using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Alerts
{
    public class AlertServiceTests
    {
        private readonly INyssContext _nyssContext;
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly IEmailTextGeneratorService _emailTextGeneratorService;
        private readonly IConfig _config;
        private readonly AlertService _alertService;
        private readonly List<Alert> _alerts;

        public AlertServiceTests()
        {
            _nyssContext = Substitute.For<INyssContext>();
            _emailPublisherService = Substitute.For<IEmailPublisherService>();
            _emailTextGeneratorService = Substitute.For<IEmailTextGeneratorService>();
            _config = Substitute.For<IConfig>();

            _alertService = new AlertService(_nyssContext, _emailPublisherService, _emailTextGeneratorService, _config);

            _alerts = TestData.GetAlerts();
            var alertsDbSet = _alerts.AsQueryable().BuildMockDbSet();
            _nyssContext.Alerts.Returns(alertsDbSet);

            _emailTextGeneratorService.GenerateEscalatedAlertEmail(TestData.ContentLanguageCode)
                .Returns((TestData.EscalationEmailSubject, TestData.EscalationEmailBody));
        }

        [Theory]
        [InlineData(AlertStatus.Closed)]
        [InlineData(AlertStatus.Dismissed)]
        [InlineData(AlertStatus.Escalated)]
        [InlineData(AlertStatus.Rejected)]
        public async Task EscalateAlert_WhenAlertIsNotPending_ShouldReturnError(AlertStatus status)
        {
            _alerts.First().Status = status;

            var result = await _alertService.EscalateAlert(TestData.AlertId);

            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Alert.EscalateAlertWrongStatus);
        }

        [Fact]
        public async Task EscalateAlert_WhenAlertAcceptedReportCountIsLowerThanThreshold_ShouldReturnError()
        {
            _alerts.First().AlertReports = new List<AlertReport>
            {
                new AlertReport { Report = new Report { Status = ReportStatus.Accepted, Village = new Village() } },
                new AlertReport { Report = new Report { Status = ReportStatus.Accepted, Village = new Village() } },
                new AlertReport { Report = new Report { Status = ReportStatus.Rejected, Village = new Village() } }
            };

            _alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 3;

            var result = await _alertService.EscalateAlert(TestData.AlertId);

            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Alert.EscalateAlertThresholdNotReached);
        }

        [Fact]
        public async Task EscalateAlert_WhenAlertMeetsTheCriteria_ShouldSendEmailMessages()
        {
            var emailAddress = "test@test.com";

            _alerts.First().AlertReports = new List<AlertReport>
            {
                new AlertReport { Report = new Report { Status = ReportStatus.Accepted, Village = new Village() } }
            };

            _alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 1;

            _alerts.First().ProjectHealthRisk.Project.AlertRecipients = new List<AlertRecipient>
            {
                new AlertRecipient { EmailAddress = emailAddress }
            };

            await _alertService.EscalateAlert(TestData.AlertId);

            await _emailPublisherService.Received(1)
                .SendEmail((emailAddress, emailAddress), TestData.EscalationEmailSubject, TestData.EscalationEmailBody);
        }

        [Fact]
        public async Task EscalateAlert_WhenAlertMeetsTheCriteria_ChangesAlertStatus()
        {
            _alerts.First().AlertReports = new List<AlertReport>
            {
                new AlertReport { Report = new Report { Status = ReportStatus.Accepted, Village = new Village() } }
            };

            _alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 1;

            var result = await _alertService.EscalateAlert(TestData.AlertId);

            _alerts.First().Status.ShouldBe(AlertStatus.Escalated);
            await _nyssContext.Received(1).SaveChangesAsync();
            result.IsSuccess.ShouldBeTrue();
        }

        [Theory]
        [InlineData(AlertStatus.Closed)]
        [InlineData(AlertStatus.Dismissed)]
        [InlineData(AlertStatus.Escalated)]
        [InlineData(AlertStatus.Rejected)]
        public async Task DismissAlert_WhenAlertIsNotPending_ShouldReturnError(AlertStatus status)
        {
            _alerts.First().Status = status;

            var result = await _alertService.DismissAlert(TestData.AlertId);

            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Alert.DismissAlertWrongStatus);
        }

        [Fact]
        public async Task DismissAlert_WhenAlertAcceptedReportAndPendingReportCountIsGreaterOrEqualToThreshold_ShouldReturnError()
        {
            _alerts.First().AlertReports = new List<AlertReport>
            {
                new AlertReport { Report = new Report { Status = ReportStatus.Accepted, Village = new Village() } },
                new AlertReport { Report = new Report { Status = ReportStatus.Accepted, Village = new Village() } },
                new AlertReport { Report = new Report { Status = ReportStatus.Pending, Village = new Village() } },
                new AlertReport { Report = new Report { Status = ReportStatus.Rejected, Village = new Village() } },
                new AlertReport { Report = new Report { Status = ReportStatus.Rejected, Village = new Village() } },
                new AlertReport { Report = new Report { Status = ReportStatus.Rejected, Village = new Village() } },
                new AlertReport { Report = new Report { Status = ReportStatus.Rejected, Village = new Village() } }
            };

            _alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 3;

            var result = await _alertService.DismissAlert(TestData.AlertId);

            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Alert.DismissAlertPossibleEscalation);
        }

        [Fact]
        public async Task DismissAlert_WhenAlertMeetsTheCriteria_ChangesAlertStatus()
        {
            _alerts.First().Status = AlertStatus.Pending;

            _alerts.First().AlertReports = new List<AlertReport>
            {
                new AlertReport { Report = new Report { Status = ReportStatus.Rejected, Village = new Village() } }
            };

            _alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 1;

            var result = await _alertService.DismissAlert(TestData.AlertId);

            _alerts.First().Status.ShouldBe(AlertStatus.Dismissed);
            await _nyssContext.Received(1).SaveChangesAsync();
            result.IsSuccess.ShouldBeTrue();
        }

        [Theory]
        [InlineData(AlertStatus.Closed)]
        [InlineData(AlertStatus.Rejected)]
        [InlineData(AlertStatus.Dismissed)]
        [InlineData(AlertStatus.Pending)]
        public async Task CloseAlert_WhenAlertIsNotPending_ShouldReturnError(AlertStatus status)
        {
            _alerts.First().Status = status;

            var result = await _alertService.CloseAlert(TestData.AlertId, "");

            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Alert.CloseAlertWrongStatus);
        }

        [Fact]
        public async Task CloseAlert_WhenAlertMeetsTheCriteria_ChangesAlertStatus()
        {
            var comments = "abc";

            _alerts.First().Status = AlertStatus.Escalated;

            var result = await _alertService.CloseAlert(TestData.AlertId, comments);

            _alerts.First().Status.ShouldBe(AlertStatus.Closed);
            _alerts.First().Comments.ShouldBe(comments);
            await _nyssContext.Received(1).SaveChangesAsync();
            result.IsSuccess.ShouldBeTrue();
        }

        private static class TestData
        {
            public const int AlertId = 1;
            public const int ReportId = 23;
            public const int ContentLanguageId = 4;
            public const string ContentLanguageCode = "en";
            public const string EscalationEmailSubject = "subject";
            public const string EscalationEmailBody = "body";

            public static List<Alert> GetAlerts() =>
                new List<Alert>
                {
                    new Alert
                    {
                        Id = AlertId,
                        AlertReports = new List<AlertReport>
                        {
                            new AlertReport
                            {
                                Report = new Report
                                {
                                    Id = ReportId,
                                    Status = ReportStatus.Accepted,
                                    Village = new Village()
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
                                TimeZone = "Dateline Standard Time",
                                AlertRecipients = new List<AlertRecipient>
                                {
                                    new AlertRecipient
                                    {
                                        EmailAddress = "aaa@aaa.com"
                                    }
                                },
                                NationalSociety = new NationalSociety
                                {
                                    ContentLanguage = new ContentLanguage
                                    {
                                        Id = ContentLanguageId,
                                        LanguageCode = ContentLanguageCode
                                    }
                                }
                            }
                        }
                    }
                };
        }
    }
}
