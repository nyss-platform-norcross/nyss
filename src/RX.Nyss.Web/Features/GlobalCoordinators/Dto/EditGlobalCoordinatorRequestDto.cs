using FluentValidation;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.GlobalCoordinators.Dto
{
    public class EditGlobalCoordinatorRequestDto
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string PhoneNumber { get; set; }
        
        public string Organization { get; set; }
        
        public string AdditionalPhoneNumber { get; set; }

        public override string ToString() => $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(PhoneNumber)}: {PhoneNumber}, {nameof(Organization)}: {Organization}";

        public class EditGlobalCoordinatorValidator : AbstractValidator<EditGlobalCoordinatorRequestDto>
        {
            public EditGlobalCoordinatorValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
                RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(x => x.PhoneNumber).MaximumLength(20);
                RuleFor(x => x.Organization).MaximumLength(100);
            }
        }
    }
}
