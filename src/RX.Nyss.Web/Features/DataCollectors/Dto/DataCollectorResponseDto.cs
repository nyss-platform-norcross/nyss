using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorResponseDto
    {
        public int Id { get; set; }

        public DataCollectorType DataCollectorType { get; set; }

        public string Name { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Sex? Sex { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public bool IsInTrainingMode { get; set; }
        public bool IsDeployed { get; set; }
        public DataCollectorSupervisorResponseDto Supervisor { get; set; } = new DataCollectorSupervisorResponseDto();
        public IEnumerable<DataCollectorLocationResponseDto> Locations { get; set; } = new List<DataCollectorLocationResponseDto>();
    }

    public class DataCollectorLocationResponseDto
    {
        public int Id { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Village { get; set; }
        public string Zone { get; set; }
    }
}
