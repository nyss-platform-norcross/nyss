using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.Organizations.Validation;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Organizations.Dto
{
    public class OrganizationRequestDto
    {
        public string Name { get; set; }
        public int NationalSocietyId { get; set; }
        public int? Id { get; set; }

        public class OrganizationValidator : AbstractValidator<OrganizationRequestDto>
        {
            public OrganizationValidator(IOrganizationValidationService organizationValidationService)
            {
                RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
                RuleFor(x => x.Name)
                    .MustAsync(async (model, name, t) => !await organizationValidationService.NameExists(model.NationalSocietyId, model.Id, name))
                    .WithMessageKey(ResultKey.Organization.NameAlreadyExists);
            }
        }
    }
}
