using System;
using FluentValidation;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Reports.Dto;

public class ReportRequestDto
{
    public DateTime Date { get; set; }
    public int DataCollectorId { get; set; }
    public int DataCollectorLocationId { get; set; }
    public ReportStatus ReportStatus { get; set; }
    public int HealthRiskId { get; set; }
    public int? CountMalesBelowFive { get; set; }
    public int? CountMalesAtLeastFive { get; set; }
    public int? CountFemalesBelowFive { get; set; }
    public int? CountFemalesAtLeastFive { get; set; }
    public int? CountUnspecifiedSexAndAge { get; set; }
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
            RuleFor(r => r.CountMalesBelowFive).GreaterThanOrEqualTo(0).When(r => r.CountMalesBelowFive.HasValue);
            RuleFor(r => r.CountMalesAtLeastFive).GreaterThanOrEqualTo(0).When(r => r.CountMalesAtLeastFive.HasValue);
            RuleFor(r => r.CountFemalesBelowFive).GreaterThanOrEqualTo(0).When(r => r.CountFemalesBelowFive.HasValue);
            RuleFor(r => r.CountFemalesAtLeastFive).GreaterThanOrEqualTo(0).When(r => r.CountFemalesAtLeastFive.HasValue);
            RuleFor(r => r.CountUnspecifiedSexAndAge).GreaterThanOrEqualTo(0).When(r => r.CountUnspecifiedSexAndAge.HasValue);
            RuleFor(hr => hr.ReferredCount).GreaterThanOrEqualTo(0).When(hr => hr.ReferredCount.HasValue);
            RuleFor(hr => hr.DeathCount).GreaterThanOrEqualTo(0).When(hr => hr.DeathCount.HasValue);
            RuleFor(hr => hr.FromOtherVillagesCount).GreaterThanOrEqualTo(0).When(hr => hr.FromOtherVillagesCount.HasValue);
            RuleFor(hr => hr.DataCollectorLocationId).GreaterThan(0);
        }
    }
}