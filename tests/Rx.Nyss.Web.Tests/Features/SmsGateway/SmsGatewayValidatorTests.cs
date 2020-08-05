using FluentValidation.TestHelper;
using NSubstitute;
using RX.Nyss.Web.Features.SmsGateways.Dto;
using RX.Nyss.Web.Features.SmsGateways.Validation;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.SmsGateway
{
    public class SmsGatewayValidatorTests
    {
        private readonly CreateGatewaySettingRequestDto.CreateGatewaySettingRequestValidator _createValidator;

        private readonly GatewaySettingRequestDto.GatewaySettingRequestValidator _editValidator;

        public SmsGatewayValidatorTests()
        {
            var validationService = Substitute.For<ISmsGatewayValidationService>();
            validationService.ApiKeyExists("1234").Returns(true);
            _createValidator = new CreateGatewaySettingRequestDto.CreateGatewaySettingRequestValidator(validationService);
            _editValidator = new GatewaySettingRequestDto.GatewaySettingRequestValidator();
        }

        [Fact]
        public void Create_WhenApiExists_ShouldHaveError() => _createValidator.ShouldHaveValidationErrorFor(gs => gs.ApiKey, "1234");

        [Fact]
        public void Create_WhenEmailIsNullAndIotHubDeviceNameIsNull_ShouldHaveError() => _createValidator.ShouldHaveValidationErrorFor(gs => gs.IotHubDeviceName, null as string);

        [Fact]
        public void Create_WhenIotHubDeviceNameIsSetAndEmailIsNull_ShouldNotHaveError() => _createValidator.ShouldNotHaveValidationErrorFor(gs => gs.IotHubDeviceName, "iothub");

        [Fact]
        public void Create_WhenIotHubDeviceNameIsNullAndEmailIsSet_ShouldNotHaveError() => _createValidator.ShouldNotHaveValidationErrorFor(gs => gs.EmailAddress, "test@example.com");

        [Fact]
        public void Edit_WhenApiExists_ShouldNotHaveError() => _editValidator.ShouldNotHaveValidationErrorFor(gs => gs.ApiKey, "1234");

        [Fact]
        public void Edit_WhenEmailIsNullAndIotHubDeviceNameIsNull_ShouldHaveError() => _editValidator.ShouldHaveValidationErrorFor(gs => gs.IotHubDeviceName, null as string);

        [Fact]
        public void Edit_WhenIotHubDeviceNameIsSetAndEmailIsNull_ShouldNotHaveError() => _editValidator.ShouldNotHaveValidationErrorFor(gs => gs.IotHubDeviceName, "iothub");

        [Fact]
        public void Edit_WhenIotHubDeviceNameIsNullAndEmailIsSet_ShouldNotHaveError() => _editValidator.ShouldNotHaveValidationErrorFor(gs => gs.EmailAddress, "test@example.com");
    }
}
