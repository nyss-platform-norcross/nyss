using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NetTopologySuite.Geometries;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataCollectors.Commands;
using RX.Nyss.Web.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataCollectors.Commands
{
    public class ReplaceSupervisorCommandTests
    {
        private const int DataCollectorWithoutReportsId = 1;

        private const string DataCollectorPhoneNumber1 = "+4712345678";

        private const int DataCollectorWithReportsId = 2;

        private const string DataCollectorPhoneNumber2 = "+4712345679";

        private const string SupervisorEmail = "supervisor@example.com";

        private const string DataCollectorName1 = "simon";

        private const string DataCollectorName2 = "garfunkel";

        private const int SupervisorId = 1;

        private readonly INyssContext _mockNyssContext;

        private readonly ISmsPublisherService _mockSmsPublisherService;

        private readonly ISmsTextGeneratorService _mockSmsTextGeneratorService;

        private readonly IEmailPublisherService _mockEmailPublisherService;

        private readonly IEmailTextGeneratorService _mockEmailTextGeneratorService;

        private readonly ReplaceSupervisorCommand.Handler _handler;

        public ReplaceSupervisorCommandTests()
        {
            _mockNyssContext = Substitute.For<INyssContext>();
            _mockSmsPublisherService = Substitute.For<ISmsPublisherService>();
            _mockEmailPublisherService = Substitute.For<IEmailPublisherService>();
            _mockEmailTextGeneratorService = Substitute.For<IEmailTextGeneratorService>();

            _mockSmsTextGeneratorService = Substitute.For<ISmsTextGeneratorService>();
            _mockSmsTextGeneratorService.GenerateReplaceSupervisorSms("en").Returns("Test");

            SetupContext(_mockNyssContext);

            _handler = new ReplaceSupervisorCommand.Handler(
                _mockNyssContext,
                _mockSmsPublisherService,
                _mockSmsTextGeneratorService,
                _mockEmailPublisherService,
                _mockEmailTextGeneratorService);
        }

        [Fact]
        public async Task WhenUsingIotHub_ShouldSendSmsToDataCollectorsThroughIotHub()
        {
            // Arrange
            var cmd = new ReplaceSupervisorCommand
            {
                DataCollectorIds = new List<int> { DataCollectorWithoutReportsId },
                SupervisorId = SupervisorId
            };

            // Act
            var res = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            res.IsSuccess.ShouldBe(true);

            await _mockSmsPublisherService.Received().SendSms("iot", Arg.Any<List<SendSmsRecipient>>(), "Test", false);
        }

        private static void SetupContext(INyssContext context)
        {
            var nationalSocieties = new List<NationalSociety>
            {
                new NationalSociety
                {
                    Id = 1,
                    ContentLanguage = new ContentLanguage
                    {
                        Id = 1,
                        LanguageCode = "en"
                    }
                },
                new NationalSociety
                {
                    Id = 2,
                    ContentLanguage = new ContentLanguage
                    {
                        Id = 1,
                        LanguageCode = "en"
                    }
                }
            };

            var gatewaySettings = new List<GatewaySetting>
            {
                new GatewaySetting
                {
                    NationalSociety = nationalSocieties[0],
                    IotHubDeviceName = "iot",
                    Modems = new List<GatewayModem>()
                },
                new GatewaySetting
                {
                    NationalSociety = nationalSocieties[1],
                    EmailAddress = "test@example.com",
                    Modems = new List<GatewayModem>()
                }
            };

            var users = new List<User>
            {
                new SupervisorUser
                {
                    Id = SupervisorId,
                    Role = Role.Supervisor,
                    EmailAddress = SupervisorEmail
                },
                new SupervisorUser
                {
                    Id = 2,
                    Role = Role.Supervisor,
                    EmailAddress = SupervisorEmail
                }
            };
            var usersNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety
                {
                    NationalSociety = nationalSocieties[0],
                    User = users[0],
                    UserId = SupervisorId,
                    NationalSocietyId = 1,
                    OrganizationId = 1,
                    Organization = new Organization()
                },
                new UserNationalSociety
                {
                    NationalSociety = nationalSocieties[1],
                    User = users[1],
                    UserId = 2,
                    NationalSocietyId = 2,
                    OrganizationId = 1,
                    Organization = new Organization()
                }
            };

            nationalSocieties[0].NationalSocietyUsers = usersNationalSocieties;
            users[0].UserNationalSocieties = new List<UserNationalSociety> { usersNationalSocieties[0] };
            users[1].UserNationalSocieties = new List<UserNationalSociety> { usersNationalSocieties[1] };

            var projects = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    NationalSociety = nationalSocieties[0],
                    NationalSocietyId = 1
                }
            };
            var supervisorUserProjects = new List<SupervisorUserProject>
            {
                new SupervisorUserProject
                {
                    SupervisorUserId = SupervisorId,
                    SupervisorUser = (SupervisorUser)users[0],
                    ProjectId = 1,
                    Project = projects[0]
                }
            };
            var headSupervisorUserProjects = new List<HeadSupervisorUserProject>();
            var regions = new List<Region>
            {
                new Region
                {
                    Id = 1,
                    NationalSociety = nationalSocieties[0],
                    Name = "Layuna"
                }
            };
            var districts = new List<District>
            {
                new District
                {
                    Id = 1,
                    Region = regions[0],
                    Name = "Layuna"
                }
            };
            var villages = new List<Village>
            {
                new Village
                {
                    Id = 1,
                    District = districts[0],
                    Name = "Layuna"
                }
            };
            var zones = new List<Zone>();
            var dataCollectors = new List<DataCollector>
            {
                new DataCollector
                {
                    Id = DataCollectorWithoutReportsId,
                    PhoneNumber = DataCollectorPhoneNumber1,
                    Project = projects[0],
                    Supervisor = (SupervisorUser)users[0],
                    AdditionalPhoneNumber = "",
                    BirthGroupDecade = 1,
                    DataCollectorType = DataCollectorType.Human,
                    DisplayName = "",
                    Name = DataCollectorName1,
                    Sex = Sex.Male,
                    DataCollectorLocations = new List<DataCollectorLocation>
                    {
                        new DataCollectorLocation
                        {
                            Id = 1,
                            Village = villages[0],
                            Location = new Point(0, 0)
                        }
                    }
                },
                new DataCollector
                {
                    Id = DataCollectorWithReportsId,
                    PhoneNumber = DataCollectorPhoneNumber2,
                    Project = projects[0],
                    Supervisor = (SupervisorUser)users[0],
                    AdditionalPhoneNumber = "",
                    BirthGroupDecade = 1,
                    DataCollectorType = DataCollectorType.Human,
                    DisplayName = "",
                    Name = DataCollectorName2,
                    Sex = Sex.Female,
                    DataCollectorLocations = new List<DataCollectorLocation>
                    {
                        new DataCollectorLocation
                        {
                            Village = villages[0],
                            Location = new Point(0, 0)
                        }
                    }
                }
            };

            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    Id = 1,
                    DataCollector = dataCollectors[1],
                    Sender = "+123456"
                },
                new RawReport
                {
                    Id = 2,
                    DataCollector = dataCollectors[1],
                    Sender = "+123456"
                }
            };
            dataCollectors[0].RawReports = new List<RawReport>();
            dataCollectors[1].RawReports = new List<RawReport>
            {
                rawReports[0],
                rawReports[1]
            };

            var nationalSocietyMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            var usersMockDbSet = users.AsQueryable().BuildMockDbSet();
            var userNationalSocietiesMockDbSet = usersNationalSocieties.AsQueryable().BuildMockDbSet();
            var projectsMockDbSet = projects.AsQueryable().BuildMockDbSet();
            var supervisorUserProjectsMockDbSet = supervisorUserProjects.AsQueryable().BuildMockDbSet();
            var regionsMockDbSet = regions.AsQueryable().BuildMockDbSet();
            var districtsMockDbSet = districts.AsQueryable().BuildMockDbSet();
            var villagesMockDbSet = villages.AsQueryable().BuildMockDbSet();
            var dataCollectorsMockDbSet = dataCollectors.AsQueryable().BuildMockDbSet();
            var zonesMockDbSet = zones.AsQueryable().BuildMockDbSet();
            var dataCollectorsDbSet = dataCollectors.AsQueryable().BuildMockDbSet();
            var headSupervisorUserProjectsDbSet = headSupervisorUserProjects.AsQueryable().BuildMockDbSet();

            var rawReportsDbSet = rawReports.AsQueryable().BuildMockDbSet();

            context.NationalSocieties.Returns(nationalSocietyMockDbSet);
            context.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            context.Users.Returns(usersMockDbSet);
            context.UserNationalSocieties.Returns(userNationalSocietiesMockDbSet);
            context.Projects.Returns(projectsMockDbSet);
            context.SupervisorUserProjects.Returns(supervisorUserProjectsMockDbSet);
            context.Regions.Returns(regionsMockDbSet);
            context.Districts.Returns(districtsMockDbSet);
            context.Villages.Returns(villagesMockDbSet);
            context.DataCollectors.Returns(dataCollectorsMockDbSet);
            context.Zones.Returns(zonesMockDbSet);
            context.DataCollectors.Returns(dataCollectorsDbSet);
            context.RawReports.Returns(rawReportsDbSet);
            context.HeadSupervisorUserProjects.Returns(headSupervisorUserProjectsDbSet);

            context.DataCollectors.FindAsync(DataCollectorWithoutReportsId).Returns(dataCollectors[0]);
            context.DataCollectors.FindAsync(2).Returns((DataCollector)null);
        }
    }
}
