using FluentValidation;

namespace RX.Nyss.Web.Features.NationalSociety.User.TechnicalAdvisor.Dto
{
    public class CreateTechnicalAdvisorRequestDto: ICreateNationalSocietyUserRequestDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }

        public class CreateTechnicalAdvisorValidator : AbstractValidator<CreateTechnicalAdvisorRequestDto>
        {
            public CreateTechnicalAdvisorValidator()
            {
                RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
                RuleFor(m => m.Email).NotEmpty().MaximumLength(100).EmailAddress();
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20);
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20);
                RuleFor(m => m.Organization).NotEmpty().MaximumLength(100);
            }
        }
    }
}
