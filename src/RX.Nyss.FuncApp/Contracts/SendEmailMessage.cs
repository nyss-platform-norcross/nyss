namespace RX.Nyss.FuncApp.Contracts
{
    public class SendEmailMessage
    {
        public Contact To { get; set; } = new Contact();

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string AttachmentFilename { get; set; } = string.Empty;

        public bool SendAsTextOnly { get; set; }
    }

    public class Contact
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; }   = string.Empty;
    }
}
