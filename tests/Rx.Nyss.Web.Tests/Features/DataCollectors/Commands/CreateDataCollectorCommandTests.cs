using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.DataCollectors.Queries;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataCollectors.Commands
{
    public class CreateDataCollectorCommandTests
    {
        private readonly INyssContext _nyssContextMock;
        private readonly CreateDataCollectorCommand.Handler _handler;

        private const int SupervisorId = 1;
        private const int HeadSupervisorId = 2;

        public CreateDataCollectorCommandTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            var dateTimeProviderMock = Substitute.For<IDateTimeProvider>();

            ArrangeData();

            _handler = new CreateDataCollectorCommand.Handler(_nyssContextMock, dateTimeProviderMock);
        }

        [Theory]
        [InlineData(DataCollectorType.Human, "Human")]
        [InlineData(DataCollectorType.CollectionPoint, null)]
        public async Task CreateDataCollector_WhenLinkingToSupervisor_ShouldReturnSuccess(DataCollectorType type, string displayName)
        {
            // Arrange
            var command = new CreateDataCollectorCommand
            {
                ProjectId = 1,
                DataCollectorType = type,
                PhoneNumber = "+4712344567",
                DisplayName = displayName,
                SupervisorId = SupervisorId,
                Locations = new List<DataCollectorLocationRequestDto>
                {
                    new()
                    {
                        VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                        Latitude = 15,
                        Longitude = 45
                    }
                }
            };

            // Act
            var res = await _handler.Handle(command, CancellationToken.None);

            // Assert
            res.IsSuccess.ShouldBeTrue();
            await _nyssContextMock.Received(1).AddAsync(Arg.Any<DataCollector>());
        }

        [Theory]
        [InlineData(DataCollectorType.Human, "Human")]
        [InlineData(DataCollectorType.CollectionPoint, null)]
        public async Task CreateDataCollector_WhenPhoneNumberIsEmpty_ShouldReturnSuccess(DataCollectorType type, string displayName)
        {
            // Arrange
            var command = new CreateDataCollectorCommand
            {
                ProjectId = 1,
                DataCollectorType = type,
                PhoneNumber = null,
                DisplayName = displayName,
                SupervisorId = SupervisorId,
                Locations = new List<DataCollectorLocationRequestDto>
                {
                    new()
                    {
                        VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                        Latitude = 15,
                        Longitude = 45
                    }
                }
            };

            // Act
            var res = await _handler.Handle(command, CancellationToken.None);

            // Assert
            res.IsSuccess.ShouldBeTrue();
            await _nyssContextMock.Received(1).AddAsync(Arg.Any<DataCollector>());
        }

        [Fact]
        public async Task CreateDataCollector_WhenLinkingToHeadSupervisor_ShouldReturnSuccess()
        {
            // Arrange
            var command = new CreateDataCollectorCommand
            {
                ProjectId = 1,
                DataCollectorType = DataCollectorType.Human,
                PhoneNumber = "+4712344567",
                DisplayName = "",
                SupervisorId = HeadSupervisorId,
                LinkedToHeadSupervisor = true,
                Locations = new List<DataCollectorLocationRequestDto>
                {
                    new()
                    {
                        VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                        Latitude = 15,
                        Longitude = 45
                    }
                }
            };

            // Act
            var res = await _handler.Handle(command, CancellationToken.None);

            // Assert
            res.IsSuccess.ShouldBeTrue();
            await _nyssContextMock.Received(1).AddAsync(Arg.Any<DataCollector>());
        }

        private void ArrangeData()
        {
            var nationalSocieties = new List<NationalSociety> { new () { Id = 1 } };
            var projects = new List<Project> { new () { Id = 1, State = ProjectState.Open, NationalSociety = nationalSocieties[0] } };
            var userNationalSocieties = new List<UserNationalSociety>
            {
                new()
                {
                    NationalSocietyId = 1,
                    User = new SupervisorUser { Id = SupervisorId, Role = Role.Supervisor }
                },
                new()
                {
                    NationalSocietyId = 1,
                    User = new HeadSupervisorUser { Id = HeadSupervisorId, Role = Role.HeadSupervisor }
                }
            };
            var regions = new List<Region> { new() { Id = 1, NationalSociety = nationalSocieties[0] } };
            var districts = new List<District> { new() { Id = 1, Region = regions[0] } };
            var villages = new List<Village> { new() { Id = 1, District = districts[0] } };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            var projectsMockDbSet = projects.AsQueryable().BuildMockDbSet();
            var userNationalSocietiesMockDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            var regionsMockDbSet = regions.AsQueryable().BuildMockDbSet();
            var districtsMockDbSet = districts.AsQueryable().BuildMockDbSet();
            var villagesMockDbSet = villages.AsQueryable().BuildMockDbSet();

            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            _nyssContextMock.UserNationalSocieties.Returns(userNationalSocietiesMockDbSet);
            _nyssContextMock.Regions.Returns(regionsMockDbSet);
            _nyssContextMock.Districts.Returns(districtsMockDbSet);
            _nyssContextMock.Villages.Returns(villagesMockDbSet);
        }
    }
}
