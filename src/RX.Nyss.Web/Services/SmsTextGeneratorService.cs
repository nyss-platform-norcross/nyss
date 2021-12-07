using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RX.Nyss.Common.Services.StringsResources;

namespace RX.Nyss.Web.Services
{
    public interface ISmsTextGeneratorService
    {
        Task<string> GenerateEscalatedAlertSms(string languageCode);

        Task<string> GenerateReplaceSupervisorSms(string languageCode);
    }

    public class SmsTextGeneratorService : ISmsTextGeneratorService
    {
        private readonly IStringsResourcesService _stringsResourcesService;

        public SmsTextGeneratorService(IStringsResourcesService stringsResourcesService)
        {
            _stringsResourcesService = stringsResourcesService;
        }

        public async Task<string> GenerateEscalatedAlertSms(string languageCode)
        {
            var translatedSmsTexts = await _stringsResourcesService.GetSmsContentResources(languageCode);

            return GetTranslation("sms.alertEscalated", languageCode, translatedSmsTexts.Value);
        }

        public async Task<string> GenerateReplaceSupervisorSms(string languageCode)
        {
            var translatedSmsText = await _stringsResourcesService.GetSmsContentResources(languageCode);

            return GetTranslation("sms.replaceSupervisor", languageCode, translatedSmsText.Value);
        }

        private static string GetTranslation(string key, string languageCode, IDictionary<string, string> translations)
        {
            if (!translations.TryGetValue(key, out var value))
            {
                throw new Exception($"Could not find translations for {key} with language: {languageCode}");
            }

            return value;
        }
    }
}
