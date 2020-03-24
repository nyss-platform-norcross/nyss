using System.Collections.Generic;

namespace RX.Nyss.FuncApp.Contracts
{
    public class MailjetSendEmailsRequest
    {
        public bool SandboxMode { get; set; }
        public List<MailjetEmail> Messages { get; set; }
    }
    
    public class MailjetEmail
    {
        public MailjetContact From { get; set; }
        public List<MailjetContact> To { get; set; }
        public string Subject { get; set; }
        public string HTMLPart { get; set; }
        public string TextPart { get; set; }
    }

    public class MailjetContact
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
