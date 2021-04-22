using FluentValidation;

namespace RX.Nyss.Web.Features.Common.Dto
{
    public class AreaDto
    {
        public int Id { get; set; }

        public AreaType Type { get; set; }

        public class Validator : AbstractValidator<AreaDto>
        {
            public Validator()
            {
                RuleFor(a => a.Type).IsInEnum();
            }
        }
    }
}
