using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.DataCollector
{
    public class GetDataCollectorResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public Sex Sex { get; set; }
        public DataCollectorType DataCollectorType { get; set; }
        public string BirthYearGroup { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Village { get; set; }
        public string District { get; set; }
        public string Region { get; set; }
        public string Zone { get; set; }
        public int SupervisorId { get; set; }
    }
}
