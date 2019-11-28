using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services
{
    public interface IMailjetEmailClient
    {
        Task<HttpResponseMessage> SendEmail(SendEmailMessage message, bool sandboxMode);
    }

    public class MailjetEmailClient : IMailjetEmailClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfig _config;

        public MailjetEmailClient(IHttpClientFactory httpClientFactory, IConfig config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<HttpResponseMessage> SendEmail(SendEmailMessage message, bool sandboxMode)
        {
            var mailjetRequest = new MailjetSendEmailsRequest
            {
                SandboxMode = sandboxMode,
                Messages = new List<MailjetEmail>
                {
                    new MailjetEmail
                    {
                        To = new List<MailjetContact> {new MailjetContact {Email = message.To.Email, Name = message.To.Name}},
                        From = new MailjetContact {Email = _config.MailjetConfig.FromAddress, Name = _config.MailjetConfig.FromName},
                        Subject = message.Subject,
                        HTMLPart = message.Body
                    }
                }
            };

            var basicAuthHeader = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_config.MailjetConfig.ApiKey}:{_config.MailjetConfig.ApiSecret}"));

            return await PostJsonAsync(new Uri(_config.MailjetConfig.SendMailUrl, UriKind.Absolute), mailjetRequest, new[]
            {
                ("Authorization", basicAuthHeader)
            });
        }

        private async Task<HttpResponseMessage> PostJsonAsync<T>(Uri requestUri, T body, IEnumerable<(string key, string value)> headers = null)
        {
            var payload = JsonConvert.SerializeObject(body);
            var httpClient = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.key, header.value);
                }
            }

            var httpResponseMessage = await httpClient.SendAsync(request);

            httpResponseMessage.EnsureSuccessStatusCode();

            return httpResponseMessage;
        }
    }
}
