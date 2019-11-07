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
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using Shouldly;
using Xunit;

namespace Rx.Nyss.Web.Tests.Features.DataCollector
{
    public class DataCollectorServiceTests
    {
        private readonly INyssContext _nyssContextMock;
        private readonly ILoggerAdapter _loggerMock;
        private readonly IDataCollectorService _dataCollectorService;

        private const int DataCollectorId = 1;
        private const string DataCollectorPhoneNumber = "+4712345678";
        private const string DataCollectorName = "Bubba";
        private const int ProjectId = 1;
        private const int SupervisorId = 1;
        private const int NationalSocietyId = 1;
        private const string Village = "Layuna";

        public DataCollectorServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _loggerMock = Substitute.For<ILoggerAdapter>();
            _dataCollectorService = new DataCollectorService(_loggerMock, _nyssContextMock);

            // Arrange
            var nationalSocieties = new List<RX.Nyss.Data.Models.NationalSociety>
            {
                new RX.Nyss.Data.Models.NationalSociety { Id = NationalSocietyId }
            };
            var users = new List<User>
            {
                new SupervisorUser { Id = SupervisorId }
            };
            var projects = new List<Project>
            {
                new Project { Id = ProjectId, NationalSociety = nationalSocieties[0] }
            };
            var regions = new List<Region>
            {
                new Region { NationalSociety = nationalSocieties[0], Name = "Layuna" }
            };
            var districts = new List<District>
            {
                new District { Region = regions[0], Name = "Layuna" }
            };
            var villages = new List<Village>
            {
                new Village { District = districts[0], Name = Village }
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
                    BirthYearGroup = "",
                    DataCollectorType = DataCollectorType.Human,
                    DisplayName = "",
                    Location = new Point(0, 0),
                    Name = "",
                    Sex = Sex.Male
                }
            };

            var nationalSocietyMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            var usersMockDbSet = users.AsQueryable().BuildMockDbSet();
            var projectsMockDbSet = projects.AsQueryable().BuildMockDbSet();
            var regionsMockDbSet = regions.AsQueryable().BuildMockDbSet();
            var districtsMockDbSet = districts.AsQueryable().BuildMockDbSet();
            var villagesMockDbSet = villages.AsQueryable().BuildMockDbSet();
            var dataCollectorsMockDbSet = dataCollectors.AsQueryable().BuildMockDbSet();
            var zonesMockDbSet = zones.AsQueryable().BuildMockDbSet();

            _nyssContextMock.NationalSocieties.Returns(nationalSocietyMockDbSet);
            _nyssContextMock.Users.Returns(usersMockDbSet);
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            _nyssContextMock.Regions.Returns(regionsMockDbSet);
            _nyssContextMock.Districts.Returns(districtsMockDbSet);
            _nyssContextMock.Villages.Returns(villagesMockDbSet);
            _nyssContextMock.DataCollectors.Returns(dataCollectorsMockDbSet);
            _nyssContextMock.Zones.Returns(zonesMockDbSet);

            _nyssContextMock.DataCollectors.FindAsync(DataCollectorId).Returns(dataCollectors[0]);
            _nyssContextMock.DataCollectors.FindAsync(2).Returns((RX.Nyss.Data.Models.DataCollector)null);
        }

        [Fact]
        public async Task CreateDataCollector_WhenSuccess_ShouldReturnSuccess()
        {
            // Arrange
            var dataCollector = new CreateDataCollectorRequestDto
            {
                PhoneNumber = "+4712344567",
                SupervisorId = SupervisorId,
                Village = _nyssContextMock.Villages.ToList()[0].Name,
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
                Village = _nyssContextMock.Villages.ToList()[0].Name,
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
        public async Task EditDataCollector_WhenDataCollectorDoesntExist_ShouldReturnError()
        {
            // Arrange
            var dataCollector = new EditDataCollectorRequestDto
            {
                Id = 2,
                PhoneNumber = DataCollectorPhoneNumber,
                SupervisorId = SupervisorId,
                Village = _nyssContextMock.Villages.ToList()[0].Name,
                Latitude = 15,
                Longitude = 45
            };

            // Act
            var result = await _dataCollectorService.EditDataCollector(ProjectId, dataCollector);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.DataCollector.DataCollectorNotFound);
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
                Village = _nyssContextMock.Villages.ToList()[0].Name,
                Latitude = 15,
                Longitude = 45
            };

            // Act
            var result = await _dataCollectorService.EditDataCollector(ProjectId, dataCollector);

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
        public async Task GetDataCollector_WhenDataCollectorDoesntExist_ShouldReturnError()
        {
            // Act
            var result = await _dataCollectorService.GetDataCollector(2);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.DataCollector.DataCollectorNotFound);
        }

        [Fact]
        public async Task ListDataCollector_WhenSuccessful_ShouldReturnSuccess()
        {
            // Act
            var result = await _dataCollectorService.ListDataCollectors();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count().ShouldBeGreaterThan(0);
        }
    }
}
