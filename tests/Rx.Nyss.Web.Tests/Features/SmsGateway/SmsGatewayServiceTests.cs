using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.SmsGateways;
using RX.Nyss.Web.Features.SmsGateways.Dto;
using RX.Nyss.Web.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.SmsGateway
{
    public class SmsGatewayServiceTests
    {
        private readonly ISmsGatewayService _smsGatewayService;
        private readonly INyssContext _nyssContextMock;
        private readonly ISmsGatewayBlobProvider _smsGatewayBlobProviderMock;
        private readonly IIotHubService _iotHubService;
        private readonly ICryptographyService _cryptographyService;
        private readonly INyssWebConfig _nyssWebConfig;

        public SmsGatewayServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            var loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            var config = Substitute.For<INyssWebConfig>();
            _smsGatewayBlobProviderMock = Substitute.For<ISmsGatewayBlobProvider>();
            _iotHubService = Substitute.For<IIotHubService>();
            _cryptographyService = Substitute.For<ICryptographyService>();
            _nyssWebConfig = Substitute.For<INyssWebConfig>();
            _smsGatewayService = new SmsGatewayService(_nyssContextMock,
                loggerAdapterMock,
                _smsGatewayBlobProviderMock,
                _iotHubService,
                _cryptographyService,
                _nyssWebConfig);
        }

        [Fact]
        public async Task GetSmsGateways_WhenNationalSocietyIsProvided_ShouldFilterResults()
        {
            // Arrange
            const int nationalSocietyId = 1;

            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = 1,
                    NationalSocietyId = nationalSocietyId
                },
                new GatewaySetting
                {
                    Id = 2,
                    NationalSocietyId = 2
                },
                new GatewaySetting
                {
                    Id = 3,
                    NationalSocietyId = nationalSocietyId
                }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            // Act
            var result = await _smsGatewayService.List(nationalSocietyId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count.ShouldBe(2);
            result.Value.Select(x => x.Id).ShouldBe(new[] { 1, 3 });
        }

        [Fact]
        public async Task GetSmsGateway_WhenSmsGatewayExists_ShouldReturnSuccess()
        {
            // Arrange
            const int existingNationalSocietyId = 3;

            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = 1,
                    NationalSocietyId = 1
                },
                new GatewaySetting
                {
                    Id = 2,
                    NationalSocietyId = 2
                },
                new GatewaySetting
                {
                    Id = existingNationalSocietyId,
                    Name = "Name",
                    ApiKey = "api-key",
                    NationalSocietyId = 1,
                    GatewayType = GatewayType.SmsEagle
                }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            // Act
            var result = await _smsGatewayService.Get(existingNationalSocietyId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldBe(3);
            result.Value.Name.ShouldBe("Name");
            result.Value.ApiKey.ShouldBe("api-key");
            result.Value.GatewayType.ShouldBe(GatewayType.SmsEagle);
        }

        [Fact]
        public async Task GetSmsGateway_WhenSmsGatewayDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentSmsGatewayId = 0;

            var gatewaySettings = new[] { new GatewaySetting { Id = 1 } };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            // Act
            var result = await _smsGatewayService.Get(nonExistentSmsGatewayId);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
        }

        [Fact]
        public async Task AddSmsGateway_WhenApiKeyDoesNotExistYet_ShouldReturnSuccess()
        {
            // Arrange
            const int nationalSocietyId = 1;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = nationalSocietyId,
                    Name = "National Society"
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = 1,
                    Name = "Name",
                    ApiKey = "api-key",
                    NationalSocietyId = nationalSocietyId,
                    GatewayType = GatewayType.SmsEagle
                }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            var gatewaySettingRequestDto = new EditGatewaySettingRequestDto
            {
                Name = "New SMS Gateway",
                ApiKey = "new-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            // Act
            var result = await _smsGatewayService.Create(nationalSocietyId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.GatewaySettings.Received(1).AddAsync(
                Arg.Is<GatewaySetting>(gs =>
                    gs.Name == "New SMS Gateway" &&
                    gs.ApiKey == "new-api-key" &&
                    gs.GatewayType == GatewayType.SmsEagle));
            await _nyssContextMock.Received(1).SaveChangesAsync();
            var content = Arg.Any<string>();
            await _smsGatewayBlobProviderMock.Received(1).UpdateApiKeys(content);
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.SmsGateway.SuccessfullyAdded);
        }

        [Fact]
        public async Task AddSmsGateway_WhenNationalSocietyDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentNationalSocietyId = 0;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = 1,
                    Name = "National Society"
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var gatewaySettingRequestDto = new EditGatewaySettingRequestDto
            {
                Name = "New SMS Gateway",
                ApiKey = "new-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            // Act
            var result = await _smsGatewayService.Create(nonExistentNationalSocietyId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.GatewaySettings.DidNotReceive().AddAsync(Arg.Any<GatewaySetting>());
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            var content = Arg.Any<string>();
            await _smsGatewayBlobProviderMock.DidNotReceive().UpdateApiKeys(content);
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.SmsGateway.NationalSocietyDoesNotExist);
        }

        [Fact]
        public async Task AddSmsGateway_WhenExceptionIsThrown_ShouldReturnError()
        {
            // Arrange
            const int nationalSocietyId = 1;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = nationalSocietyId,
                    Name = "National Society"
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = 1,
                    Name = "Name",
                    ApiKey = "api-key",
                    NationalSocietyId = nationalSocietyId,
                    GatewayType = GatewayType.SmsEagle
                }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            var gatewaySettingRequestDto = new EditGatewaySettingRequestDto
            {
                Name = "New SMS Gateway",
                ApiKey = "new-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            var content = Arg.Any<string>();
            _smsGatewayBlobProviderMock.UpdateApiKeys(content).ThrowsForAnyArgs(new ResultException(ResultKey.UnexpectedError));

            // Act
            var result = await _smsGatewayService.Create(nationalSocietyId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.GatewaySettings.Received(1).AddAsync(
                Arg.Is<GatewaySetting>(gs =>
                    gs.Name == "New SMS Gateway" &&
                    gs.ApiKey == "new-api-key" &&
                    gs.GatewayType == GatewayType.SmsEagle));
            await _nyssContextMock.Received(1).SaveChangesAsync();
            var content1 = Arg.Any<string>();
            await _smsGatewayBlobProviderMock.Received(1).UpdateApiKeys(content1);
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }

        [Fact]
        public async Task UpdateSmsGateway_WhenSmsGatewayExists_ShouldReturnSuccess()
        {
            // Arrange
            const int smsGatewayId = 1;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = 1,
                    Name = "National Society"
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = smsGatewayId,
                    Name = "Name",
                    ApiKey = "api-key",
                    NationalSocietyId = 1,
                    GatewayType = GatewayType.SmsEagle,
                    Modems = new List<GatewayModem>()
                }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.GatewaySettings.FindAsync(smsGatewayId).Returns(gatewaySettings[0]);

            var gatewaySettingRequestDto = new EditGatewaySettingRequestDto
            {
                Name = "Updated SMS Gateway",
                ApiKey = "updated-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            // Act
            var result = await _smsGatewayService.Edit(smsGatewayId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.Received(1).SaveChangesAsync();
            var content = Arg.Any<string>();
            await _smsGatewayBlobProviderMock.Received(1).UpdateApiKeys(content);
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.SmsGateway.SuccessfullyUpdated);
        }

        [Fact]
        public async Task UpdateSmsGateway_WhenSmsGatewayDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentSmsGatewayId = 0;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = 1,
                    Name = "National Society"
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = 1,
                    Name = "Name",
                    ApiKey = "api-key",
                    NationalSocietyId = 1,
                    GatewayType = GatewayType.SmsEagle
                }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.GatewaySettings.FindAsync(nonExistentSmsGatewayId).ReturnsNull();

            var gatewaySettingRequestDto = new EditGatewaySettingRequestDto
            {
                Name = "Updated SMS Gateway",
                ApiKey = "updated-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            // Act
            var result = await _smsGatewayService.Edit(nonExistentSmsGatewayId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            var content = Arg.Any<string>();
            await _smsGatewayBlobProviderMock.DidNotReceive().UpdateApiKeys(content);
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
        }

        [Fact]
        public async Task UpdateSmsGateway_WhenExceptionIsThrown_ShouldReturnError()
        {
            // Arrange
            const int smsGatewayId = 1;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = 1,
                    Name = "National Society"
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = smsGatewayId,
                    Name = "Name",
                    ApiKey = "api-key",
                    NationalSocietyId = 1,
                    GatewayType = GatewayType.SmsEagle,
                    Modems = new List<GatewayModem>()
                }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.GatewaySettings.FindAsync(smsGatewayId).Returns(gatewaySettings[0]);

            var gatewaySettingRequestDto = new EditGatewaySettingRequestDto
            {
                Name = "Updated SMS Gateway",
                ApiKey = "updated-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            var content = Arg.Any<string>();
            _smsGatewayBlobProviderMock.UpdateApiKeys(content).ThrowsForAnyArgs(new ResultException(ResultKey.UnexpectedError));

            // Act
            var result = await _smsGatewayService.Edit(smsGatewayId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.Received(1).SaveChangesAsync();
            var content1 = Arg.Any<string>();
            await _smsGatewayBlobProviderMock.Received(1).UpdateApiKeys(content1);
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }

        [Fact]
        public async Task UpdateSmsGateway_WhenModemsAreUsedAndRemoved_ShouldRemoveModemReferences()
        {
            // Arrange
            const int smsGatewayId = 1;

            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = smsGatewayId,
                    Name = "Name",
                    ApiKey = "api-key",
                    NationalSocietyId = 1,
                    GatewayType = GatewayType.SmsEagle,
                    Modems = new List<GatewayModem>
                    {
                        new GatewayModem
                        {
                            Id = 1
                        },
                        new GatewayModem
                        {
                            Id = 2
                        }
                    }
                }
            };

            var users = new List<User>
            {
                new ManagerUser
                {
                    Id = 1,
                    Modem = gatewaySettings.First().Modems.First(),
                    Role = Role.Manager
                },
                new TechnicalAdvisorUser
                {
                    Id = 2
                }
            };

            var technicalAdvisorModems = new List<TechnicalAdvisorUserGatewayModem>
            {
                new TechnicalAdvisorUserGatewayModem
                {
                    GatewayModem = gatewaySettings.First().Modems.First(),
                    TechnicalAdvisorUser = (TechnicalAdvisorUser)users[1]
                }
            };
            var alertRecipients = new List<AlertNotificationRecipient>();

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            var usersMockDbSet = users.AsQueryable().BuildMockDbSet();
            var technicalAdvisorModemsMockDbSet = technicalAdvisorModems.AsQueryable().BuildMockDbSet();
            var alertRecipientsMockDbSet = alertRecipients.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.Users.Returns(usersMockDbSet);
            _nyssContextMock.TechnicalAdvisorUserGatewayModems.Returns(technicalAdvisorModemsMockDbSet);
            _nyssContextMock.AlertNotificationRecipients.Returns(alertRecipientsMockDbSet);

            var dto = new EditGatewaySettingRequestDto
            {
                Id = smsGatewayId,
                Name = "Name",
                ApiKey = "api-key"
            };

            // Act
            var result = await _smsGatewayService.Edit(smsGatewayId, dto);

            // Assert
            _nyssContextMock.TechnicalAdvisorUserGatewayModems.Received(1).RemoveRange(Arg.Any<IEnumerable<TechnicalAdvisorUserGatewayModem>>());
            await _nyssContextMock.Received(1).SaveChangesAsync();
            var content1 = Arg.Any<string>();
            await _smsGatewayBlobProviderMock.Received(1).UpdateApiKeys(content1);
            result.IsSuccess.ShouldBeTrue();
            var manager = (ManagerUser)_nyssContextMock.Users.First(u => u.Id == 1);
            manager.Modem.ShouldBeNull();
        }

        [Fact]
        public async Task DeleteSmsGateway_WhenSmsGatewayExists_ShouldReturnSuccess()
        {
            // Arrange
            const int existingSmsGatewayId = 1;

            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = existingSmsGatewayId,
                    Name = "Name",
                    ApiKey = "api-key",
                    NationalSocietyId = 1,
                    GatewayType = GatewayType.SmsEagle,
                    Modems = new List<GatewayModem>()
                }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.GatewaySettings.FindAsync(existingSmsGatewayId).Returns(gatewaySettings[0]);

            // Act
            var result = await _smsGatewayService.Delete(existingSmsGatewayId);

            // Assert
            _nyssContextMock.GatewaySettings.Received(1).Remove(Arg.Is<GatewaySetting>(gs => gs.Id == existingSmsGatewayId));
            await _nyssContextMock.Received(1).SaveChangesAsync();
            var content = Arg.Any<string>();
            await _smsGatewayBlobProviderMock.Received(1).UpdateApiKeys(content);
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.SmsGateway.SuccessfullyDeleted);
        }

        [Fact]
        public async Task DeleteSmsGateway_WhenSmsGatewayDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentSmsGatewayId = 0;

            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = 1,
                    Name = "Name",
                    ApiKey = "api-key",
                    NationalSocietyId = 1,
                    GatewayType = GatewayType.SmsEagle
                }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.GatewaySettings.FindAsync(nonExistentSmsGatewayId).ReturnsNull();

            // Act
            var result = await _smsGatewayService.Delete(nonExistentSmsGatewayId);

            // Assert
            _nyssContextMock.GatewaySettings.DidNotReceive().Remove(Arg.Any<GatewaySetting>());
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            var content = Arg.Any<string>();
            await _smsGatewayBlobProviderMock.DidNotReceive().UpdateApiKeys(content);
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
        }

        [Fact]
        public async Task DeleteSmsGateway_WhenExceptionIsThrown_ShouldReturnError()
        {
            // Arrange
            const int smsGatewayId = 1;

            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = smsGatewayId,
                    Name = "Name",
                    ApiKey = "api-key",
                    NationalSocietyId = 1,
                    GatewayType = GatewayType.SmsEagle,
                    Modems = new List<GatewayModem>()
                }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.GatewaySettings.FindAsync(smsGatewayId).Returns(gatewaySettings[0]);

            var content = Arg.Any<string>();
            _smsGatewayBlobProviderMock.UpdateApiKeys(content).ThrowsForAnyArgs(new ResultException(ResultKey.UnexpectedError));

            // Act
            var result = await _smsGatewayService.Delete(smsGatewayId);

            // Assert
            _nyssContextMock.GatewaySettings.Received(1).Remove(Arg.Is<GatewaySetting>(gs => gs.Id == smsGatewayId));
            await _nyssContextMock.Received(1).SaveChangesAsync();
            var content1 = Arg.Any<string>();
            await _smsGatewayBlobProviderMock.Received(1).UpdateApiKeys(content1);
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }

        [Fact]
        public async Task DeleteSmsGateway_WhenModemsAreUsed_ShouldRemoveModemReferences()
        {
            // Arrange
            const int smsGatewayId = 1;

            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = smsGatewayId,
                    Name = "Name",
                    ApiKey = "api-key",
                    NationalSocietyId = 1,
                    GatewayType = GatewayType.SmsEagle,
                    Modems = new List<GatewayModem>
                    {
                        new GatewayModem
                        {
                            Id = 1
                        },
                        new GatewayModem
                        {
                            Id = 2
                        }
                    }
                }
            };

            var users = new List<User>
            {
                new ManagerUser
                {
                    Id = 1,
                    Modem = gatewaySettings.First().Modems.First(),
                    Role = Role.Manager
                },
                new TechnicalAdvisorUser
                {
                    Id = 2
                }
            };

            var technicalAdvisorModems = new List<TechnicalAdvisorUserGatewayModem>
            {
                new TechnicalAdvisorUserGatewayModem
                {
                    GatewayModem = gatewaySettings.First().Modems.First(),
                    TechnicalAdvisorUser = (TechnicalAdvisorUser)users[1]
                }
            };
            var alertRecipients = new List<AlertNotificationRecipient>();

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            var usersMockDbSet = users.AsQueryable().BuildMockDbSet();
            var technicalAdvisorModemsMockDbSet = technicalAdvisorModems.AsQueryable().BuildMockDbSet();
            var alertRecipientsMockDbSet = alertRecipients.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.Users.Returns(usersMockDbSet);
            _nyssContextMock.TechnicalAdvisorUserGatewayModems.Returns(technicalAdvisorModemsMockDbSet);
            _nyssContextMock.AlertNotificationRecipients.Returns(alertRecipientsMockDbSet);

            // Act
            var result = await _smsGatewayService.Delete(smsGatewayId);

            // Assert
            _nyssContextMock.GatewaySettings.Received(1).Remove(Arg.Is<GatewaySetting>(gs => gs.Id == smsGatewayId));
            _nyssContextMock.TechnicalAdvisorUserGatewayModems.Received(1).RemoveRange(Arg.Any<IEnumerable<TechnicalAdvisorUserGatewayModem>>());
            await _nyssContextMock.Received(1).SaveChangesAsync();
            var content1 = Arg.Any<string>();
            await _smsGatewayBlobProviderMock.Received(1).UpdateApiKeys(content1);
            result.IsSuccess.ShouldBeTrue();
            var manager = (ManagerUser)_nyssContextMock.Users.First(u => u.Id == 1);
            manager.Modem.ShouldBeNull();
        }

        [Fact]
        public async Task UpdateAuthorizedApiKeys_Always_ShouldCallBlobService()
        {
            // Arrange
            var gatewaySettings = new[]
            {
                new GatewaySetting
                {
                    Id = 1,
                    ApiKey = "first-api-key",
                    NationalSocietyId = 1
                },
                new GatewaySetting
                {
                    Id = 2,
                    ApiKey = "second-api-key",
                    NationalSocietyId = 1
                },
                new GatewaySetting
                {
                    Id = 3,
                    ApiKey = "third-api-key",
                    NationalSocietyId = 1
                }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            // Act
            await _smsGatewayService.UpdateAuthorizedApiKeys();

            // Assert
            var content = Arg.Is<string>(c =>
                c.Contains("first-api-key") &&
                c.Contains("second-api-key") &&
                c.Contains("third-api-key"));
            await _smsGatewayBlobProviderMock.Received(1).UpdateApiKeys(content);
        }
    }
}
