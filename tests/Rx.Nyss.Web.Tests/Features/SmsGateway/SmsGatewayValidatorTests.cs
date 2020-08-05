using FluentValidation.TestHelper;
using NSubstitute;
using RX.Nyss.Web.Features.SmsGateways.Dto;
using RX.Nyss.Web.Features.SmsGateways.Validation;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.SmsGateway
{
    public class SmsGatewayValidatorTests
    {
        private GatewaySettingRequestDto.GatewaySettingRequestValidator Validator { get; }

        public SmsGatewayValidatorTests()
        {
            var validationService = Substitute.For<ISmsGatewayValidationService>();
            validationService.ApiKeyExists("1234").Returns(true);
            Validator = new GatewaySettingRequestDto.GatewaySettingRequestValidator(validationService);
        }

        [Fact]
        public void Create_WhenApiExists_ShouldHaveError()
        {
            Validator.ShouldHaveValidationErrorFor(gs => gs.ApiKey, "1234");
        }

        [Fact]
        public void Create_WhenEmailIsNullAndIotHubDeviceNameIsNull_ShouldHaveError()
        {
            Validator.ShouldHaveValidationErrorFor(gs => gs.IotHubDeviceName, null as string);
        }

        [Fact]
        public void Create_WhenIotHubDeviceNameIsSetAndEmailIsNull_ShouldNotHaveError()
        {
            Validator.ShouldNotHaveValidationErrorFor(gs => gs.IotHubDeviceName, "iothub");
        }

        [Fact]
        public void Create_WhenIotHubDeviceNameIsNullAndEmailIsSet_ShouldNotHaveError()
        {
            Validator.ShouldNotHaveValidationErrorFor(gs => gs.EmailAddress, "test@example.com");
        }
    }
}
