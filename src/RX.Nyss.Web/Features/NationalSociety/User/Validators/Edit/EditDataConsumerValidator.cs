using FluentValidation;
using RX.Nyss.Web.Features.NationalSociety.User.Dto.Edit;

namespace RX.Nyss.Web.Features.NationalSociety.User.Validators.Edit
{
    public class EditDataConsumerValidator : AbstractValidator<EditDataConsumerRequestDto>
    {
        public EditDataConsumerValidator()
        {
            RuleFor(m => m.Organization).NotEmpty().MaximumLength(100);
        }
    }
}
