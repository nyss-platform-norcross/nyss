using System;
using System.Collections.Generic;
using System.Linq;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts;
using RX.Nyss.Web.Features.Projects;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Tests.Features.Alerts
{
    public abstract class AlertFeatureBase
    {
        protected INyssContext NyssContext { get; set; }

        protected IEmailPublisherService EmailPublisherService { get; set; }

        protected ISmsTextGeneratorService SmsTextGeneratorService { get; set; }

        protected AlertService AlertService { get; set; }

        protected List<Alert> Alerts { get; set; }

        protected IDateTimeProvider DateTimeProvider { get; set; }

        protected IAuthorizationService AuthorizationService { get; set; }

        protected DateTime Now { get; set; } = DateTime.UtcNow;

        protected User CurrentUser { get; set; } = new GlobalCoordinatorUser();

        protected ISmsPublisherService SmsPublisherService { get; set; }

        protected List<GatewaySetting> GatewaySettings { get; set; }

        protected List<AlertNotificationRecipient> AlertNotificationRecipients { get; set; }

        protected IProjectService ProjectService { get; set; }

        protected IExcelExportService ExcelExportService { get; set; }

        protected IStringsService StringsService { get; set; }

        protected INyssWebConfig Config { get; set; }

        protected IEmailTextGeneratorService EmailTextGeneratorService { get; set; }

        protected ILoggerAdapter LoggerAdapter { get; set; }

        public AlertFeatureBase()
        {
            Config = Substitute.For<INyssWebConfig>();
            Config.PaginationRowsPerPage.Returns(5);

            LoggerAdapter = Substitute.For<ILoggerAdapter>();
            EmailTextGeneratorService = Substitute.For<IEmailTextGeneratorService>();

            NyssContext = Substitute.For<INyssContext>();
            EmailPublisherService = Substitute.For<IEmailPublisherService>();
            SmsTextGeneratorService = Substitute.For<ISmsTextGeneratorService>();

            DateTimeProvider = Substitute.For<IDateTimeProvider>();
            AuthorizationService = Substitute.For<IAuthorizationService>();
            StringsService = Substitute.For<IStringsService>();

            SmsPublisherService = Substitute.For<ISmsPublisherService>();
            ProjectService = Substitute.For<IProjectService>();
            ExcelExportService = Substitute.For<IExcelExportService>();

            AlertService = new AlertService(
                NyssContext,
                AuthorizationService);

            Alerts = TestData.GetAlerts();
            var alertsDbSet = Alerts.AsQueryable().BuildMockDbSet();
            NyssContext.Alerts.Returns(alertsDbSet);


            AlertNotificationRecipients = new List<AlertNotificationRecipient>
            {
                new AlertNotificationRecipient
                {
                    ProjectId = 1,
                    OrganizationId = 1,
                    Email = "test@test.com",
                    PhoneNumber = "+1234578",
                    SupervisorAlertRecipients = new List<SupervisorUserAlertRecipient>(),
                    HeadSupervisorUserAlertRecipients = new List<HeadSupervisorUserAlertRecipient>(),
                    ProjectHealthRiskAlertRecipients = new List<ProjectHealthRiskAlertRecipient>()
                }
            };
            var alertNotificationDbSet = AlertNotificationRecipients.AsQueryable().BuildMockDbSet();
            NyssContext.AlertNotificationRecipients.Returns(alertNotificationDbSet);

            GatewaySettings = TestData.GetGatewaySettings();
            var gatewaySettingsDbSet = GatewaySettings.AsQueryable().BuildMockDbSet();
            NyssContext.GatewaySettings.Returns(gatewaySettingsDbSet);

            var userNationalSociety = new List<UserNationalSociety> { new UserNationalSociety { Organization = new Organization { Id = 1 } } };
            var unsDbSet = userNationalSociety.AsQueryable().BuildMockDbSet();
            NyssContext.UserNationalSocieties.Returns(unsDbSet);

            EmailTextGeneratorService.GenerateEscalatedAlertEmail(TestData.ContentLanguageCode)
                .Returns((TestData.EscalationEmailSubject, TestData.EscalationEmailBody));

            DateTimeProvider.UtcNow.Returns(Now);
            AuthorizationService.GetCurrentUser().Returns(CurrentUser);
            AuthorizationService.GetCurrentUser().Returns(CurrentUser);
        }

        protected static class TestData
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
            public static readonly DateTime AlertCreatedAt = new DateTime(2020, 1, 1);
            public static readonly GlobalCoordinatorUser DefaultUser = new GlobalCoordinatorUser { Name = "DefaultUser" };

            public static List<Alert> GetAlerts() =>
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

            public static List<Alert> GetAlertsForFiltering() =>
                new List<Alert>
                {
                    new Alert
                    {
                        Id = 1,
                        Status = AlertStatus.Escalated,
                        CreatedAt = new DateTime(2020, 1, 1),
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
                                Id = 1,
                                AlertNotificationRecipients = new List<AlertNotificationRecipient>
                                {
                                    new AlertNotificationRecipient
                                    {
                                        Email = "aaaa@example.com",
                                        PhoneNumber = "+423423223"
                                    }
                                },
                                NationalSociety = new NationalSociety
                                {
                                    Id = NationalSocietyId,
                                    ContentLanguage = new ContentLanguage
                                    {
                                        Id = ContentLanguageId,
                                        LanguageCode = ContentLanguageCode
                                    },
                                    NationalSocietyUsers = new List<UserNationalSociety>()
                                }
                            }
                        }
                    },
                    new Alert
                    {
                        Id = 1,
                        Status = AlertStatus.Open,
                        CreatedAt = new DateTime(2020, 1, 2),
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
                                Id = 1,
                                AlertNotificationRecipients = new List<AlertNotificationRecipient>
                                {
                                    new AlertNotificationRecipient
                                    {
                                        Email = "aaaa@example.com",
                                        PhoneNumber = "+423423223"
                                    }
                                },
                                NationalSociety = new NationalSociety
                                {
                                    Id = NationalSocietyId,
                                    ContentLanguage = new ContentLanguage
                                    {
                                        Id = ContentLanguageId,
                                        LanguageCode = ContentLanguageCode
                                    },
                                    NationalSocietyUsers = new List<UserNationalSociety>()
                                }
                            }
                        }
                    },
                    new Alert
                    {
                        Id = 1,
                        Status = AlertStatus.Dismissed,
                        CreatedAt = new DateTime(2020, 1, 3),
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
                                Id = 1,
                                AlertNotificationRecipients = new List<AlertNotificationRecipient>
                                {
                                    new AlertNotificationRecipient
                                    {
                                        Email = "aaaa@example.com",
                                        PhoneNumber = "+423423223"
                                    }
                                },
                                NationalSociety = new NationalSociety
                                {
                                    Id = NationalSocietyId,
                                    ContentLanguage = new ContentLanguage
                                    {
                                        Id = ContentLanguageId,
                                        LanguageCode = ContentLanguageCode
                                    },
                                    NationalSocietyUsers = new List<UserNationalSociety>()
                                }
                            }
                        }
                    },
                    new Alert
                    {
                        Id = 1,
                        Status = AlertStatus.Closed,
                        CreatedAt = new DateTime(2020, 1, 4),
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
                                Id = 1,
                                AlertNotificationRecipients = new List<AlertNotificationRecipient>
                                {
                                    new AlertNotificationRecipient
                                    {
                                        Email = "aaaa@example.com",
                                        PhoneNumber = "+423423223"
                                    }
                                },
                                NationalSociety = new NationalSociety
                                {
                                    Id = NationalSocietyId,
                                    ContentLanguage = new ContentLanguage
                                    {
                                        Id = ContentLanguageId,
                                        LanguageCode = ContentLanguageCode
                                    },
                                    NationalSocietyUsers = new List<UserNationalSociety>()
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
                        NationalSocietyId = NationalSocietyId,
                        Modems = new List<GatewayModem>()
                    }
                };

            public static List<User> GetUsers() =>
                new List<User>
                {
                    new AdministratorUser
                    {
                        EmailAddress = "admin@domain.com",
                        Id = 1
                    }
                };
        }
    }
}
