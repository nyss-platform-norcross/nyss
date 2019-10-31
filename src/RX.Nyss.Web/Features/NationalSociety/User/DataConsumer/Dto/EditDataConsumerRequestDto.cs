using FluentValidation;

namespace RX.Nyss.Web.Features.NationalSociety.User.DataConsumer.Dto
{
    public class EditDataConsumerRequestDto : IEditNationalSocietyUserRequestDto
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
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20);
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20);
                RuleFor(m => m.Organization).NotEmpty().MaximumLength(100);
            }
        }
    }
}
