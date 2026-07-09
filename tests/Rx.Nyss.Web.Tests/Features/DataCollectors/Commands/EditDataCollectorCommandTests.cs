using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataCollectors.Commands;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataCollectors.Commands;

public class EditDataCollectorCommandTests
{
    private const int SupervisorId = 1;

    private const int HeadSupervisorId = 2;

    private const int DataCollectorId = 1;

    private const string DataCollectorPhoneNumber = "+4725323525";

    private readonly INyssContext _nyssContextMock;

    private readonly IDateTimeProvider _mockDateTimeProvider;

    private readonly EditDataCollectorCommand.Handler _handler;

    public EditDataCollectorCommandTests()
    {
        _nyssContextMock = Substitute.For<INyssContext>();
        _mockDateTimeProvider = Substitute.For<IDateTimeProvider>();

        ArrangeData();

        _handler = new EditDataCollectorCommand.Handler(_nyssContextMock, _mockDateTimeProvider);
    }

    [Fact]
    public void WhenDataCollectorDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var command = new EditDataCollectorCommand
        {
            Id = 3,
            PhoneNumber = DataCollectorPhoneNumber,
            SupervisorId = SupervisorId,
            Locations = new List<DataCollectorLocationRequestDto>
            {
                new ()
                {
                    VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                    Latitude = 15,
                    Longitude = 45
                }
            }
        };

        Should.ThrowAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Theory]
    [InlineData(DataCollectorType.Human, "Human")]
    [InlineData(DataCollectorType.CollectionPoint, null)]
    public async Task WhenSuccessful_ShouldReturnSuccess(DataCollectorType type, string displayName)
    {
        // Arrange
        var command = new EditDataCollectorCommand
        {
            DataCollectorType = type,
            Id = DataCollectorId,
            DisplayName = displayName,
            PhoneNumber = DataCollectorPhoneNumber,
            SupervisorId = SupervisorId,
            Locations = new List<DataCollectorLocationRequestDto>
            {
                new ()
                {
                    VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                    Latitude = 15,
                    Longitude = 45
                },
                new ()
                {
                    Id = 1,
                    VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                    Latitude = 15,
                    Longitude = 45
                }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Message.Key.ShouldBe(ResultKey.DataCollector.EditSuccess);
    }

    [Theory]
    [InlineData(DataCollectorType.Human, "Human")]
    [InlineData(DataCollectorType.CollectionPoint, null)]
    public async Task WhenPhoneNumberIsEmpty_ShouldReturnSuccess(DataCollectorType type, string displayName)
    {
        // Arrange
        var command = new EditDataCollectorCommand
        {
            DataCollectorType = type,
            Id = DataCollectorId,
            DisplayName = displayName,
            PhoneNumber = null,
            SupervisorId = SupervisorId,
            Locations = new List<DataCollectorLocationRequestDto>
            {
                new ()
                {
                    VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                    Latitude = 15,
                    Longitude = 45
                },
                new ()
                {
                    Id = 1,
                    VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                    Latitude = 15,
                    Longitude = 45
                }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Message.Key.ShouldBe(ResultKey.DataCollector.EditSuccess);
    }

    [Fact]
    public async Task WhenEditingToBeLinkedToHeadSupervisor_ShouldReturnSuccess()
    {
        // Arrange
        var command = new EditDataCollectorCommand
        {
            DataCollectorType = DataCollectorType.Human,
            Id = DataCollectorId,
            DisplayName = "",
            PhoneNumber = DataCollectorPhoneNumber,
            SupervisorId = HeadSupervisorId,
            LinkedToHeadSupervisor = true,
            Locations = new List<DataCollectorLocationRequestDto>
            {
                new ()
                {
                    VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                    Latitude = 15,
                    Longitude = 45
                },
                new ()
                {
                    Id = 1,
                    VillageId = _nyssContextMock.Villages.ToList()[0].Id,
                    Latitude = 15,
                    Longitude = 45
                }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Message.Key.ShouldBe(ResultKey.DataCollector.EditSuccess);
    }

    private void ArrangeData()
    {
        var nationalSocieties = new List<NationalSociety> { new() { Id = 1 } };
        var projects = new List<Project> { new() { Id = 1, State = ProjectState.Open, NationalSociety = nationalSocieties[0] } };
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
        var zones = new List<Zone>();
        var dataCollectors = new List<DataCollector>
        {
            new ()
            {
                Id = DataCollectorId,
                Project = projects[0],
                PhoneNumber = DataCollectorPhoneNumber,
                Supervisor = (SupervisorUser)userNationalSocieties[0].User,
                DataCollectorLocations = new List<DataCollectorLocation>
                {
                    new ()
                    {
                        Id = 1,
                        Village = villages[0]
                    }
                }
            }
        };

        var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
        var projectsMockDbSet = projects.AsQueryable().BuildMockDbSet();
        var userNationalSocietiesMockDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
        var regionsMockDbSet = regions.AsQueryable().BuildMockDbSet();
        var districtsMockDbSet = districts.AsQueryable().BuildMockDbSet();
        var villagesMockDbSet = villages.AsQueryable().BuildMockDbSet();
        var zonesMockDbSet = zones.AsQueryable().BuildMockDbSet();
        var dataCollectorsMockDbSet = dataCollectors.AsQueryable().BuildMockDbSet();

        _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);
        _nyssContextMock.Projects.Returns(projectsMockDbSet);
        _nyssContextMock.UserNationalSocieties.Returns(userNationalSocietiesMockDbSet);
        _nyssContextMock.Regions.Returns(regionsMockDbSet);
        _nyssContextMock.Districts.Returns(districtsMockDbSet);
        _nyssContextMock.Villages.Returns(villagesMockDbSet);
        _nyssContextMock.Zones.Returns(zonesMockDbSet);
        _nyssContextMock.DataCollectors.Returns(dataCollectorsMockDbSet);
    }
}
