using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class AlertListFilterResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; }
        public StructureResponseDto Locations { get; set; }
    }
}
