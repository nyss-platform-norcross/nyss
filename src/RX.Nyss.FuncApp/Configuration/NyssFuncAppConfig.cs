namespace RX.Nyss.FuncApp.Configuration
{
    public interface IConfig
    {
        NyssFuncAppConfig.MailjetConfigOptions MailjetConfig { get; set; }
    }

    public class NyssFuncAppConfig : IConfig
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
            public bool EnableFeedbackSms { get; set; }
            public bool SendFeedbackSmsToAll { get; set; }
        }
    }
}
