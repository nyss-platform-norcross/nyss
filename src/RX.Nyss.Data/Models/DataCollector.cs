using NetTopologySuite.Geometries;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class DataCollector
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DataCollectorType DataCollectorType { get; set; }
        public string DisplayName { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public SupervisorUser Supervisor { get; set; }
        public Project Project { get; set; }
        public Village Village { get; set; }
        public Zone Zone { get; set; }
        public Point Location { get; set; }
    }
}
