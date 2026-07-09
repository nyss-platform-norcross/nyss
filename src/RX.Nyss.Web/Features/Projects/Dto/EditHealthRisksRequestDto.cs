using System.Collections.Generic;
using FluentValidation;

namespace RX.Nyss.Web.Features.Projects.Dto;

public class EditHealthRisksRequestDto
{
    public IEnumerable<ProjectHealthRiskRequestDto> HealthRisks { get; set; } = new List<ProjectHealthRiskRequestDto>();

    public class Validator : AbstractValidator<EditHealthRisksRequestDto>
    {
        public Validator()
        {
            RuleFor(p => p.HealthRisks).NotNull();
            RuleForEach(p => p.HealthRisks)
                .OverrideIndexer((x, collection, element, index) => $".{element.HealthRiskId}")
                .SetValidator(new ProjectHealthRiskRequestDto.Validator())
                .OverridePropertyName("HealthRisk");
        }
    }
}
