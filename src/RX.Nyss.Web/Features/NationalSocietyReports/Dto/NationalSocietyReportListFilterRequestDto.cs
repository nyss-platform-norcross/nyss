using System;
using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyReports.Dto
{
    public class NationalSocietyReportListFilterRequestDto
    {
        public static readonly string DateColumnName = "date";
        public ReportListDataCollectorType DataCollectorType { get; set; } = ReportListDataCollectorType.Human;
        public ReportErrorFilterType? ErrorType { get; set; }
        public IList<int> HealthRisks { get; set; }
        public AreaDto? Locations { get; set; }
        public ReportStatusFilterDto? ReportStatus { get; set; }
        public string OrderBy { get; set; }
        public bool SortAscending { get; set; }
        public int UtcOffset { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public class Validator : AbstractValidator<NationalSocietyReportListFilterRequestDto>
        {
            public Validator()
            {
                RuleFor(f => f.DataCollectorType).IsInEnum();
                RuleForEach(f => f.HealthRisks).Must(x => x > 0);
                RuleFor(f => f.OrderBy).Equal(DateColumnName);
                RuleFor(f => f.Locations).SetValidator(new AreaDto.Validator()!);
                RuleFor(f => f.ErrorType).IsInEnum().When(f => f.ErrorType.HasValue);
            }
        }
    }
}