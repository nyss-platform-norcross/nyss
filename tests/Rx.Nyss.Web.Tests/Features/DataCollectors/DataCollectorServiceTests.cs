using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MockQueryable.NSubstitute;
using NetTopologySuite.Geometries;
using NSubstitute;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Services.Geolocation;
using Shouldly;
using Xunit;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Tests.Features.DataCollectors
{
    public class DataCollectorServiceTests
    {
        private const int DataCollectorWithoutReportsId = 1;
        private const int DataCollectorWithReportsId = 2;
        private const string DataCollectorPhoneNumber1 = "+4712345678";
        private const string DataCollectorPhoneNumber2 = "+4712345679";
        private const string DataCollectorPhoneNumber3 = "+4712345680";
        private const string DataCollectorName1 = "simon";
        private const string DataCollectorName2 = "garfunkel";
        private const int ProjectId = 1;
        private const int SupervisorId = 1;
        private const string SupervisorEmail = "supervisor@example.com";
        private const int NationalSocietyId = 1;
        private const int NationalSocietyWithoutIotDeviceId = 2;
        private const string Village = "Layuna";
        private const int RegionId = 1;
        private const int DistrictId = 1;
        private const int VillageId = 1;
        private readonly INyssContext _nyssContextMock;
        private readonly IDataCollectorService _dataCollectorService;
        private readonly IDataCollectorPerformanceService _dataCollectorPerformanceService;
        private readonly IEmailToSMSService _emailToSMSService;
        private readonly ISmsPublisherService _smsPublisherService;
        private List<NationalSociety> _nationalSocieties;

        public DataCollectorServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            var config = Substitute.For<INyssWebConfig>();
            var nationalSocietyStructureService = Substitute.For<INationalSocietyStructureService>();
            var geolocationService = Substitute.For<IGeolocationService>();
            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            httpContextAccessorMock.HttpContext.User.Identity.Name.Returns(SupervisorEmail);
            var authorizationService = Substitute.For<IAuthorizationService>();
            _emailToSMSService = Substitute.For<IEmailToSMSService>();
            _smsPublisherService = Substitute.For<ISmsPublisherService>();
            var smsTextGeneratorService = Substitute.For<ISmsTextGeneratorService>();
            smsTextGeneratorService.GenerateReplaceSupervisorSms("en").Returns("Test");
            config.PaginationRowsPerPage.Returns(5);

            dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
            _dataCollectorService = new DataCollectorService(
                _nyssContextMock,
                config,
                nationalSocietyStructureService,
                geolocationService,
                dateTimeProvider,
                authorizationService,
                _emailToSMSService,
                _smsPublisherService,
                smsTextGeneratorService);

            _dataCollectorPerformanceService = new DataCollectorPerformanceService(config, dateTimeProvider, _dataCollectorService);

            // Arrange
            _nationalSocieties = new List<NationalSociety>
            {
                new NationalSociety
                {
                    Id = NationalSocietyId,
                    ContentLanguage = new ContentLanguage
                    {
                        Id = 1,
                        LanguageCode = "en"
                    }
                },
                new NationalSociety
                {
                    Id = NationalSocietyWithoutIotDeviceId,
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
                    NationalSociety = _nationalSocieties[0],
                    IotHubDeviceName = "iot"
                },
                new GatewaySetting
                {
                    NationalSociety = _nationalSocieties[1],
                    EmailAddress = "test@example.com"
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
                    NationalSociety = _nationalSocieties[0],
                    User = users[0],
                    UserId = SupervisorId,
                    NationalSocietyId = NationalSocietyId,
                    OrganizationId = 1,
                    Organization = new Organization()
                },
                new UserNationalSociety
                {
                    NationalSociety = _nationalSocieties[1],
                    User = users[1],
                    UserId = 2,
                    NationalSocietyId = NationalSocietyWithoutIotDeviceId,
                    OrganizationId = 1,
                    Organization = new Organization()
                }
            };

            _nationalSocieties[0].NationalSocietyUsers = usersNationalSocieties;
            users[0].UserNationalSocieties = new List<UserNationalSociety> { usersNationalSocieties[0] };
            users[1].UserNationalSocieties = new List<UserNationalSociety> { usersNationalSocieties[1] };

            var projects = new List<Project>
            {
                new Project
                {
                    Id = ProjectId,
                    NationalSociety = _nationalSocieties[0],
                    NationalSocietyId = NationalSocietyId
                }
            };
            var supervisorUserProjects = new List<SupervisorUserProject>
            {
                new SupervisorUserProject
                {
                    SupervisorUserId = SupervisorId,
                    SupervisorUser = (SupervisorUser)users[0],
                    ProjectId = ProjectId,
                    Project = projects[0]
                }
            };
            var regions = new List<Region>
            {
                new Region
                {
                    Id = RegionId,
                    NationalSociety = _nationalSocieties[0],
                    Name = "Layuna"
                }
            };
            var districts = new List<District>
            {
                new District
                {
                    Id = DistrictId,
                    Region = regions[0],
                    Name = "Layuna"
                }
            };
            var villages = new List<Village>
            {
                new Village
                {
                    Id = VillageId,
                    District = districts[0],
                    Name = Village
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
                    Village = villages[0],
                    Supervisor = (SupervisorUser)users[0],
                    AdditionalPhoneNumber = "",
                    BirthGroupDecade = 1,
                    DataCollectorType = DataCollectorType.Human,
                    DisplayName = "",
                    Location = new Point(0, 0),
                    Name = DataCollectorName1,
                    Sex = Sex.Male
                },
                new DataCollector
                {
                    Id = DataCollectorWithReportsId,
                    PhoneNumber = DataCollectorPhoneNumber2,
                    Project = projects[0],
                    Village = villages[0],
                    Supervisor = (SupervisorUser)users[0],
                    AdditionalPhoneNumber = "",
                    BirthGroupDecade = 1,
                    DataCollectorType = DataCollectorType.Human,
                    DisplayName = "",
                    Location = new Point(0, 0),
                    Name = DataCollectorName2,
                    Sex = Sex.Female
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

            var nationalSocietyMockDbSet = _nationalSocieties.AsQueryable().BuildMockDbSet();
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

            var rawReportsDbSet = rawReports.AsQueryable().BuildMockDbSet();

            _nyssContextMock.NationalSocieties.Returns(nationalSocietyMockDbSet);
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.Users.Returns(usersMockDbSet);
            _nyssContextMock.UserNationalSocieties.Returns(userNationalSocietiesMockDbSet);
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            _nyssContextMock.SupervisorUserProjects.Returns(supervisorUserProjectsMockDbSet);
            _nyssContextMock.Regions.Returns(regionsMockDbSet);
            _nyssContextMock.Districts.Returns(districtsMockDbSet);
            _nyssContextMock.Villages.Returns(villagesMockDbSet);
            _nyssContextMock.DataCollectors.Returns(dataCollectorsMockDbSet);
            _nyssContextMock.Zones.Returns(zonesMockDbSet);
            _nyssContextMock.DataCollectors.Returns(dataCollectorsDbSet);
            _nyssContextMock.RawReports.Returns(rawReportsDbSet);

            _nyssContextMock.DataCollectors.FindAsync(DataCollectorWithoutReportsId).Returns(dataCollectors[0]);
            _nyssContextMock.DataCollectors.FindAsync(2).Returns((DataCollector)null);

            nationalSocietyStructureService.ListRegions(NationalSocietyId).Returns(Success(new List<RegionResponseDto>()));
            nationalSocietyStructureService.ListDistricts(DistrictId).Returns(Success(new List<DistrictResponseDto>()));
            nationalSocietyStructureService.ListVillages(VillageId).Returns(Success(new List<VillageResponseDto>()));
            nationalSocietyStructureService.ListZones(Arg.Any<int>()).Returns(Success(new List<ZoneResponseDto>()));

            authorizationService.GetCurrentUser().Returns(Task.FromResult((User)new AdministratorUser()));
        }

        [Theory]
        [InlineData(DataCollectorType.Human, "Human")]
        [InlineData(DataCollectorType.CollectionPoint, null)]
        public async Task CreateDataCollector_WhenSuccess_ShouldReturnSuccess(DataCollectorType type, string displayName)
        {
            // Arrange
            var dataCollector = new CreateDataCollectorRequestDto
            {
                DataCollectorType = type,
                PhoneNumber = "+4712344567",
                DisplayName = displayName,
                SupervisorId = SupervisorId,
                VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                Latitude = 15,
                Longitude = 45
            };

            // Act
            await _dataCollectorService.Create(ProjectId, dataCollector);

            // Assert
            await _nyssContextMock.Received(1).AddAsync(Arg.Any<DataCollector>());
        }

        [Fact]
        public void EditDataCollector_WhenDataCollectorDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var dataCollector = new EditDataCollectorRequestDto
            {
                Id = 3,
                PhoneNumber = DataCollectorPhoneNumber3,
                SupervisorId = SupervisorId,
                VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                Latitude = 15,
                Longitude = 45
            };

            Should.ThrowAsync<Exception>(() => _dataCollectorService.Edit(dataCollector));
        }

        [Theory]
        [InlineData(DataCollectorType.Human, "Human")]
        [InlineData(DataCollectorType.CollectionPoint, null)]
        public async Task EditDataCollector_WhenSuccessful_ShouldReturnSuccess(DataCollectorType type, string displayName)
        {
            // Arrange
            var dataCollector = new EditDataCollectorRequestDto
            {
                DataCollectorType = type,
                Id = DataCollectorWithoutReportsId,
                DisplayName = displayName,
                PhoneNumber = DataCollectorPhoneNumber1,
                SupervisorId = SupervisorId,
                VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                Latitude = 15,
                Longitude = 45
            };

            // Act
            var result = await _dataCollectorService.Edit(dataCollector);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.DataCollector.EditSuccess);
        }

        [Fact]
        public async Task RemoveDataCollector_WhenDataCollectorDoesNotExist_ShouldReturnError()
        {
            // Act
            var result = await _dataCollectorService.Delete(999);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.DataCollector.DataCollectorNotFound);
        }

        [Fact]
        public async Task RemoveDataCollector_WhenDataCollectorExists_ShouldReturnSuccess()
        {
            // Act
            var result = await _dataCollectorService.Delete(DataCollectorWithoutReportsId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.DataCollector.RemoveSuccess);
        }

        [Fact]
        public async Task GetDataCollector_WhenDataCollectorExists_ShouldReturnSuccess()
        {
            // Act
            var result = await _dataCollectorService.Get(DataCollectorWithoutReportsId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldBe(DataCollectorWithoutReportsId);
        }

        [Fact]
        public void GetDataCollector_WhenDataCollectorDoesNotExist_ShouldThrowException()
        {
            Should.ThrowAsync<Exception>(() => _dataCollectorService.Get(3));
        }

        [Fact]
        public async Task SetTrainingState_WhenDataCollectorExists_ShouldReturnSuccess()
        {
            // Act
            var result = await _dataCollectorService.SetTrainingState(new SetDataCollectorsTrainingStateRequestDto
            {
                DataCollectorIds = new [] { DataCollectorWithoutReportsId },
                InTraining = true
            });

            // Assert
            await _nyssContextMock.Received(1).SaveChangesAsync();
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task ListDataCollector_WhenSuccessful_ShouldReturnSuccess()
        {
            // Act
            var filters = new DataCollectorsFiltersRequestDto
            {
                Area = null,
                Sex = null,
                SupervisorId = null,
                TrainingStatus = null,
                Name = null
            };
            var result = await _dataCollectorService.List(ProjectId, filters);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Data.Count().ShouldBe(2);
            var dataCollector = result.Value.Data.First();
            dataCollector.Id.ShouldBe(DataCollectorWithReportsId);
            dataCollector.DisplayName.ShouldBe("");
            dataCollector.PhoneNumber.ShouldBe(DataCollectorPhoneNumber2);
            dataCollector.Village.ShouldBe(Village);
            dataCollector.District.ShouldBe("Layuna");
            dataCollector.Name.ShouldBe(DataCollectorName2);
            dataCollector.Sex.ShouldBe(Sex.Female);
            dataCollector.Region.ShouldBe("Layuna");

            var secondDataCollector = result.Value.Data.Last();
            secondDataCollector.Id.ShouldBe(DataCollectorWithoutReportsId);
            secondDataCollector.Sex.ShouldBe(Sex.Male);
        }

        [Fact]
        public async Task ListDataCollector_WhenFiltered_ShouldReturnFilteredList()
        {
            // Act
            var filters = new DataCollectorsFiltersRequestDto
            {
                Area = null,
                Sex = SexDto.Male,
                SupervisorId = null,
                TrainingStatus = null,
                Name = null
            };
            var result = await _dataCollectorService.List(ProjectId, filters);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Data.Count().ShouldBe(1);
            var dataCollector = result.Value.Data.First();
            dataCollector.Id.ShouldBe(DataCollectorWithoutReportsId);
            dataCollector.DisplayName.ShouldBe("");
            dataCollector.PhoneNumber.ShouldBe(DataCollectorPhoneNumber1);
            dataCollector.Village.ShouldBe(Village);
            dataCollector.District.ShouldBe("Layuna");
            dataCollector.Name.ShouldBe(DataCollectorName1);
            dataCollector.Sex.ShouldBe(Sex.Male);
            dataCollector.Region.ShouldBe("Layuna");
        }

        [Fact]
        public async Task ListDataCollector_WhenFilteredByName_ShouldReturnFilteredList()
        {
            // Act
            var filters = new DataCollectorsFiltersRequestDto
            {
                Area = null,
                Sex = null,
                SupervisorId = null,
                TrainingStatus = null,
                Name = "simon"
            };
            var result = await _dataCollectorService.List(ProjectId, filters);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Data.Count().ShouldBe(1);
            var dataCollector = result.Value.Data.First();
            dataCollector.Id.ShouldBe(DataCollectorWithoutReportsId);
            dataCollector.DisplayName.ShouldBe("");
            dataCollector.PhoneNumber.ShouldBe(DataCollectorPhoneNumber1);
            dataCollector.Village.ShouldBe(Village);
            dataCollector.District.ShouldBe("Layuna");
            dataCollector.Name.ShouldBe(DataCollectorName1);
            dataCollector.Sex.ShouldBe(Sex.Male);
            dataCollector.Region.ShouldBe("Layuna");
        }

        [Fact(Skip = "EFCore Extension for BatchUpdate is not working with MockDbSet")]
        public async Task RemoveDataCollector_WhenDataCollectorHasReports_ShouldAnonymizeDataCollector()
        {
            //Arrange
            var dataCollector = _nyssContextMock.DataCollectors.Single(x => x.Id == DataCollectorWithReportsId);

            // Act
            var result = await _dataCollectorService.Delete(DataCollectorWithReportsId);

            //Assert
            result.IsSuccess.ShouldBeTrue();
            dataCollector.Name.ShouldBe(Anonymization.Text);
            dataCollector.DisplayName.ShouldBe(Anonymization.Text);
            dataCollector.PhoneNumber.ShouldBe(Anonymization.Text);
            dataCollector.AdditionalPhoneNumber.ShouldBe(Anonymization.Text);
        }

        [Fact(Skip = "EFCore Extension for BatchUpdate is not working with MockDbSet")]
        public async Task RemoveDataCollector_WhenDataCollectorHasReports_AnonymizationUpdateForReportsWasReceived()
        {
            //Arrange
            var dataCollector = _nyssContextMock.DataCollectors.Single(x => x.Id == DataCollectorWithReportsId);
            FormattableString expectedSqlCommand = $"UPDATE Nyss.RawReports SET Sender = {Anonymization.Text} WHERE DataCollectorId = {dataCollector.Id}";

            // Act
            var result = await _dataCollectorService.Delete(DataCollectorWithReportsId);

            //Assert
            result.IsSuccess.ShouldBeTrue();
            await _nyssContextMock.Received().ExecuteSqlInterpolatedAsync(Arg.Is<FormattableString>(arg => arg.ToString() == expectedSqlCommand.ToString()));
        }

        [Theory]
        [MemberData(nameof(GetPerformanceTestData))]
        public async Task GetDataCollectorPerformance_WhenDataCollectorsHaveReported_ShouldReturnCorrectStatus(string phoneNumber, List<RawReport> reports)
        {
            // Arrange
            var rawReportsMockDbSet = reports.AsQueryable().BuildMockDbSet();
            var dataCollectors = new List<DataCollector>
            {
                new DataCollector
                {
                    PhoneNumber = phoneNumber,
                    Village = new Village
                    {
                        Name = "Coronia"
                    },
                    Project = new Project
                    {
                        Id = ProjectId,
                        NationalSociety = _nationalSocieties[0]
                    },
                    RawReports = reports,
                    Supervisor = new SupervisorUser()
                }
            };
            var dataCollectorsMockDbSet = dataCollectors.AsQueryable().BuildMockDbSet();

            _nyssContextMock.RawReports.Returns(rawReportsMockDbSet);
            _nyssContextMock.DataCollectors.Returns(dataCollectorsMockDbSet);

            var dateTimeNow = DateTime.UtcNow;

            // Act
            var result = await _dataCollectorPerformanceService.Performance(ProjectId, new DataCollectorPerformanceFiltersRequestDto
            {
                LastWeek = new PerformanceStatusFilterDto
                {
                    NotReporting = true,
                    ReportingCorrectly = true,
                    ReportingWithErrors = true
                },
                TwoWeeksAgo = new PerformanceStatusFilterDto
                {
                    NotReporting = true,
                    ReportingCorrectly = true,
                    ReportingWithErrors = true
                },
                ThreeWeeksAgo = new PerformanceStatusFilterDto
                {
                    NotReporting = true,
                    ReportingCorrectly = true,
                    ReportingWithErrors = true
                },
                FourWeeksAgo = new PerformanceStatusFilterDto
                {
                    NotReporting = true,
                    ReportingCorrectly = true,
                    ReportingWithErrors = true
                },
                FiveWeeksAgo = new PerformanceStatusFilterDto
                {
                    NotReporting = true,
                    ReportingCorrectly = true,
                    ReportingWithErrors = true
                },
                SixWeeksAgo = new PerformanceStatusFilterDto
                {
                    NotReporting = true,
                    ReportingCorrectly = true,
                    ReportingWithErrors = true
                },
                SevenWeeksAgo = new PerformanceStatusFilterDto
                {
                    NotReporting = true,
                    ReportingCorrectly = true,
                    ReportingWithErrors = true
                },
                EightWeeksAgo = new PerformanceStatusFilterDto
                {
                    NotReporting = true,
                    ReportingCorrectly = true,
                    ReportingWithErrors = true
                }
            });

            // Assert
            result.Value.Performance.Data[0].StatusLastWeek.ShouldBe(DataCollectorStatusFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 0)));
            result.Value.Performance.Data[0].StatusTwoWeeksAgo.ShouldBe(DataCollectorStatusFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 1)));
            result.Value.Performance.Data[0].StatusThreeWeeksAgo.ShouldBe(DataCollectorStatusFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 2)));
            result.Value.Performance.Data[0].StatusFourWeeksAgo.ShouldBe(DataCollectorStatusFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 3)));
            result.Value.Performance.Data[0].StatusFiveWeeksAgo.ShouldBe(DataCollectorStatusFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 4)));
            result.Value.Performance.Data[0].StatusSixWeeksAgo.ShouldBe(DataCollectorStatusFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 5)));
            result.Value.Performance.Data[0].StatusSevenWeeksAgo.ShouldBe(DataCollectorStatusFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 6)));
            result.Value.Performance.Data[0].StatusEightWeeksAgo.ShouldBe(DataCollectorStatusFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 7)));
            result.Value.Completeness.LastWeek.Percentage.ShouldBe(CompletenessPercentageFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 0)));
            result.Value.Completeness.TwoWeeksAgo.Percentage.ShouldBe(CompletenessPercentageFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 1)));
            result.Value.Completeness.ThreeWeeksAgo.Percentage.ShouldBe(CompletenessPercentageFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 2)));
            result.Value.Completeness.FourWeeksAgo.Percentage.ShouldBe(CompletenessPercentageFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 3)));
            result.Value.Completeness.FiveWeeksAgo.Percentage.ShouldBe(CompletenessPercentageFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 4)));
            result.Value.Completeness.SixWeeksAgo.Percentage.ShouldBe(CompletenessPercentageFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 5)));
            result.Value.Completeness.SevenWeeksAgo.Percentage.ShouldBe(CompletenessPercentageFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 6)));
            result.Value.Completeness.EightWeeksAgo.Percentage.ShouldBe(CompletenessPercentageFromReports(reports.Where(r => (int)(dateTimeNow - r.ReceivedAt).TotalDays / 7 == 7)));
        }

        [Fact]
        public async Task ReplaceSupervisor_WhenUsingIotHub_ShouldSendSmsToDataCollectorsThroughIotHub()
        {
            // Arrange
            var replaceSupervisorDto = new ReplaceSupervisorRequestDto
            {
                DataCollectorIds = new List<int> { DataCollectorWithoutReportsId },
                SupervisorId = SupervisorId
            };

            // Act
            var res = await _dataCollectorService.ReplaceSupervisor(replaceSupervisorDto);

            // Assert
            res.IsSuccess.ShouldBe(true);
            await _smsPublisherService.Received().SendSms("iot", Arg.Any<List<string>>(), "Test");
        }

        [Fact]
        public async Task ReplaceSupervisor_WhenUsingEmailToSms_ShouldSendSmsToDataCollectorsThroughEmail()
        {
            // Arrange
            var replaceSupervisorDto = new ReplaceSupervisorRequestDto
            {
                DataCollectorIds = new List<int> { DataCollectorWithoutReportsId },
                SupervisorId = 2
            };

            // Act
            var res = await _dataCollectorService.ReplaceSupervisor(replaceSupervisorDto);

            // Assert
            res.IsSuccess.ShouldBe(true);
            await _emailToSMSService.Received().SendMessage(Arg.Any<GatewaySetting>(), Arg.Any<List<string>>(), "Test");
        }

        public ReportingStatus DataCollectorStatusFromReports(IEnumerable<RawReport> reports)
        {
            return reports.Any()
                ? reports.All(r => r.Report != null) ? ReportingStatus.ReportingCorrectly : ReportingStatus.ReportingWithErrors
                : ReportingStatus.NotReporting;
        }

        public int CompletenessPercentageFromReports(IEnumerable<RawReport> reports) =>
            reports.Any()
                ? 100
                : 0;

        public static IEnumerable<object[]> GetPerformanceTestData()
        {
            yield return new object[]
            {
                DataCollectorPhoneNumber1,
                new List<RawReport>
                {
                    new RawReport
                    {
                        ReceivedAt = DateTime.UtcNow,
                        IsTraining = false,
                        Report = new Report(),
                        ReportId = 1
                    }
                }
            };

            yield return new object[]
            {
                DataCollectorPhoneNumber1,
                new List<RawReport>
                {
                    new RawReport
                    {
                        ReceivedAt = DateTime.UtcNow.AddDays(-8),
                        IsTraining = false,
                        Report = new Report(),
                        ReportId = 2
                    },
                    new RawReport
                    {
                        ReceivedAt = DateTime.UtcNow,
                        IsTraining = false
                    }
                }
            };

            yield return new object[] { DataCollectorPhoneNumber1, new List<RawReport>() };

            yield return new object[]
            {
                DataCollectorPhoneNumber1,
                new List<RawReport>
                {
                    new RawReport
                    {
                        ReceivedAt = DateTime.UtcNow.AddDays(-8),
                        IsTraining = false,
                        Report = new Report(),
                        ReportId = 3
                    },
                    new RawReport
                    {
                        ReceivedAt = DateTime.UtcNow.AddDays(-35),
                        IsTraining = false
                    }
                }
            };
        }
    }
}
