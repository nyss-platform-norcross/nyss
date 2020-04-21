using FluentValidation;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class CloseAlertRequestDto
    {
        public CloseAlertOptions CloseOption { get; set; }
        public string Comments { get; set; }

        public class Validator : AbstractValidator<CloseAlertRequestDto>
        {
            public Validator()
            {
                RuleFor(ca => ca.CloseOption).IsInEnum();
                RuleFor(ca => ca.Comments).MaximumLength(500);
                RuleFor(ca => ca.Comments).NotEmpty().When(ca => ca.CloseOption == CloseAlertOptions.Other);
            }
        }
    }
}
