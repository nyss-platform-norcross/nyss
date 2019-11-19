using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;
        private readonly IGeolocationService _geolocationService;

        private const int DataCollectorId = 1;
        private const string DataCollectorPhoneNumber = "+4712345678";
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
            _nationalSocietyStructureService = Substitute.For<INationalSocietyStructureService>();
            _geolocationService = Substitute.For<IGeolocationService>();
            _dataCollectorService = new DataCollectorService(_nyssContextMock, _nationalSocietyStructureService, _geolocationService);

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
                    Id = DataCollectorId,
                    PhoneNumber = DataCollectorPhoneNumber,
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
                }
            };

            var nationalSocietyMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            var usersMockDbSet = users.AsQueryable().BuildMockDbSet();
            var userNationalSocietitiesMockDbSet = usersNationalSocieties.AsQueryable().BuildMockDbSet();
            var projectsMockDbSet = projects.AsQueryable().BuildMockDbSet();
            var regionsMockDbSet = regions.AsQueryable().BuildMockDbSet();
            var districtsMockDbSet = districts.AsQueryable().BuildMockDbSet();
            var villagesMockDbSet = villages.AsQueryable().BuildMockDbSet();
            var dataCollectorsMockDbSet = dataCollectors.AsQueryable().BuildMockDbSet();
            var zonesMockDbSet = zones.AsQueryable().BuildMockDbSet();

            _nyssContextMock.NationalSocieties.Returns(nationalSocietyMockDbSet);
            _nyssContextMock.Users.Returns(usersMockDbSet);
            _nyssContextMock.UserNationalSocieties.Returns(userNationalSocietitiesMockDbSet);
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            _nyssContextMock.Regions.Returns(regionsMockDbSet);
            _nyssContextMock.Districts.Returns(districtsMockDbSet);
            _nyssContextMock.Villages.Returns(villagesMockDbSet);
            _nyssContextMock.DataCollectors.Returns(dataCollectorsMockDbSet);
            _nyssContextMock.Zones.Returns(zonesMockDbSet);

            _nyssContextMock.DataCollectors.FindAsync(DataCollectorId).Returns(dataCollectors[0]);
            _nyssContextMock.DataCollectors.FindAsync(2).Returns((RX.Nyss.Data.Models.DataCollector)null);

            _nationalSocietyStructureService.GetRegions(NationalSocietyId).Returns(Success(new List<RegionResponseDto>()));
            _nationalSocietyStructureService.GetDistricts(DistrictId).Returns(Success(new List<DistrictResponseDto>()));
            _nationalSocietyStructureService.GetVillages(VillageId).Returns(Success(new List<VillageResponseDto>()));
            _nationalSocietyStructureService.GetZones(Arg.Any<int>()).Returns(Success(new List<ZoneResponseDto>()));

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
            var result = await _dataCollectorService.CreateDataCollector(ProjectId, dataCollector);

            // Assert
            await _nyssContextMock.Received(1).AddAsync(Arg.Any<RX.Nyss.Data.Models.DataCollector>());
        }

        [Fact]
        public async Task CreateDataCollector_WhenPhoneNumberAlreadyExists_ShouldReturnError()
        {
            // Arrange
            var dataCollector = new CreateDataCollectorRequestDto
            {
                PhoneNumber = DataCollectorPhoneNumber,
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
        public async Task EditDataCollector_WhenDataCollectorDoesntExist_ShouldThrowException()
        {
            // Arrange
            var dataCollector = new EditDataCollectorRequestDto
            {
                Id = 2,
                PhoneNumber = DataCollectorPhoneNumber,
                SupervisorId = SupervisorId,
                VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                Latitude = 15,
                Longitude = 45
            };

            await Should.ThrowAsync<Exception>(() => _dataCollectorService.EditDataCollector(dataCollector));
        }

        [Fact]
        public async Task EditDataCollector_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            var dataCollector = new EditDataCollectorRequestDto
            {
                Id = DataCollectorId,
                PhoneNumber = DataCollectorPhoneNumber,
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
        public async Task RemoveDataCollector_WhenDataCollectorDoesntExist_ShouldReturnError()
        {
            // Act
            var result = await _dataCollectorService.RemoveDataCollector(2);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.DataCollector.DataCollectorNotFound);
        }

        [Fact]
        public async Task RemoveDataCollector_WhenDataCollectorExists_ShouldReturnSuccess()
        {
            // Act
            var result = await _dataCollectorService.RemoveDataCollector(DataCollectorId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.DataCollector.RemoveSuccess);
        }

        [Fact]
        public async Task GetDataCollector_WhenDataCollectorExists_ShouldReturnSuccess()
        {
            // Act
            var result = await _dataCollectorService.GetDataCollector(DataCollectorId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldBe(DataCollectorId);
        }

        [Fact]
        public async Task GetDataCollector_WhenDataCollectorDoesntExist_ShouldThrowException()
        {
            await Should.ThrowAsync<Exception>(() => _dataCollectorService.GetDataCollector(2));
        }

        [Fact]
        public async Task ListDataCollector_WhenSuccessful_ShouldReturnSuccess()
        {
            // Act
            var result = await _dataCollectorService.ListDataCollectors(ProjectId, "", new List<string>());

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count().ShouldBeGreaterThan(0);
        }
    }
}
