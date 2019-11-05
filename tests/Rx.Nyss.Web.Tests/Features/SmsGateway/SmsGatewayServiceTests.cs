using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.SmsGateway;
using RX.Nyss.Web.Features.SmsGateway.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using Shouldly;
using Xunit;

namespace Rx.Nyss.Web.Tests.Features.SmsGateway
{
    public class SmsGatewayServiceTests
    {
        private readonly ISmsGatewayService _smsGatewayService;
        private readonly INyssContext _nyssContextMock;
        private readonly IBlobService _blobServiceMock;

        public SmsGatewayServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            var loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            var config = Substitute.For<IConfig>();
            _blobServiceMock = Substitute.For<IBlobService>();
            _smsGatewayService = new SmsGatewayService(_nyssContextMock, loggerAdapterMock, config, _blobServiceMock);
        }

        [Fact]
        public async Task GetSmsGateways_WhenNationalSocietyIsProvided_ShouldFilterResults()
        {
            // Arrange
            const int nationalSocietyId = 1;

            var gatewaySettings = new[]
            {
                new GatewaySetting { Id = 1, NationalSocietyId = nationalSocietyId },
                new GatewaySetting { Id = 2, NationalSocietyId = 2 },
                new GatewaySetting { Id = 3, NationalSocietyId = nationalSocietyId }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            // Act
            var result = await _smsGatewayService.GetSmsGateways(nationalSocietyId);

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

            var gatewaySettings = new List<GatewaySetting>
            {
                new GatewaySetting { Id = 1, NationalSocietyId = 1 },
                new GatewaySetting { Id = 2, NationalSocietyId = 2 },
                new GatewaySetting { Id = existingNationalSocietyId, Name = "Name", ApiKey = "api-key", NationalSocietyId = 1, GatewayType = GatewayType.SmsEagle}
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            // Act
            var result = await _smsGatewayService.GetSmsGateway(existingNationalSocietyId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldBe(3);
            result.Value.Name.ShouldBe("Name");
            result.Value.ApiKey.ShouldBe("api-key");
            result.Value.NationalSocietyId.ShouldBe(1);
            result.Value.GatewayType.ShouldBe(GatewayType.SmsEagle);
        }

        [Fact]
        public async Task GetSmsGateway_WhenSmsGatewayDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentSmsGatewayId = 0;

            var gatewaySettings = new List<GatewaySetting>
            {
                new GatewaySetting { Id = 1 }
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            // Act
            var result = await _smsGatewayService.GetSmsGateway(nonExistentSmsGatewayId);

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
                new RX.Nyss.Data.Models.NationalSociety { Id = nationalSocietyId, Name = "National Society"}
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var gatewaySettings = new[]
            {
                new GatewaySetting { Id = 1, Name = "Name", ApiKey = "api-key", NationalSocietyId = nationalSocietyId, GatewayType = GatewayType.SmsEagle}
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            var gatewaySettingRequestDto = new GatewaySettingRequestDto
            {
                Name = "New SMS Gateway",
                ApiKey = "new-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            // Act
            var result = await _smsGatewayService.AddSmsGateway(nationalSocietyId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.GatewaySettings.Received(1).AddAsync(
                Arg.Is<GatewaySetting>(gs =>
                    gs.Name == "New SMS Gateway" &&
                    gs.ApiKey == "new-api-key" &&
                    gs.GatewayType == GatewayType.SmsEagle));
            await _nyssContextMock.Received(1).SaveChangesAsync();
            await _blobServiceMock.Received(1).UpdateBlob(Arg.Any<string>(), Arg.Any<string>());
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
                new RX.Nyss.Data.Models.NationalSociety { Id = 1, Name = "National Society"}
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var gatewaySettingRequestDto = new GatewaySettingRequestDto
            {
                Name = "New SMS Gateway",
                ApiKey = "new-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            // Act
            var result = await _smsGatewayService.AddSmsGateway(nonExistentNationalSocietyId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.GatewaySettings.DidNotReceive().AddAsync(Arg.Any<GatewaySetting>());
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            await _blobServiceMock.DidNotReceive().UpdateBlob(Arg.Any<string>(), Arg.Any<string>());
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.SmsGateway.NationalSocietyDoesNotExist);
        }

        [Fact]
        public async Task AddSmsGateway_WhenApiKeyAlreadyExists_ShouldReturnError()
        {
            // Arrange
            const int nationalSocietyId = 1;

            var nationalSocieties = new[]
            {
                new RX.Nyss.Data.Models.NationalSociety { Id = nationalSocietyId, Name = "National Society"}
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var gatewaySettings = new[]
            {
                new GatewaySetting { Id = 1, Name = "Name", ApiKey = "existing-api-key", NationalSocietyId = nationalSocietyId, GatewayType = GatewayType.SmsEagle}
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            var gatewaySettingRequestDto = new GatewaySettingRequestDto
            {
                Name = "New SMS Gateway",
                ApiKey = "existing-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            // Act
            var result = await _smsGatewayService.AddSmsGateway(nationalSocietyId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.GatewaySettings.DidNotReceive().AddAsync(Arg.Any<GatewaySetting>());
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            await _blobServiceMock.DidNotReceive().UpdateBlob(Arg.Any<string>(), Arg.Any<string>());
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.SmsGateway.ApiKeyAlreadyExists);
        }

        [Fact]
        public async Task AddSmsGateway_WhenExceptionIsThrown_ShouldReturnError()
        {
            // Arrange
            const int nationalSocietyId = 1;

            var nationalSocieties = new[]
            {
                new RX.Nyss.Data.Models.NationalSociety { Id = nationalSocietyId, Name = "National Society"}
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var gatewaySettings = new[]
            {
                new GatewaySetting { Id = 1, Name = "Name", ApiKey = "api-key", NationalSocietyId = nationalSocietyId, GatewayType = GatewayType.SmsEagle}
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            var gatewaySettingRequestDto = new GatewaySettingRequestDto
            {
                Name = "New SMS Gateway",
                ApiKey = "new-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            _blobServiceMock.UpdateBlob(Arg.Any<string>(), Arg.Any<string>()).ThrowsForAnyArgs(new ResultException(ResultKey.UnexpectedError));

            // Act
            var result = await _smsGatewayService.AddSmsGateway(nationalSocietyId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.GatewaySettings.Received(1).AddAsync(
                Arg.Is<GatewaySetting>(gs =>
                    gs.Name == "New SMS Gateway" &&
                    gs.ApiKey == "new-api-key" &&
                    gs.GatewayType == GatewayType.SmsEagle));
            await _nyssContextMock.Received(1).SaveChangesAsync();
            await _blobServiceMock.Received(1).UpdateBlob(Arg.Any<string>(), Arg.Any<string>());
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
                new RX.Nyss.Data.Models.NationalSociety { Id = 1, Name = "National Society"}
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);
            
            var gatewaySettings = new[]
            {
                new GatewaySetting { Id = smsGatewayId, Name = "Name", ApiKey = "api-key", NationalSocietyId = 1, GatewayType = GatewayType.SmsEagle}
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.GatewaySettings.FindAsync(smsGatewayId).Returns(gatewaySettings[0]);

            var gatewaySettingRequestDto = new GatewaySettingRequestDto
            {
                Name = "Updated SMS Gateway",
                ApiKey = "updated-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            // Act
            var result = await _smsGatewayService.UpdateSmsGateway(smsGatewayId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.Received(1).SaveChangesAsync();
            await _blobServiceMock.Received(1).UpdateBlob(Arg.Any<string>(), Arg.Any<string>());
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
                new RX.Nyss.Data.Models.NationalSociety { Id = 1, Name = "National Society"}
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);
            
            var gatewaySettings = new[]
            {
                new GatewaySetting { Id = 1, Name = "Name", ApiKey = "api-key", NationalSocietyId = 1, GatewayType = GatewayType.SmsEagle}
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.GatewaySettings.FindAsync(nonExistentSmsGatewayId).ReturnsNull();

            var gatewaySettingRequestDto = new GatewaySettingRequestDto
            {
                Name = "Updated SMS Gateway",
                ApiKey = "updated-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            // Act
            var result = await _smsGatewayService.UpdateSmsGateway(nonExistentSmsGatewayId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            await _blobServiceMock.DidNotReceive().UpdateBlob(Arg.Any<string>(), Arg.Any<string>());
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
        }

        [Fact]
        public async Task UpdateSmsGateway_WhenApiKeyAlreadyExists_ShouldReturnError()
        {
            // Arrange
            const int smsGatewayId = 1;

            var nationalSocieties = new[]
            {
                new RX.Nyss.Data.Models.NationalSociety { Id = 1, Name = "National Society"}
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);
            
            var gatewaySettings = new[]
            {
                new GatewaySetting { Id = smsGatewayId, Name = "Name", ApiKey = "api-key", NationalSocietyId = 1, GatewayType = GatewayType.SmsEagle},
                new GatewaySetting { Id = 2, Name = "Name", ApiKey = "existing-api-key", NationalSocietyId = 1, GatewayType = GatewayType.SmsEagle}
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.GatewaySettings.FindAsync(smsGatewayId).Returns(gatewaySettings[0]);

            var gatewaySettingRequestDto = new GatewaySettingRequestDto
            {
                Name = "Updated SMS Gateway",
                ApiKey = "existing-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            // Act
            var result = await _smsGatewayService.UpdateSmsGateway(smsGatewayId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            await _blobServiceMock.DidNotReceive().UpdateBlob(Arg.Any<string>(), Arg.Any<string>());
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.SmsGateway.ApiKeyAlreadyExists);
        }

        [Fact]
        public async Task UpdateSmsGateway_WhenExceptionIsThrown_ShouldReturnError()
        {
            // Arrange
            const int smsGatewayId = 1;

            var nationalSocieties = new[]
            {
                new RX.Nyss.Data.Models.NationalSociety { Id = 1, Name = "National Society"}
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var gatewaySettings = new[]
            {
                new GatewaySetting { Id = smsGatewayId, Name = "Name", ApiKey = "api-key", NationalSocietyId = 1, GatewayType = GatewayType.SmsEagle}
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            _nyssContextMock.GatewaySettings.FindAsync(smsGatewayId).Returns(gatewaySettings[0]);

            var gatewaySettingRequestDto = new GatewaySettingRequestDto
            {
                Name = "Updated SMS Gateway",
                ApiKey = "updated-api-key",
                GatewayType = GatewayType.SmsEagle
            };

            _blobServiceMock.UpdateBlob(Arg.Any<string>(), Arg.Any<string>()).ThrowsForAnyArgs(new ResultException(ResultKey.UnexpectedError));

            // Act
            var result = await _smsGatewayService.UpdateSmsGateway(smsGatewayId, gatewaySettingRequestDto);

            // Assert
            await _nyssContextMock.Received(1).SaveChangesAsync();
            await _blobServiceMock.Received(1).UpdateBlob(Arg.Any<string>(), Arg.Any<string>());
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }

        [Fact]
        public async Task DeleteSmsGateway_WhenSmsGatewayExists_ShouldReturnSuccess()
        {
            // Arrange
            const int existingSmsGatewayId = 1;

            var gatewaySettings = new[]
            {
                new GatewaySetting { Id = existingSmsGatewayId, Name = "Name", ApiKey = "api-key", NationalSocietyId = 1, GatewayType = GatewayType.SmsEagle}
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            
            // Act
            var result = await _smsGatewayService.DeleteSmsGateway(existingSmsGatewayId);

            // Assert
            _nyssContextMock.GatewaySettings.Received(1).Remove(Arg.Is<GatewaySetting>(gs => gs.Id == existingSmsGatewayId));
            await _nyssContextMock.Received(1).SaveChangesAsync();
            await _blobServiceMock.Received(1).UpdateBlob(Arg.Any<string>(), Arg.Any<string>());
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
                new GatewaySetting { Id = 1, Name = "Name", ApiKey = "api-key", NationalSocietyId = 1, GatewayType = GatewayType.SmsEagle}
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            
            // Act
            var result = await _smsGatewayService.DeleteSmsGateway(nonExistentSmsGatewayId);

            // Assert
            _nyssContextMock.GatewaySettings.DidNotReceive().Remove(Arg.Any<GatewaySetting>());
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            await _blobServiceMock.DidNotReceive().UpdateBlob(Arg.Any<string>(), Arg.Any<string>());
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
                new GatewaySetting { Id = smsGatewayId, Name = "Name", ApiKey = "api-key", NationalSocietyId = 1, GatewayType = GatewayType.SmsEagle}
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);

            _blobServiceMock.UpdateBlob(Arg.Any<string>(), Arg.Any<string>()).ThrowsForAnyArgs(new ResultException(ResultKey.UnexpectedError));

            // Act
            var result = await _smsGatewayService.DeleteSmsGateway(smsGatewayId);

            // Assert
            _nyssContextMock.GatewaySettings.Received(1).Remove(Arg.Is<GatewaySetting>(gs => gs.Id == smsGatewayId));
            await _nyssContextMock.Received(1).SaveChangesAsync();
            await _blobServiceMock.Received(1).UpdateBlob(Arg.Any<string>(), Arg.Any<string>());
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }

        [Fact]
        public async Task UpdateAuthorizedApiKeys_Always_ShouldCallBlobService()
        {
            // Arrange
            var gatewaySettings = new[]
            {
                new GatewaySetting { Id = 1, ApiKey = "first-api-key", NationalSocietyId = 1},
                new GatewaySetting { Id = 2, ApiKey = "second-api-key", NationalSocietyId = 1},
                new GatewaySetting { Id = 3, ApiKey = "third-api-key", NationalSocietyId = 1}
            };

            var gatewaySettingsMockDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsMockDbSet);
            
            // Act
            await _smsGatewayService.UpdateAuthorizedApiKeys();

            // Assert
            await _blobServiceMock.Received(1).UpdateBlob(Arg.Any<string>(), Arg.Is<string>(c => 
                c.Contains("first-api-key") &&
                c.Contains("second-api-key") &&
                c.Contains("third-api-key")));
        }
    }
}
