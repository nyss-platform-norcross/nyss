using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services
{
    public class MailjetEmailClient : IEmailClient
    {
        private readonly IHttpPostClient _httpPostClient;
        private readonly IConfig _config;
        private readonly string _basicAuthHeader;
        private readonly Uri _requestUri;

        public MailjetEmailClient(IHttpPostClient httpPostClient, IConfig config)
        {
            _config = config;
            _httpPostClient = httpPostClient;
            _basicAuthHeader = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_config.MailConfig.Mailjet.ApiKey}:{_config.MailConfig.Mailjet.ApiSecret}"));
            _requestUri = new Uri(_config.MailConfig.Mailjet.SendMailUrl, UriKind.Absolute);
        }

        public async Task SendEmail(SendEmailMessage message, bool sandboxMode)
        {
            var mailjetRequest = CreateMailjetSendEmailsRequest(message: message, sandboxMode: sandboxMode);

            await _httpPostClient.PostJsonAsync(_requestUri, mailjetRequest, new[] { ("Authorization", _basicAuthHeader) });
        }

        public async Task SendEmailAsTextOnly(SendEmailMessage message, bool sandboxMode)
        {
            var mailjetRequest = CreateMailjetSendEmailsRequest(message: message, sandboxMode: sandboxMode, asTextOnly: true);

            await _httpPostClient.PostJsonAsync(_requestUri, mailjetRequest, new[] { ("Authorization", _basicAuthHeader) });
        }

        private MailjetSendEmailsRequest CreateMailjetSendEmailsRequest(SendEmailMessage message, bool sandboxMode, bool asTextOnly = false) =>
            new MailjetSendEmailsRequest
            {
                SandboxMode = sandboxMode,
                Messages = new List<MailjetEmail>
                {
                    new MailjetEmail
                    {
                        To = new List<MailjetContact>
                        {
                            new MailjetContact
                            {
                                Email = message.To.Email,
                                Name = message.To.Name
                            }
                        },
                        From = new MailjetContact
                        {
                            Email = _config.MailConfig.FromAddress,
                            Name = _config.MailConfig.FromName
                        },
                        Subject = message.Subject,
                        HTMLPart = !asTextOnly
                            ? message.Body
                            : null,
                        TextPart = asTextOnly
                            ? message.Body
                            : null
                    }
                }
            };
    }
}
