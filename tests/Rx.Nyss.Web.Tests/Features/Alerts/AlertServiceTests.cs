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
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Alerts
{
    public class AlertServiceTests
    {
        private readonly INyssContext _nyssContext;
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly ISmsTextGeneratorService _smsTextGeneratorService;
        private readonly AlertService _alertService;
        private readonly List<Alert> _alerts;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly DateTime _now = DateTime.UtcNow;
        private readonly User _currentUser = new GlobalCoordinatorUser();

        public AlertServiceTests()
        {
            _nyssContext = Substitute.For<INyssContext>();
            _emailPublisherService = Substitute.For<IEmailPublisherService>();
            var emailTextGeneratorService = Substitute.For<IEmailTextGeneratorService>();
            _smsTextGeneratorService = Substitute.For<ISmsTextGeneratorService>();
            var config = Substitute.For<IConfig>();
            var loggerAdapter = Substitute.For<ILoggerAdapter>();

            _dateTimeProvider = Substitute.For<IDateTimeProvider>();
            _authorizationService = Substitute.For<IAuthorizationService>();
            _alertService = new AlertService(_nyssContext, _emailPublisherService, emailTextGeneratorService, config, _smsTextGeneratorService, loggerAdapter, _dateTimeProvider, _authorizationService);

            _alerts = TestData.GetAlerts();
            var alertsDbSet = _alerts.AsQueryable().BuildMockDbSet();
            _nyssContext.Alerts.Returns(alertsDbSet);

            var gatewaySettings = TestData.GetGatewaySettings();
            var gatewaySettingsDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContext.GatewaySettings.Returns(gatewaySettingsDbSet);

            emailTextGeneratorService.GenerateEscalatedAlertEmail(TestData.ContentLanguageCode)
                .Returns((TestData.EscalationEmailSubject, TestData.EscalationEmailBody));

            _dateTimeProvider.UtcNow.Returns(_now);
            _authorizationService.GetCurrentUser().Returns(_currentUser);
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
            result.Message.Key.ShouldBe(ResultKey.Alert.EscalateAlert.WrongStatus);
        }

        [Fact]
        public async Task EscalateAlert_WhenAlertAcceptedReportCountIsLowerThanThreshold_ShouldReturnError()
        {
            _alerts.First().AlertReports = new List<AlertReport>
            {
                new AlertReport { Report = new Report {
                    Status = ReportStatus.Accepted,
                    Village = new Village(),
                    RawReport = new RawReport { ApiKey = TestData.ApiKey } } },
                new AlertReport {
                    Report = new Report {
                    Status = ReportStatus.Accepted,
                    Village = new Village(),
                    RawReport = new RawReport { ApiKey = TestData.ApiKey } } },
                new AlertReport { Report = new Report {
                    Status = ReportStatus.Rejected,
                    Village = new Village(),
                    RawReport = new RawReport { ApiKey = TestData.ApiKey } } }
            };

            _alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 3;

            var result = await _alertService.EscalateAlert(TestData.AlertId);

            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Alert.EscalateAlert.ThresholdNotReached);
        }

        [Fact]
        public async Task EscalateAlert_WhenAlertMeetsTheCriteria_ShouldSendEmailAndSmsMessages()
        {
            var emailAddress = "test@test.com";
            var phonenumber = "+1234578";
            var smsText = "hey";

            _smsTextGeneratorService.GenerateEscalatedAlertSms(TestData.ContentLanguageCode).Returns(smsText);

            _alerts.First().AlertReports = new List<AlertReport>
            {
                new AlertReport { Report = new Report {
                    Status = ReportStatus.Accepted,
                    Village = new Village(),
                    RawReport = new RawReport {
                        ApiKey = TestData.ApiKey
                    } } }
            };

            _alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 1;
            _alerts.First().ProjectHealthRisk.Project.EmailAlertRecipients = new List<EmailAlertRecipient>
            {
                new EmailAlertRecipient { EmailAddress = emailAddress }
            };
            _alerts.First().ProjectHealthRisk.Project.SmsAlertRecipients = new List<SmsAlertRecipient>
            {
                new SmsAlertRecipient{ PhoneNumber= phonenumber }
            };


            await _alertService.EscalateAlert(TestData.AlertId);

            await _emailPublisherService.ReceivedWithAnyArgs(2).SendEmail((null, null), null, null);

            await _emailPublisherService.Received(1)
                .SendEmail((emailAddress, emailAddress), TestData.EscalationEmailSubject, TestData.EscalationEmailBody);

            await _emailPublisherService.Received(1)
                .SendEmail((TestData.GatewayEmail, TestData.GatewayEmail), phonenumber, smsText, true);
        }
        [Fact]
        public async Task EscalateAlert_WhenLastReportGatewayNotExists_ShouldSendSmsThroughFirstInSociety()
        {
            var phonenumber = "+1234578";
            var smsText = "hey";
            _smsTextGeneratorService.GenerateEscalatedAlertSms(TestData.ContentLanguageCode).Returns(smsText);

            _alerts.First().AlertReports = new List<AlertReport>
            {
                new AlertReport { Report = new Report {
                    Status = ReportStatus.Accepted,
                    Village = new Village(),
                    RawReport = new RawReport {
                        ApiKey = "Some_missing_key"
                    } } }
            };

            _alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 1;
            _alerts.First().ProjectHealthRisk.Project.SmsAlertRecipients = new List<SmsAlertRecipient>
            {
                new SmsAlertRecipient{ PhoneNumber= phonenumber }
            };
            
            await _alertService.EscalateAlert(TestData.AlertId);

            await _emailPublisherService.Received(1)
                .SendEmail((TestData.GatewayEmail, TestData.GatewayEmail), phonenumber, smsText, true);
        }

        [Fact]
        public async Task EscalateAlert_WhenAlertMeetsTheCriteria_ChangesAlertStatus()
        {
            _alerts.First().AlertReports = new List<AlertReport>
            {
                new AlertReport {
                    Report = new Report {
                        Status = ReportStatus.Accepted,
                        Village = new Village(),
                        RawReport = new RawReport {
                            ApiKey = TestData.ApiKey
                        }
                    }
                }
            };

            _alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 1;

            var result = await _alertService.EscalateAlert(TestData.AlertId);

            _alerts.First().Status.ShouldBe(AlertStatus.Escalated);
            await _nyssContext.Received(1).SaveChangesAsync();
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task DismissAlert_WhenAlertIsClosed_ShouldReturnError()
        {
            _alerts.First().Status = AlertStatus.Closed;

            var result = await _alertService.DismissAlert(TestData.AlertId);

            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Alert.DismissAlert.WrongStatus);
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
            result.Message.Key.ShouldBe(ResultKey.Alert.DismissAlert.PossibleEscalation);
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
            result.Message.Key.ShouldBe(ResultKey.Alert.CloseAlert.WrongStatus);
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
            public const int NationalSocietyId = 5;
            public const string ContentLanguageCode = "en";
            public const string EscalationEmailSubject = "subject";
            public const string EscalationEmailBody = "body";
            public const string ApiKey = "123";
            public const string GatewayEmail = "gw1@example.com";

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
                                    Village = new Village(),
                                    RawReport = new RawReport
                                    {
                                        ApiKey = ApiKey
                                    }
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
                                EmailAlertRecipients = new List<EmailAlertRecipient>
                                {
                                    new EmailAlertRecipient
                                    {
                                        EmailAddress = "aaa@aaa.com"
                                    }
                                },
                                SmsAlertRecipients = new List<SmsAlertRecipient>
                                {
                                    new SmsAlertRecipient
                                    {
                                        PhoneNumber = "+12345678"
                                    }
                                },
                                NationalSociety = new NationalSociety
                                {
                                    Id = NationalSocietyId,
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

            public static List<GatewaySetting> GetGatewaySettings() =>
                new List<GatewaySetting>
                {
                    new GatewaySetting
                    {
                        ApiKey = ApiKey,
                        GatewayType = GatewayType.SmsEagle,
                        EmailAddress = GatewayEmail,
                        NationalSocietyId = NationalSocietyId
                    }
                };
        }
    }
}
