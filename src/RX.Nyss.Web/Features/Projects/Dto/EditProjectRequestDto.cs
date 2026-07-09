using FluentValidation;

namespace RX.Nyss.Web.Features.Projects.Dto
{
    public class EditProjectRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public bool AllowMultipleOrganizations { get; set; }

        public class Validator : AbstractValidator<EditProjectRequestDto>
        {
            public Validator()
            {
                RuleFor(p => p.Name).NotEmpty().MaximumLength(200);
                RuleFor(p => p.Name).MaximumLength(50);
            }
        }
    }
}
