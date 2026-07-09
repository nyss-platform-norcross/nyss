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
        public string Name { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string? EmailAddress { get; set; }
        public GatewayType GatewayType { get; set; }
        public string? IotHubDeviceName { get; set; }
        public string? ModemOneName { get; set; }
        public string? ModemTwoName { get; set; }
        public string? TelerivetApiKey { get; set; }
        public string? TelerivetProjectId { get; set; }
        public string? GatewayApiKey { get; set; }
        public string? GatewayApiKeyName { get; set; }
        public string? GatewayExtraKey { get; set; }
        public string? GatewayExtraKeyName { get; set; }
        public string? GatewayUrl { get; set; }
        public string? GatewayAuthUrl { get; set; }
        public string? GatewaySenderId { get; set; }

        public class GatewaySettingRequestValidator : AbstractValidator<EditGatewaySettingRequestDto>
        {
            public GatewaySettingRequestValidator(ISmsGatewayValidationService smsGatewayValidationService)
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
                    .MustAsync(async (model, apiKey, t) => !await smsGatewayValidationService.ApiKeyExistsToOther(model.Id, apiKey))
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
}
