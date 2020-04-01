using FluentValidation;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.SmsGateways.Dto
{
    public class GatewaySettingRequestDto
    {
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string EmailAddress { get; set; }
        public GatewayType GatewayType { get; set; }
        public string IotHubDeviceName { get; set; }

        public class GatewaySettingValidator : AbstractValidator<GatewaySettingRequestDto>
        {
            public GatewaySettingValidator()
            {
                RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
                RuleFor(x => x.ApiKey).NotEmpty().MaximumLength(100);
                RuleFor(x => x.GatewayType).IsInEnum();
                When(x => string.IsNullOrEmpty(x.EmailAddress), () => RuleFor(x => x.IotHubDeviceName).NotEmpty().MaximumLength(250))
                    .Otherwise(() => RuleFor(x => x.EmailAddress).NotEmpty().MaximumLength(100));
            }
        }
    }
}
