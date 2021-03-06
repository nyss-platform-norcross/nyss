﻿using System.Collections.Generic;
using FluentValidation;

namespace RX.Nyss.Web.Features.Projects.Dto
{
    public class EditProjectRequestDto
    {
        public string Name { get; set; }

        public bool AllowMultipleOrganizations { get; set; }

        public IEnumerable<ProjectHealthRiskRequestDto> HealthRisks { get; set; }

        public class Validator : AbstractValidator<EditProjectRequestDto>
        {
            public Validator()
            {
                RuleFor(p => p.Name).NotEmpty().MaximumLength(200);
                RuleFor(p => p.Name).MaximumLength(50);
                RuleFor(p => p.HealthRisks).NotNull();
                RuleForEach(p => p.HealthRisks)
                    .OverrideIndexer((x, collection, element, index) => $".{element.HealthRiskId}")
                    .SetValidator(new ProjectHealthRiskRequestDto.Validator())
                    .OverridePropertyName("HealthRisk");
            }
        }
    }
}
