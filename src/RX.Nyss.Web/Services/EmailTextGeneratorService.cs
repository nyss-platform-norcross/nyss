using System.Threading.Tasks;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Services.StringsResources;

namespace RX.Nyss.Web.Services
{
    public interface IEmailTextGeneratorService
    {
        Task<(string subject, string body)> GenerateResetPasswordEmail(string resetUrl, string name, string languageCode);
        Task<(string subject, string body)> GenerateEmailVerificationEmail(Role role, string callbackUrl, string name, string languageCode);
    }

    public class EmailTextGeneratorService : IEmailTextGeneratorService
    {
        private readonly IStringsResourcesService _stringsResourcesService;
        public EmailTextGeneratorService(IStringsResourcesService stringsResourcesService)
        {
            _stringsResourcesService = stringsResourcesService;
        }

        public async Task<(string subject, string body)> GenerateResetPasswordEmail(string resetUrl, string name, string languageCode)
        {
            var emailContents = await _stringsResourcesService.GetEmailContentResources(languageCode);
            emailContents.Value.TryGetValue("email.reset.subject", out var subject);
            emailContents.Value.TryGetValue("email.reset.body", out var body);

            body = body
                .Replace("{{name}}", name)
                .Replace("{{resetUrl}}", resetUrl);

            return (subject, body);
        }

        public async Task<(string subject, string body)> GenerateEmailVerificationEmail(Role role, string callbackUrl, string name, string languageCode)
        {
            var roleName = GetRoleName(role);
            var emailContents = await _stringsResourcesService.GetEmailContentResources(languageCode);
            emailContents.Value.TryGetValue("email.verification.subject", out var subject);
            emailContents.Value.TryGetValue("email.verification.body", out var body);

            body = body
                .Replace("{{username}}", name)
                .Replace("{{roleName}}", roleName)
                .Replace("{{link}}", callbackUrl);

            return (subject, body);
        }

        private static string GetRoleName(Role userRole) =>
            userRole switch
            {
                Role.DataConsumer => "Data consumer",
                Role.GlobalCoordinator => "Global coordinator",
                Role.TechnicalAdvisor => "Technical advisor",
                _ => userRole.ToString()
            };
    }
}
