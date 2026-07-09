namespace RX.Nyss.FuncApp.Configuration;

public interface IConfig
{
    NyssFuncAppConfig.MailConfigOptions MailConfig { get; set; }
    NyssFuncAppConfig.ConnectionStringsOptions ConnectionStrings { get; set; }
    string ReleaseName { get; set; }
    int MaxContentLength { get; set; }
    bool EnableResendingFeedbackMessages { get; set; }
    int NumberOfMessagesToFetchForResending { get; set; }
}

public class NyssFuncAppConfig : IConfig
{
    public MailConfigOptions MailConfig { get; set; } = new MailConfigOptions();
    public ConnectionStringsOptions ConnectionStrings { get; set; } = new ConnectionStringsOptions();
    public string ReleaseName { get; set; } = string.Empty;
    public int MaxContentLength { get; set; }
    public bool EnableResendingFeedbackMessages { get; set; }
    public int NumberOfMessagesToFetchForResending { get; set; }

    public class MailConfigOptions
    {
        public bool EnableFeedbackSms { get; set; }
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool SendToAll { get; set; }
        public bool SendFeedbackSmsToAll { get; set; }
        public SendGridConfigOptions SendGrid { get; set; } = new SendGridConfigOptions();

        public class SendGridConfigOptions
        {
            public string SendMailUrl { get; set; } = string.Empty;
            public string ApiKey { get; set; } = string.Empty;
        }
    }

    public class ConnectionStringsOptions
    {
        public string IotHubService { get; set; } = string.Empty;
    }
}
