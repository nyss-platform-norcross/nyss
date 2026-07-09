namespace RX.Nyss.Web.Services.ReportsDashboard.Dto
{
    public class PeriodDto
    {
        public string? Period { get; set; }

        public int Count { get; set; }

        public bool ValueEquals(object obj)
        {
            var other = (PeriodDto)obj;
            return Period == other.Period && Count == other.Count;
        }
    }
}
