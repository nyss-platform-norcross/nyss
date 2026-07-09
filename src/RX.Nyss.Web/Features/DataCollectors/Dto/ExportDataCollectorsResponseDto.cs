using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class ExportDataCollectorsResponseDto
    {
        public string DataCollectorType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public Sex? Sex { get; set; }
        public int? BirthDecade { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? AdditionalPhoneNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Village { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string? Zone { get; set; }
        public string Supervisor { get; set; } = string.Empty;
        public string TrainingStatus { get; set; } = string.Empty;
        public string Deployed { get; set; } = string.Empty;
    }
}
