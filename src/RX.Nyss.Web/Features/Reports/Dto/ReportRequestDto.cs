using System;
using FluentValidation;
using Microsoft.CodeAnalysis;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataCollectors.Dto;

namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class ReportRequestDto
    {
        public DateTime Date { get; set; }
        public int DataCollectorId { get; set; }
        public DataCollectorLocationRequestDto DataCollectorLocation { get; set; }
        public ReportStatus ReportStatus { get; set; }
        public int HealthRiskId { get; set; }
        public int CountMalesBelowFive { get; set; }
        public int CountMalesAtLeastFive { get; set; }
        public int CountFemalesBelowFive { get; set; }
        public int CountFemalesAtLeastFive { get; set; }
        public int CountUnspecifiedSexAndAge { get; set; }
        public int? ReferredCount { get; set; }
        public int? DeathCount { get; set; }
        public int? FromOtherVillagesCount { get; set; }

        public class Validator : AbstractValidator<ReportRequestDto>
        {
            public Validator()
            {
                RuleFor(hr => hr.Date).GreaterThan(DateTime.MinValue);
                RuleFor(hr => hr.DataCollectorId).GreaterThanOrEqualTo(0);
                RuleFor(hr => hr.HealthRiskId).GreaterThan(0);
                RuleFor(hr => hr.CountMalesBelowFive).GreaterThanOrEqualTo(0);
                RuleFor(hr => hr.CountMalesAtLeastFive).GreaterThanOrEqualTo(0);
                RuleFor(hr => hr.CountFemalesBelowFive).GreaterThanOrEqualTo(0);
                RuleFor(hr => hr.CountFemalesAtLeastFive).GreaterThanOrEqualTo(0);
                RuleFor(hr => hr.ReferredCount).GreaterThanOrEqualTo(0).When(hr => hr.ReferredCount.HasValue);
                RuleFor(hr => hr.DeathCount).GreaterThanOrEqualTo(0).When(hr => hr.DeathCount.HasValue);
                RuleFor(hr => hr.FromOtherVillagesCount).GreaterThanOrEqualTo(0).When(hr => hr.FromOtherVillagesCount.HasValue);
            }
        }
    }
}
