using FluentValidation;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class ReportListFilterRequestDto
    {
        public static readonly string DateColumnName = "date";
        public ReportListType ReportsType { get; set; } = ReportListType.Main;
        public AreaDto Area { get; set; }
        public int? HealthRiskId { get; set; }
        public bool Status { get; set; }
        public bool IsTraining { get; set; }
        public string OrderBy { get; set; }
        public bool SortAscending { get; set; }
        public int UtcOffset { get; set; }

        public class Validator : AbstractValidator<ReportListFilterRequestDto>
        {
            public Validator()
            {
                RuleFor(f => f.ReportsType).IsInEnum();
                RuleFor(f => f.HealthRiskId).GreaterThan(0).When(f => f.HealthRiskId.HasValue);
                RuleFor(f => f.OrderBy).Equal(DateColumnName);
                RuleFor(f => f.Area).SetValidator(new AreaDto.Validator());
            }
        }
    }
}
