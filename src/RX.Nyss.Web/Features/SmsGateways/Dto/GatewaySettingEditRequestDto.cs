using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.SmsGateways.Validation;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.SmsGateways.Dto
{
    public class GatewaySettingEditRequestDto
    {
        public int Id { get; set; }
        public int NationalSocietyId { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string EmailAddress { get; set; }
        public GatewayType GatewayType { get; set; }
        public string IotHubDeviceName { get; set; }

        public class GatewaySettingEditValidator : AbstractValidator<GatewaySettingEditRequestDto>
        {
            public GatewaySettingEditValidator(ISmsGatewayValidationService smsGatewayValidationService)
            {
                RuleFor(gs => gs.Id)
                    .MustAsync(async (model, id, t) => await smsGatewayValidationService.GatewayExists(id))
                    .WithMessageKey(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
                    
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
