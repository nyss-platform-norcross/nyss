using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class MapOverviewResponseDto
    {
        public LocationDto CenterLocation { get; set; }
        public List<MapOverviewLocationResponseDto> DataCollectorLocations { get; set; }
    }
}
