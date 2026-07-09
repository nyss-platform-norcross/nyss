using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorFormDataResponse
    {
        public int NationalSocietyId { get; set; }

        public List<RegionResponseDto> Regions { get; set; } = new List<RegionResponseDto>();

        public List<DataCollectorSupervisorResponseDto> Supervisors { get; set; } = new List<DataCollectorSupervisorResponseDto>();

        public LocationDto DefaultLocation { get; set; } = new LocationDto();

        public string CountryCode { get; set; } = string.Empty;

        public int? DefaultSupervisorId { get; set; }
    }
}
