namespace RX.Nyss.ReportApi.Features.Stats.Contracts
{
    public class NyssStats
    {
        public int EscalatedAlerts { get; set; }
        public int ActiveDataCollectors { get; set; }
        public int ActiveProjects { get; set; }
        public int TotalProjects { get; set; }
    }
}
