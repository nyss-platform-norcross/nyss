using FluentValidation;

namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class SendReportRequestDto
    {
        public string Sender { get; set; }
        public string Timestamp { get; set; }
        public string Text { get; set; }
        public string ApiKey { get; set; }

        public class Validator : AbstractValidator<SendReportRequestDto>
        {
            public Validator()
            {
                RuleFor(x => x.Sender).NotNull().NotEmpty();
                RuleFor(x => x.Timestamp).NotNull().NotEmpty();
                RuleFor(x => x.Text).NotNull().NotEmpty();
                RuleFor(x => x.ApiKey).NotNull().NotEmpty();
            }
        }
    }
}