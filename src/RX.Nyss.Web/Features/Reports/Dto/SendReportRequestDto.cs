using FluentValidation;

namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class SendReportRequestDto
    {
        public int DataCollectorId { get; set; }
        public string Timestamp { get; set; }
        public string Text { get; set; }
        public int? ModemId { get; set; }
        public int UtcOffset { get; set; }

        public class Validator : AbstractValidator<SendReportRequestDto>
        {
            public Validator()
            {
                RuleFor(x => x.DataCollectorId).GreaterThan(0);
                RuleFor(x => x.Timestamp).NotNull().NotEmpty();
                RuleFor(x => x.Text).NotNull().NotEmpty();
                RuleFor(x => x.ModemId).NotEmpty().When(x => x.ModemId.HasValue);
                RuleFor(x => x.UtcOffset).NotEmpty();
            }
        }
    }
}