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

        public MailjetEmailClient(IHttpPostClient httpPostClient, IConfig config)
        {
            _httpPostClient = httpPostClient;
            _config = config;
        }

        public async Task SendEmail(SendEmailMessage message, bool sandboxMode)
        {
            var mailjetRequest = new MailjetSendEmailsRequest
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
                        HTMLPart = message.Body
                    }
                }
            };

            var basicAuthHeader = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_config.MailConfig.Mailjet.ApiKey}:{_config.MailConfig.Mailjet.ApiSecret}"));

            await _httpPostClient.PostJsonAsync(new Uri(_config.MailConfig.Mailjet.SendMailUrl, UriKind.Absolute), mailjetRequest, new[] { ("Authorization", basicAuthHeader) });
        }

        public async Task SendEmailAsTextOnly(SendEmailMessage message, bool sandboxMode)
        {
            var mailjetRequest = new MailjetSendTextEmailsRequest
            {
                SandboxMode = sandboxMode,
                Messages = new List<MailjetTextEmail>
                {
                    new MailjetTextEmail
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
                        TextPart = message.Body
                    }
                }
            };

            var basicAuthHeader = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_config.MailConfig.Mailjet.ApiKey}:{_config.MailConfig.Mailjet.ApiSecret}"));

            await _httpPostClient.PostJsonAsync(new Uri(_config.MailConfig.Mailjet.SendMailUrl, UriKind.Absolute), mailjetRequest, new[] { ("Authorization", basicAuthHeader) });
        }
    }
}
