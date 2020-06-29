using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.SmsGateways.Validation;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.SmsGateways.Dto
{
    public class GatewaySettingCreateRequestDto
    {
        public int NationalSocietyId { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string EmailAddress { get; set; }
        public GatewayType GatewayType { get; set; }
        public string IotHubDeviceName { get; set; }

        public class GatewaySettingCreateValidator : AbstractValidator<GatewaySettingCreateRequestDto>
        {
            public GatewaySettingCreateValidator(ISmsGatewayValidationService smsGatewayValidationService)
            {
                RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
                RuleFor(x => x.ApiKey).NotEmpty().MaximumLength(100);
                RuleFor(x => x.GatewayType).IsInEnum();
                When(x => string.IsNullOrEmpty(x.EmailAddress), () => RuleFor(x => x.IotHubDeviceName).NotEmpty().MaximumLength(250))
                    .Otherwise(() => RuleFor(x => x.EmailAddress).NotEmpty().MaximumLength(100));

                RuleFor(gs => gs.ApiKey)
                    .MustAsync(async (model, apiKey, t) => !await smsGatewayValidationService.ApiKeyExists(apiKey))
                    .WithMessageKey(ResultKey.NationalSociety.SmsGateway.ApiKeyAlreadyExists);

                RuleFor(gs => gs.NationalSocietyId)
                    .MustAsync(async (model, nationalSocietyId, t) => await smsGatewayValidationService.NationalSocietyExists(nationalSocietyId))
                    .WithMessageKey(ResultKey.NationalSociety.SmsGateway.NationalSocietyDoesNotExist);
            }
        }
    }
}
