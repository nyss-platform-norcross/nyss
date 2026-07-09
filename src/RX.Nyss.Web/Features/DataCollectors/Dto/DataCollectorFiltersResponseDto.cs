using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorFiltersReponseDto
    {
        public IEnumerable<DataCollectorSupervisorResponseDto> Supervisors { get; set; } = new List<DataCollectorSupervisorResponseDto>();
        public StructureResponseDto Locations { get; set; } = new StructureResponseDto();
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; } = new List<HealthRiskDto>();
    }
}