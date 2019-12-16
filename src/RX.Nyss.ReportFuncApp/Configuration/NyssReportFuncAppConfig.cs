namespace RX.Nyss.ReportFuncApp.Configuration
{
    public interface IConfig
    {
        string ReportApiBaseUrl { get; set; }
    }

    public class NyssReportFuncAppConfig : IConfig
    {
        public string ReportApiBaseUrl { get; set; }
    }
}
