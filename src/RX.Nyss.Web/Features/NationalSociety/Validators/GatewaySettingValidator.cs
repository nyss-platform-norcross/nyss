using FluentValidation;
using RX.Nyss.Web.Features.NationalSociety.Dto;

namespace RX.Nyss.Web.Features.NationalSociety.Validators
{
    public class GatewaySettingValidator : AbstractValidator<GatewaySettingRequestDto>
    {
        public GatewaySettingValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ApiKey).NotEmpty().MaximumLength(100);
            RuleFor(x => x.GatewayType).IsInEnum();
        }
    }
}
