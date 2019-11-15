using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.DataCollector.Dto
{
    public class DataCollectorFormDataResponse
    {
        public int NationalSocietyId { get; set; }

        public List<RegionResponseDto> Regions { get; set; }

        public List<DataCollectorSupervisorResponseDto> Supervisors { get; set; }

        public LocationDto DefaultLocation { get; set; }

        public int? DefaultSupervisorId { get; set; }
    }
}
