namespace RX.Nyss.FuncApp.Configuration
{
    public interface IConfig
    {
        NyssFuncAppConfig.MailConfigOptions MailConfig { get; set; }
        NyssFuncAppConfig.ConnectionStringsOptions ConnectionStrings { get; set; }
        string ReleaseName { get; set; }
        int MaxContentLength { get; set; }
    }

    public class NyssFuncAppConfig : IConfig
    {
        public MailConfigOptions MailConfig { get; set; }
        public ConnectionStringsOptions ConnectionStrings { get; set; }
        public string ReleaseName { get; set; }
        public int MaxContentLength { get; set; }

        public class MailConfigOptions
        {
            public bool EnableFeedbackSms { get; set; }
            public string FromAddress { get; set; }
            public string FromName { get; set; }
            public bool SendToAll { get; set; }
            public bool SendFeedbackSmsToAll { get; set; }
            public SendGridConfigOptions SendGrid { get; set; }

            public class SendGridConfigOptions
            {
                public string SendMailUrl { get; set; }
                public string ApiKey { get; set; }
            }
        }

        public class ConnectionStringsOptions
        {
            public string IotHubService { get; set; }
        }
    }
}
