using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.SmsGateways.Validation;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.SmsGateways.Dto;

public class CreateGatewaySettingRequestDto : EditGatewaySettingRequestDto
{
    public class CreateGatewaySettingRequestValidator : AbstractValidator<CreateGatewaySettingRequestDto>
    {
        public CreateGatewaySettingRequestValidator(ISmsGatewayValidationService smsGatewayValidationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);
            RuleFor(x => x.ApiKey)
                .NotEmpty()
                .MaximumLength(100);
            RuleFor(x => x.GatewayType)
                .IsInEnum();
            When(x => string.IsNullOrEmpty(x.EmailAddress), () => RuleFor(x => x.IotHubDeviceName)
                    .NotEmpty()
                    .MaximumLength(250))
                .Otherwise(() => RuleFor(x => x.EmailAddress)
                    .NotEmpty()
                    .MaximumLength(100));
            RuleFor(gs => gs.ApiKey)
                .MustAsync(async (model, apiKey, t) => !await smsGatewayValidationService.ApiKeyExists(apiKey))
                .WithMessageKey(ResultKey.NationalSociety.SmsGateway.ApiKeyAlreadyExists);
            RuleFor(x => x.ModemOneName)
                .NotEmpty()
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.IotHubDeviceName) && !string.IsNullOrEmpty(x.ModemTwoName));
            RuleFor(x => x.ModemTwoName)
                .NotEmpty()
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.IotHubDeviceName) && !string.IsNullOrEmpty(x.ModemOneName));
        }
    }
}
