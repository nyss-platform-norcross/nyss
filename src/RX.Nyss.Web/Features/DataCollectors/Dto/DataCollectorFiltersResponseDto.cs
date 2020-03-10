using System.Collections.Generic;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorFiltersReponseDto
    {
        public IEnumerable<DataCollectorSupervisorResponseDto> Supervisors { get; set; }
        public int NationalSocietyId { get; set; }
    }
}