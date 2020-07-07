using FluentValidation.TestHelper;
using NSubstitute;
using NUnit.Framework;
using RX.Nyss.Web.Features.SmsGateways.Dto;
using RX.Nyss.Web.Features.SmsGateways.Validation;

namespace RX.Nyss.Web.Tests.Validators
{
    [TestFixture]
    public class SmsGatewayValidatorTests
    {
        private GatewaySettingRequestDto.GatewaySettingRequestValidator Validator { get; set; }

        public SmsGatewayValidatorTests()
        {
            var validationService = Substitute.For<ISmsGatewayValidationService>();
            validationService.ApiKeyExists("1234").Returns(true);
            Validator = new GatewaySettingRequestDto.GatewaySettingRequestValidator(validationService);
        }

        [Test]
        public void Create_WhenApiExists_ShouldHaveError()
        {
            Validator.ShouldHaveValidationErrorFor(gs => gs.ApiKey, "1234");
        }

        [Test]
        public void Create_WhenEmailIsNullAndIotHubDeviceNameIsNull_ShouldHaveError()
        {
            Validator.ShouldHaveValidationErrorFor(gs => gs.IotHubDeviceName, null as string);
        }

        [Test]
        public void Create_WhenIotHubDeviceNameIsSetAndEmailIsNull_ShouldNotHaveError()
        {
            Validator.ShouldNotHaveValidationErrorFor(gs => gs.IotHubDeviceName, "iothub");
        }

        [Test]
        public void Create_WhenIotHubDeviceNameIsNullAndEmailIsSet_ShouldNotHaveError()
        {
            Validator.ShouldNotHaveValidationErrorFor(gs => gs.EmailAddress, "test@example.com");
        }
    }
}