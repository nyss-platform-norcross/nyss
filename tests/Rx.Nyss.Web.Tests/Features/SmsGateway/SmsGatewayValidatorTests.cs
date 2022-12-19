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

        private readonly EditGatewaySettingRequestDto.GatewaySettingRequestValidator _editValidator;

        public SmsGatewayValidatorTests()
        {
            var validationService = Substitute.For<ISmsGatewayValidationService>();
            validationService.ApiKeyExists("1234").Returns(true);
            validationService.ApiKeyExistsToOther(1, "1234").Returns(true);
            _createValidator = new CreateGatewaySettingRequestDto.CreateGatewaySettingRequestValidator(validationService);
            _editValidator = new EditGatewaySettingRequestDto.GatewaySettingRequestValidator(validationService);
        }

        [Fact]
        public void Edit_WhenApiExistsToOther_ShouldHaveError()
        {
            var result = _editValidator.TestValidateAsync(new EditGatewaySettingRequestDto
            {
                Id = 1,
                ApiKey = "1234"
            });
        }
    }
}
