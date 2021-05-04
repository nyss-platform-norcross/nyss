namespace RX.Nyss.Web.Features.Common.Dto
{
    public class ReportStatusFilterDto
    {
        public bool Kept { get; set; }
        public bool Dismissed { get; set; }
        public bool NotCrossChecked { get; set; }
        public bool Training { get; set; }
    }
}
