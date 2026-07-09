namespace RX.Nyss.Data.Models
{
    public class AlertRule
    {
        public int Id { get; set; }

        public int CountThreshold { get; set; }

        public int? DaysThreshold { get; set; }

        public int? KilometersThreshold { get; set; }
    }
}
