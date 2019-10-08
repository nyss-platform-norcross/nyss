using System;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class Alert
    {
        public int Id { get; set; }

        public AlertStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Comments { get; set; }

        public ProjectHealthRisk ProjectHealthRisk { get; set; }
    }
}
