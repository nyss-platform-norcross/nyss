using FluentValidation;
using RX.Nyss.Web.Features.Administration.GlobalCoordinator.Dto;

namespace RX.Nyss.Web.Features.Administration.GlobalCoordinator.Validators
{
    public class EditGlobalCoordinatorValidator : AbstractValidator<EditGlobalCoordinatorRequestDto>
    {
        public EditGlobalCoordinatorValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Organization).MaximumLength(100);
        }
    }
}
