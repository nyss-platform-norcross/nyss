namespace RX.Nyss.FuncApp.Configuration
{
    public interface INyssFuncAppConfig
    {
        NyssFuncAppConfig.MailjetConfigOptions MailjetConfig { get; set; }
    }

    public class NyssFuncAppConfig : INyssFuncAppConfig
    {
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
