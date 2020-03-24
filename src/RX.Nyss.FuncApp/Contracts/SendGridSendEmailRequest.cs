using System.Collections.Generic;

namespace RX.Nyss.FuncApp.Contracts
{
    public class SendGridSendEmailRequest
    {
        public List<SendGridPersonalizationsOptions> Personalizations { get; set; }

        public SendGridEmailObject From { get; set; }

        public List<SendGridEmailContent> Content { get; set; }

        public SendGridMailSettings Mail_settings { get; set; }
    }

    public class SendGridEmailContent
    {
        public string Type { get; set; }

        public string Content { get; set; }
    }

    public class SendGridMailSettings
    {
        public bool Sandbox_mode { get; set; }
    }

    public class SendGridPersonalizationsOptions
    {
        public List<SendGridEmailObject> To { get; set; }
        public string Subject { get; set; }
    }

    public class SendGridEmailObject
    {
        public string Email { get; set; }

        public string Name { get; set; }
    }
}
