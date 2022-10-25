using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Alerts.Commands;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.EidsrService;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Alerts.Commands;

public class EscalateTests : AlertFeatureBase
{
    private readonly IEidsrService _eidsrService;
    private readonly EscalateCommand.Handler _handler;

    public EscalateTests()
    {
        _eidsrService = Substitute.For<IEidsrService>();

        var nationalSociety = new List<NationalSociety> { new NationalSociety { Id = 1 } };
        var nationalSocietyDbSet = nationalSociety.AsQueryable().BuildMockDbSet();
        NyssContext.NationalSocieties.Returns(nationalSocietyDbSet);

        _handler = new EscalateCommand.Handler(
            NyssContext,
            AlertService,
            AuthorizationService,
            DateTimeProvider,
            EmailPublisherService,
            EmailTextGeneratorService,
            SmsPublisherService,
            SmsTextGeneratorService,
            LoggerAdapter,
            Config,
            _eidsrService
        );
    }

    [Theory]
    [InlineData(AlertStatus.Closed)]
    [InlineData(AlertStatus.Dismissed)]
    [InlineData(AlertStatus.Escalated)]
    public async Task EscalateAlert_WhenAlertIsNotPending_ShouldReturnError(AlertStatus status)
    {
        Alerts.First().Status = status;

        var result = await _handler.Handle(new EscalateCommand(TestData.AlertId, false), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Message.Key.ShouldBe(ResultKey.Alert.EscalateAlert.WrongStatus);
    }

    [Fact]
    public async Task EscalateAlert_WhenAlertAcceptedReportCountIsLowerThanThreshold_ShouldReturnError()
    {
        Alerts.First().Status = AlertStatus.Open;

        Alerts.First().AlertReports = new List<AlertReport>
        {
            new AlertReport
            {
                Report = new Report
                {
                    Status = ReportStatus.Accepted,
                    RawReport = new RawReport
                    {
                        ApiKey = TestData.ApiKey,
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
            },
            new AlertReport
            {
                Report = new Report
                {
                    Status = ReportStatus.Accepted,
                    RawReport = new RawReport
                    {
                        ApiKey = TestData.ApiKey,
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
            },
            new AlertReport
            {
                Report = new Report
                {
                    Status = ReportStatus.Rejected,
                    RawReport = new RawReport
                    {
                        ApiKey = TestData.ApiKey,
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
        };

        Alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 3;

        var result = await _handler.Handle(new EscalateCommand(TestData.AlertId, false), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Message.Key.ShouldBe(ResultKey.Alert.EscalateAlert.ThresholdNotReached);
    }

    [Fact]
    public async Task EscalateAlert_WhenAlertMeetsTheCriteria_ShouldSendEmailAndSmsMessages()
    {
        var smsText = "hey";

        SmsTextGeneratorService.GenerateEscalatedAlertSms(TestData.ContentLanguageCode).Returns(smsText);

        var alert = Alerts.First();
        alert.Status = AlertStatus.Open;
        alert.ProjectHealthRisk.Id = 1;
        alert.ProjectHealthRisk.Project.Id = 1;
        alert.AlertReports = new List<AlertReport>
        {
            new AlertReport
            {
                Report = new Report
                {
                    Status = ReportStatus.Accepted,
                    RawReport = new RawReport
                    {
                        ApiKey = TestData.ApiKey,
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
                    DataCollector = new DataCollector
                    {
                        Supervisor = new SupervisorUser
                        {
                            Id = 1,
                            UserNationalSocieties = new List<UserNationalSociety> { new UserNationalSociety { OrganizationId = 1 } }
                        }
                    }
                }
            }
        };

        alert.ProjectHealthRisk.AlertRule.CountThreshold = 1;

        var result = await _handler.Handle(new EscalateCommand(TestData.AlertId, true), CancellationToken.None);

        await EmailPublisherService.ReceivedWithAnyArgs(2).SendEmail((null, null), null, null);

        await EmailPublisherService.Received(1)
            .SendEmail(("test@test.com", "test@test.com"), TestData.EscalationEmailSubject, TestData.EscalationEmailBody);

        await EmailPublisherService.Received(1)
            .SendEmail((TestData.GatewayEmail, TestData.GatewayEmail), "+1234578", smsText, true);
    }

    [Fact]
    public async Task EscalateAlert_WhenGatewayIsIoTHubDevice_ShouldSendSmsThroughIotHub()
    {
        var emailAddress = "test@test.com";
        var phonenumber = "+1234578";
        var smsText = "hey";
        AlertNotificationRecipients.Add(new AlertNotificationRecipient
        {
            ProjectId = 1,
            OrganizationId = 1,
            Email = emailAddress,
            PhoneNumber = phonenumber,
            SupervisorAlertRecipients = new List<SupervisorUserAlertRecipient>(),
            HeadSupervisorUserAlertRecipients = new List<HeadSupervisorUserAlertRecipient>(),
            ProjectHealthRiskAlertRecipients = new List<ProjectHealthRiskAlertRecipient>()
        });
        SmsTextGeneratorService.GenerateEscalatedAlertSms(TestData.ContentLanguageCode).Returns(smsText);
        GatewaySettings.First().IotHubDeviceName = "TestDevice";

        var alert = Alerts.First();
        alert.Status = AlertStatus.Open;
        alert.ProjectHealthRisk.Id = 1;
        alert.ProjectHealthRisk.Project.Id = 1;
        alert.AlertReports = new List<AlertReport>
        {
            new AlertReport
            {
                Report = new Report
                {
                    Status = ReportStatus.Accepted,
                    RawReport = new RawReport
                    {
                        ApiKey = TestData.ApiKey,
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
                    DataCollector = new DataCollector
                    {
                        Supervisor = new SupervisorUser
                        {
                            Id = 1,
                            UserNationalSocieties = new List<UserNationalSociety> { new UserNationalSociety { OrganizationId = 1 } }
                        }
                    }
                }
            }
        };

        alert.ProjectHealthRisk.AlertRule.CountThreshold = 1;

        var result = await _handler.Handle(new EscalateCommand(TestData.AlertId, true), CancellationToken.None);

        await EmailPublisherService.ReceivedWithAnyArgs(1).SendEmail((null, null), null, null);

        await EmailPublisherService.Received(1)
            .SendEmail((emailAddress, emailAddress), TestData.EscalationEmailSubject, TestData.EscalationEmailBody);

        await SmsPublisherService.Received(1)
            .SendSms("TestDevice", Arg.Is<List<SendSmsRecipient>>(x => x.Count == 1 && x.First().PhoneNumber == phonenumber), smsText);
    }

    [Fact]
    public async Task EscalateAlert_WhenLastReportGatewayNotExists_ShouldSendSmsThroughFirstInSociety()
    {
        // Arrange
        string phonenumber = "+1234578";
        string smsText = "hey";
        ConfigureAlertValidNotifications(phonenumber, smsText);

        // Act
        var result = await _handler.Handle(new EscalateCommand(TestData.AlertId, true), CancellationToken.None);

        // Assert
        await EmailPublisherService.Received(1)
            .SendEmail((TestData.GatewayEmail, TestData.GatewayEmail), phonenumber, smsText, true);
    }

    [Fact]
    public async Task EscalateAlert_WhenAlertMeetsTheCriteria_ChangesAlertStatus()
    {
        // Arrange
        Alerts.First().Status = AlertStatus.Open;

        Alerts.First().AlertReports = new List<AlertReport>
        {
            new AlertReport
            {
                Report = new Report
                {
                    Status = ReportStatus.Accepted,
                    RawReport = new RawReport
                    {
                        ApiKey = TestData.ApiKey,
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
        };

        Alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 1;

        // Act
        var result = await _handler.Handle(new EscalateCommand(TestData.AlertId, false), CancellationToken.None);

        //Assert
        Alerts.First().Status.ShouldBe(AlertStatus.Escalated);
        Alerts.First().EscalatedAt.ShouldBe(Now);
        Alerts.First().EscalatedBy.ShouldBe(CurrentUser);
        await NyssContext.Received(1).SaveChangesAsync();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task EscalateAlert_WhenSendNotificationIsFalse_ShouldNotSendNotification()
    {
        // Arrange
        ConfigureAlertValidNotifications();

        // Act
        var result = await _handler.Handle(new EscalateCommand(TestData.AlertId, false), CancellationToken.None);

        // Assert
        await SmsPublisherService.Received(0).SendSms(Arg.Any<string>(), Arg.Any<List<SendSmsRecipient>>(), Arg.Any<string>());
        await EmailPublisherService.Received(0).SendEmail(Arg.Any<(string, string)>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task EscalateAlert_WhenEidsrReportFails_ShouldSucceedWithError()
    {
        Alerts.First().Status = AlertStatus.Open;

        // Arrange
        _eidsrService.SendReportsToEidsr(Arg.Any<List<int>>())
            .Throws(new ResultException("Some connection error or queue doesn't exists"));

        // Act
        var result = await _handler.Handle(new EscalateCommand(TestData.AlertId, false), CancellationToken.None);

        // Assert
        var valueResult = (Result<string>)result;
        valueResult.Value.ShouldBe(ResultKey.Alert.EscalateAlert.EidsrReportFailed);
    }

    [Fact]
    public async Task EscalateAlert_WhenSubprocessesFail_ShouldSucceedWithFirstError()
    {
        // Arrange
        ConfigureAlertValidNotifications();

        _eidsrService.SendReportsToEidsr(Arg.Any<List<int>>())
            .Throws(new ResultException("Some connection error or queue doesn't exists"));

        EmailTextGeneratorService.GenerateEscalatedAlertEmail(Arg.Any<string>())
            .Throws(new Exception("Random null reference exception"));

        // Act
        var result = await _handler.Handle(new EscalateCommand(TestData.AlertId, true), CancellationToken.None);

        // Assert
        var valueResult = (Result<string>)result;

        valueResult.Value.ShouldBe(ResultKey.Alert.EscalateAlert.EidsrReportFailed);
    }

    [Fact]
    public async Task EscalateAlert_WhenEmailTextGeneratorServiceFails_ShouldSucceedWithError()
    {
        // Arrange
        ConfigureAlertValidNotifications();

        EmailTextGeneratorService.GenerateEscalatedAlertEmail(Arg.Any<string>())
            .Throws(new Exception("Random null reference exception"));

        // Act
        var result = await _handler.Handle(new EscalateCommand(TestData.AlertId, true), CancellationToken.None);

        // Assert
        var valueResult = (Result<string>)result;

        valueResult.Value.ShouldBe(ResultKey.Alert.EscalateAlert.EmailNotificationFailed);
    }

    private void ConfigureAlertValidNotifications(string phonenumber = "+1234578", string smsText = "hey")
    {
        SmsTextGeneratorService.GenerateEscalatedAlertSms(TestData.ContentLanguageCode).Returns(smsText);

        var alert = Alerts.First();
        alert.Status = AlertStatus.Open;
        alert.ProjectHealthRisk.Id = 1;
        alert.ProjectHealthRisk.Project.Id = 1;

        alert.AlertReports = new List<AlertReport>
        {
            new AlertReport
            {
                Report = new Report
                {
                    Status = ReportStatus.Accepted,
                    RawReport = new RawReport
                    {
                        ApiKey = "Some_missing_key",
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
                    DataCollector = new DataCollector
                    {
                        Supervisor = new SupervisorUser
                        {
                            Id = 1,
                            UserNationalSocieties = new List<UserNationalSociety> { new UserNationalSociety { OrganizationId = 1 } }
                        }
                    }
                }
            }
        };

        alert.ProjectHealthRisk.AlertRule.CountThreshold = 1;
        alert.ProjectHealthRisk.Project.AlertNotificationRecipients = new List<AlertNotificationRecipient> { new AlertNotificationRecipient { PhoneNumber = phonenumber } };
    }
}
