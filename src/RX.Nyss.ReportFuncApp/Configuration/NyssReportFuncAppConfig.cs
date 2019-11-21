namespace RX.Nyss.ReportFuncApp.Configuration
{
    public interface INyssReportFuncAppConfig
    {
        string ReportApiUrl { get; set; }
    }

    public class NyssReportFuncAppConfig : INyssReportFuncAppConfig
    {
        public string ReportApiUrl { get; set; }
    }
}
