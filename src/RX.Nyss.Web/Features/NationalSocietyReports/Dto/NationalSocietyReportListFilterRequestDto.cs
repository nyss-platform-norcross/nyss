using FluentValidation;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyReports.Dto
{
    public class NationalSocietyReportListFilterRequestDto
    {
        public static readonly string DateColumnName = "date";
        public NationalSocietyReportListType ReportsType { get; set; } = NationalSocietyReportListType.Main;
        public AreaDto Area { get; set; }
        public int? HealthRiskId { get; set; }
        public bool Status { get; set; }
        public string OrderBy { get; set; }
        public bool SortAscending { get; set; }

        public class Validator : AbstractValidator<NationalSocietyReportListFilterRequestDto>
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
