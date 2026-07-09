using System.Collections.Generic;
using FluentValidation;

namespace RX.Nyss.Web.Features.SuspectedDiseases.Dto
{
    public class SuspectedDiseaseRequestDto
    {
        public int SuspectedDiseaseCode { get; set; }

        public IEnumerable<SuspectedDiseaseLanguageContentDto> LanguageContent { get; set; } = new List<SuspectedDiseaseLanguageContentDto>();

        public class Validator : AbstractValidator<SuspectedDiseaseRequestDto>
        {
            public Validator()
            {
                RuleFor(hr => hr.SuspectedDiseaseCode).GreaterThan(0);
                RuleFor(hr => hr.LanguageContent).NotEmpty();
            }
        }
    }
}
