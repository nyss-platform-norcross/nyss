using System;

namespace RX.Nyss.Data.Models
{
    public class DataCollectorNotDeployed
    {
        public int Id { get; set; }
        public int DataCollectorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
