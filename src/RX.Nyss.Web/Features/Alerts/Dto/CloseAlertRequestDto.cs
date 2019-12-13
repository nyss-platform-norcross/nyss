using FluentValidation;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class CloseAlertRequestDto
    {
        public string Comments { get; set; }

        public class Validator : AbstractValidator<CloseAlertRequestDto>
        {
            public Validator()
            {
                RuleFor(hr => hr.Comments).MaximumLength(500);
            }
        }
    }
}
