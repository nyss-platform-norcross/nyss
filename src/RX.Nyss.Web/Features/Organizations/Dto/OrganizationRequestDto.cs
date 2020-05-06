using FluentValidation;

namespace RX.Nyss.Web.Features.Organizations.Dto
{
    public class OrganizationRequestDto
    {
        public string Name { get; set; }

        public class OrganizationValidator : AbstractValidator<OrganizationRequestDto>
        {
            public OrganizationValidator()
            {
                RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            }
        }
    }
}
