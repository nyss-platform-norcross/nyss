using NetTopologySuite.Geometries;

namespace RX.Nyss.Data.Models
{
    public class DataCollectorLocation
    {
        public int Id { get; set; }
        public int DataCollectorId { get; set; }
        public virtual DataCollector? DataCollector { get; set; }
        public Point? Location { get; set; }
        public virtual Village? Village { get; set; }
        public virtual Zone? Zone { get; set; }
    }
}
