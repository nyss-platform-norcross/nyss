namespace RX.Nyss.FuncApp.Configuration
{
    public interface INyssFuncappConfig
    {
        NyssFuncappConfig.MailjetConfigOptions MailjetConfig { get; set; }
        string InternalApiReportUrl { get; set; }
    }

    public class NyssFuncappConfig : INyssFuncappConfig
    {
        public string InternalApiReportUrl { get; set; }
        public MailjetConfigOptions MailjetConfig { get; set; }

        public class MailjetConfigOptions
        {
            public string ApiKey { get; set; }
            public string ApiSecret { get; set; }
            public string FromAddress { get; set; }
            public string FromName { get; set; }
            public bool SendToAll { get; set; }
            public string SendMailUrl { get; set; }
        }
    }
}
