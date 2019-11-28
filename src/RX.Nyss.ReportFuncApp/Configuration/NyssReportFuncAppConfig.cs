namespace RX.Nyss.ReportFuncApp.Configuration
{
    public interface IConfig
    {
        string ReportApiUrl { get; set; }
    }

    public class NyssReportFuncAppConfig : IConfig
    {
        public string ReportApiUrl { get; set; }
    }
}
