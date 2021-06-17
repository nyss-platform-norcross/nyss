using FluentValidation;

namespace RX.Nyss.Web.Features.AlertEvents.Dto
{
    public class EditAlertEventRequestDto
    {
        public int AlertEventLogId { get; set; }
        public string Text { get; set; }

        public class Validator : AbstractValidator<CreateAlertEventRequestDto>
        {
            public Validator()
            {
                RuleFor(x => x.Text)
                    .MaximumLength(4000);
            }
        }
    }
}
