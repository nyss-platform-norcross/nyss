using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class ExportDataCollectorsResponseDto
    {
        public string DataCollectorType { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public Sex? Sex { get; set; }
        public int? BirthGroupDecade { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Village { get; set; }
        public string District { get; set; }
        public string Region { get; set; }
        public string Zone { get; set; }
        public string Supervisor { get; set; }
        public string TrainingStatus { get; set; }
    }
}
