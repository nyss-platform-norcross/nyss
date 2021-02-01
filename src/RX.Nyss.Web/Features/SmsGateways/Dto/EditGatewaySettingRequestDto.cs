using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.SmsGateways.Validation;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.SmsGateways.Dto
{
    public class EditGatewaySettingRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string EmailAddress { get; set; }
        public GatewayType GatewayType { get; set; }
        public string IotHubDeviceName { get; set; }
        public string ModemOneName { get; set; }
        public string ModemTwoName { get; set; }

        public class GatewaySettingRequestValidator : AbstractValidator<EditGatewaySettingRequestDto>
        {
            public GatewaySettingRequestValidator(ISmsGatewayValidationService smsGatewayValidationService)
            {
                RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
                RuleFor(x => x.ApiKey).NotEmpty().MaximumLength(100);
                RuleFor(x => x.GatewayType).IsInEnum();
                When(x => string.IsNullOrEmpty(x.EmailAddress), () => RuleFor(x => x.IotHubDeviceName).NotEmpty().MaximumLength(250))
                    .Otherwise(() => RuleFor(x => x.EmailAddress).NotEmpty().MaximumLength(100));
                RuleFor(gs => gs.ApiKey)
                    .MustAsync(async (model, apiKey, t) => !await smsGatewayValidationService.ApiKeyExistsToOther(model.Id, apiKey))
                    .WithMessageKey(ResultKey.NationalSociety.SmsGateway.ApiKeyAlreadyExists);
                RuleFor(x => x.ModemOneName).NotEmpty().MaximumLength(100)
                    .When(x => !string.IsNullOrEmpty(x.ModemTwoName));
                RuleFor(x => x.ModemTwoName).NotEmpty().MaximumLength(100)
                    .When(x => !string.IsNullOrEmpty(x.ModemOneName));
            }
        }
    }
}
