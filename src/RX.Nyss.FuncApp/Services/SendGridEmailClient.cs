using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services
{
    public class SendGridEmailClient : IEmailClient
    {
        private readonly IHttpPostClient _httpPostClient;
        private readonly IConfig _config;
        private readonly string _basicAuthHeader;
        private readonly Uri _sendGridSendMailUrl;

        public SendGridEmailClient(IHttpPostClient httpPostClient, IConfig config)
        {
            _httpPostClient = httpPostClient;
            _config = config;
            _basicAuthHeader = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_config.MailConfig.SendGrid.ApiKey}"));
            _sendGridSendMailUrl = new Uri(_config.MailConfig.SendGrid.SendMailUrl, UriKind.Absolute);
        }

        public async Task SendEmail(SendEmailMessage message, bool sandboxMode)
        {
            var sendGridMessage = CreateSendGridMessage(message: message, sandboxMode: sandboxMode, "text/html");

            await _httpPostClient.PostJsonAsync(_sendGridSendMailUrl, sendGridMessage, new[] { ("Authorization", _basicAuthHeader) });
        }

        public async Task SendEmailAsTextOnly(SendEmailMessage message, bool sandboxMode)
        {
            var sendGridMessage = CreateSendGridMessage(message: message, sandboxMode: sandboxMode, "text/plain");

            await _httpPostClient.PostJsonAsync(_sendGridSendMailUrl, sendGridMessage, new[] { ("Authorization", _basicAuthHeader) });
        }

        private SendGridSendEmailRequest CreateSendGridMessage(SendEmailMessage message, bool sandboxMode, string contentType) =>
            new SendGridSendEmailRequest
            {
                Personalizations = new List<SendGridPersonalizationsOptions>
                {
                    new SendGridPersonalizationsOptions
                    {
                        Subject = message.Subject,
                        To = new List<SendGridEmailObject>
                        {
                            new SendGridEmailObject
                            {
                                Email = message.To.Email,
                                Name = message.To.Name
                            }
                        }
                    }
                },
                From = new SendGridEmailObject
                {
                    Email = _config.MailConfig.FromAddress,
                    Name = _config.MailConfig.FromName
                },
                Content = new List<SendGridEmailContent>
                {
                    new SendGridEmailContent
                    {
                        Type = contentType,
                        Content = message.Body
                    }
                },
                Mail_settings = new SendGridMailSettings { Sandbox_mode = sandboxMode }
            };
    }
}
