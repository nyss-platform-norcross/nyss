using System;
using FluentValidation;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public class CreateHealthRiskRequestDto
    {
        public int HealthRiskCode { get; set; }

        public string Name { get; set; }

        public HealthRiskType HealthRiskType { get; set; }

        public string CaseDefinitionEnglish { get; set; }

        public string CaseDefinitionFrench { get; set; }

        public string FeedbackMessageEnglish { get; set; }

        public string FeedbackMessageFrench { get; set; }

        public string AlertRuleInformation { get; set; }

        public int AlertRuleCountThreshold { get; set; }

        public int AlertRuleHoursThreshold { get; set; }

        public int AlertRuleDistanceBetweenCases { get; set; }

        public class Validator : AbstractValidator<CreateHealthRiskRequestDto>
        {
            public Validator()
            {
                RuleFor(hr => hr.Name).NotEmpty().MaximumLength(100);
                RuleFor(hr => hr.HealthRiskCode).NotEmpty();
                RuleFor(hr => hr.HealthRiskType).NotEmpty();
                RuleFor(hr => hr.CaseDefinitionEnglish).NotEmpty().MaximumLength(500);
                RuleFor(hr => hr.CaseDefinitionFrench).NotEmpty().MaximumLength(500);
                RuleFor(hr => hr.FeedbackMessageEnglish).NotEmpty().MaximumLength(500);
                RuleFor(hr => hr.FeedbackMessageFrench).NotEmpty().MaximumLength(500);
            }
        }
    }
}
