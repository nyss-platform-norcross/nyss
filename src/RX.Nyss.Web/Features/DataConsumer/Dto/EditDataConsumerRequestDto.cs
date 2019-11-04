using FluentValidation;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.DataConsumer.Dto
{
    public class EditDataConsumerRequestDto
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }

        public class EditDataConsumerValidator : AbstractValidator<EditDataConsumerRequestDto>
        {
            public EditDataConsumerValidator()
            {
                RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20).PhoneNumber();
                RuleFor(m => m.Organization).NotEmpty().MaximumLength(100);
            }
        }
    }
}
