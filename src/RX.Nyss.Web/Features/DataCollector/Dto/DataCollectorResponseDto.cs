using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.DataCollector.Dto
{
    public class DataCollectorResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public Sex Sex { get; set; }

        public string PhoneNumber { get; set; }

        public string Village { get; set; }

        public string District { get; set; }

        public string Region { get; set; }

        public bool IsInTrainingMode { get; set; }
    }
}
