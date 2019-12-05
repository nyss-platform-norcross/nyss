using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MockQueryable.NSubstitute;
using NetTopologySuite.Geometries;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataCollector;
using RX.Nyss.Web.Features.DataCollector.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;
using RX.Nyss.Web.Services.Geolocation;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using Shouldly;
using Xunit;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace Rx.Nyss.Web.Tests.Features.DataCollector
{
    public class DataCollectorServiceTests
    {
        private readonly INyssContext _nyssContextMock;
        private readonly IDataCollectorService _dataCollectorService;

        private const int DataCollectorWithoutReportsId = 1;
        private const int DataCollectorWithReportsId = 2;
        private const string DataCollectorPhoneNumber1 = "+4712345678";
        private const string DataCollectorPhoneNumber2 = "+4712345679";
        private const string DataCollectorPhoneNumber3 = "+4712345680";
        private const int ProjectId = 1;
        private const int SupervisorId = 1;
        private const int NationalSocietyId = 1;
        private const string Village = "Layuna";
        private const int RegionId = 1;
        private const int DistrictId = 1;
        private const int VillageId = 1;

        public DataCollectorServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            var nationalSocietyStructureService = Substitute.For<INationalSocietyStructureService>();
            var geolocationService = Substitute.For<IGeolocationService>();
            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            
            dateTimeProvider.UtcNow.Returns(new DateTime(2019, 1, 1));
            _dataCollectorService = new DataCollectorService(_nyssContextMock, nationalSocietyStructureService, geolocationService, dateTimeProvider);

            // Arrange
            var nationalSocieties = new List<RX.Nyss.Data.Models.NationalSociety>
            {
                new RX.Nyss.Data.Models.NationalSociety { Id = NationalSocietyId }
            };
            var users = new List<User>
            {
                new SupervisorUser { Id = SupervisorId, Role = Role.Supervisor }
            };
            var usersNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety { NationalSociety = nationalSocieties[0], User = users[0], NationalSocietyId = NationalSocietyId }
            };
            var projects = new List<RX.Nyss.Data.Models.Project>
            {
                new RX.Nyss.Data.Models.Project { Id = ProjectId, NationalSociety = nationalSocieties[0] }
            };
            var supervisorUserProjects = new List<SupervisorUserProject>
            {
                new SupervisorUserProject { SupervisorUserId = SupervisorId, SupervisorUser = (SupervisorUser)users[0], ProjectId = ProjectId, Project = projects[0] }
            };
            var regions = new List<Region>
            {
                new Region { Id = RegionId, NationalSociety = nationalSocieties[0], Name = "Layuna" }
            };
            var districts = new List<District>
            {
                new District { Id = DistrictId, Region = regions[0], Name = "Layuna" }
            };
            var villages = new List<Village>
            {
                new Village { Id = VillageId, District = districts[0], Name = Village }
            };
            var zones = new List<Zone>();
            var dataCollectors = new List<RX.Nyss.Data.Models.DataCollector>
            {
                new RX.Nyss.Data.Models.DataCollector
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
                    Name = "",
                    Sex = Sex.Male
                },
                new RX.Nyss.Data.Models.DataCollector
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
                    Name = "",
                    Sex = Sex.Female
                }
            };

            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    Id = 1,
                    DataCollector = dataCollectors[1],
                    Sender = "+123456",
                },
                new RawReport
                {
                    Id = 2,
                    DataCollector = dataCollectors[1],
                    Sender = "+123456",
                }
            };
            dataCollectors[0].RawReports = new List<RawReport>();
            dataCollectors[1].RawReports = new List<RawReport>{ rawReports[0], rawReports[1] };

            var nationalSocietyMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
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
            _nyssContextMock.DataCollectors.FindAsync(2).Returns((RX.Nyss.Data.Models.DataCollector)null);

            nationalSocietyStructureService.GetRegions(NationalSocietyId).Returns(Success(new List<RegionResponseDto>()));
            nationalSocietyStructureService.GetDistricts(DistrictId).Returns(Success(new List<DistrictResponseDto>()));
            nationalSocietyStructureService.GetVillages(VillageId).Returns(Success(new List<VillageResponseDto>()));
            nationalSocietyStructureService.GetZones(Arg.Any<int>()).Returns(Success(new List<ZoneResponseDto>()));
        }

        [Fact]
        public async Task CreateDataCollector_WhenSuccess_ShouldReturnSuccess()
        {
            // Arrange
            var dataCollector = new CreateDataCollectorRequestDto
            {
                PhoneNumber = "+4712344567",
                SupervisorId = SupervisorId,
                VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                Latitude = 15,
                Longitude = 45,
            };

            // Act
            await _dataCollectorService.CreateDataCollector(ProjectId, dataCollector);

            // Assert
            await _nyssContextMock.Received(1).AddAsync(Arg.Any<RX.Nyss.Data.Models.DataCollector>());
        }

        [Fact]
        public async Task CreateDataCollector_WhenPhoneNumberAlreadyExists_ShouldReturnError()
        {
            // Arrange
            var dataCollector = new CreateDataCollectorRequestDto
            {
                PhoneNumber = DataCollectorPhoneNumber1,
                SupervisorId = SupervisorId,
                VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                Latitude = 15,
                Longitude = 45
            };

            // Act
            var result = await _dataCollectorService.CreateDataCollector(ProjectId, dataCollector);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.DataCollector.PhoneNumberAlreadyExists);
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

            Should.ThrowAsync<Exception>(() => _dataCollectorService.EditDataCollector(dataCollector));
        }

        [Fact]
        public async Task EditDataCollector_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            var dataCollector = new EditDataCollectorRequestDto
            {
                Id = DataCollectorWithoutReportsId,
                PhoneNumber = DataCollectorPhoneNumber1,
                SupervisorId = SupervisorId,
                VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                Latitude = 15,
                Longitude = 45
            };

            // Act
            var result = await _dataCollectorService.EditDataCollector(dataCollector);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.DataCollector.EditSuccess);
        }

        [Fact]
        public async Task RemoveDataCollector_WhenDataCollectorDoesNotExist_ShouldReturnError()
        {
            // Act
            var result = await _dataCollectorService.RemoveDataCollector(999);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.DataCollector.DataCollectorNotFound);
        }

        [Fact]
        public async Task RemoveDataCollector_WhenDataCollectorExists_ShouldReturnSuccess()
        {
            // Act
            var result = await _dataCollectorService.RemoveDataCollector(DataCollectorWithoutReportsId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.DataCollector.RemoveSuccess);
        }

        [Fact]
        public async Task GetDataCollector_WhenDataCollectorExists_ShouldReturnSuccess()
        {
            // Act
            var result = await _dataCollectorService.GetDataCollector(DataCollectorWithoutReportsId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldBe(DataCollectorWithoutReportsId);
        }

        [Fact]
        public void GetDataCollector_WhenDataCollectorDoesNotExist_ShouldThrowException()
        {
            Should.ThrowAsync<Exception>(() => _dataCollectorService.GetDataCollector(3));
        }

        [Fact]
        public async Task ListDataCollector_WhenSuccessful_ShouldReturnSuccess()
        {
            // Act
            var result = await _dataCollectorService.ListDataCollectors(ProjectId, "", new List<string>());

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count().ShouldBe(2);
            var dataCollector = result.Value.First();
            dataCollector.Id.ShouldBe(DataCollectorWithoutReportsId);
            dataCollector.DisplayName.ShouldBe("");
            dataCollector.PhoneNumber.ShouldBe(DataCollectorPhoneNumber1);
            dataCollector.Village.ShouldBe(Village);
            dataCollector.District.ShouldBe("Layuna");
            dataCollector.Name.ShouldBe("");
            dataCollector.Sex.ShouldBe(Sex.Male);
            dataCollector.Region.ShouldBe("Layuna");

            var secondDataCollector = result.Value.Last();
            secondDataCollector.Id.ShouldBe(DataCollectorWithReportsId);
            secondDataCollector.Sex.ShouldBe(Sex.Female);
        }

        [Fact]
        public async Task RemoveDataCollector_WhenDataCollectorHasReports_ShouldAnonymizeDataCollector()
        {
            //Arrange
            var dataCollector = _nyssContextMock.DataCollectors.Single(x => x.Id == DataCollectorWithReportsId);
            var anonymizationString = "--Data removed--";

            // Act
            var result = await _dataCollectorService.RemoveDataCollector(DataCollectorWithReportsId);

            //Assert
            result.IsSuccess.ShouldBeTrue();
            dataCollector.Name.ShouldBe(anonymizationString);
            dataCollector.DisplayName.ShouldBe(anonymizationString);
            dataCollector.PhoneNumber.ShouldBe(anonymizationString);
            dataCollector.AdditionalPhoneNumber.ShouldBe(anonymizationString);
        }

        [Fact]
        public async Task RemoveDataCollector_WhenDataCollectorHasReports_AnonymizationUpdateForReportsWasReceived()
        {
            //Arrange
            var dataCollector = _nyssContextMock.DataCollectors.Single(x => x.Id == DataCollectorWithReportsId);
            var anonymizationString = "--Data removed--";
            FormattableString expectedSqlCommand = $"UPDATE Nyss.RawReports SET Sender = {anonymizationString} WHERE DataCollectorId = {dataCollector.Id}";

            // Act
            var result = await _dataCollectorService.RemoveDataCollector(DataCollectorWithReportsId);

            //Assert
            result.IsSuccess.ShouldBeTrue();
            await _nyssContextMock.Received().ExecuteSqlInterpolatedAsync(Arg.Is<FormattableString>(arg => arg.ToString() == expectedSqlCommand.ToString()));
        }

    }
}
