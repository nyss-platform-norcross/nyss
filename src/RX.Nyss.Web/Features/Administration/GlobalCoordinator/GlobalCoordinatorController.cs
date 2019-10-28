using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Administration.GlobalCoordinator.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Administration.GlobalCoordinator
{
    public class GlobalCoordinatorController : BaseController
    {
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly IGlobalCoordinatorService _globalCoordinatorService;
        private readonly IConfig _config;

        public GlobalCoordinatorController(IGlobalCoordinatorService globalCoordinatorService, IEmailPublisherService emailPublisherService, IConfig config)
        {
            _globalCoordinatorService = globalCoordinatorService;
            _emailPublisherService = emailPublisherService;
            _config = config;
        }

        /// <summary>
        ///     Register a global coordinator user.
        /// </summary>
        /// <param name="dto">The global coordinator to be created</param>
        /// <returns></returns>
        [HttpPost("registerGlobalCoordinator")]
        [NeedsRole(Role.Administrator)]
        public async Task<Result> RegisterGlobalCoordinator([FromBody] RegisterGlobalCoordinatorRequestDto dto)
        {
            var (result, securityStamp) = await _globalCoordinatorService.RegisterGlobalCoordinator(dto);

            if (!result.IsSuccess)
            {
                return result;
            }

            var verificationUrl = new Uri(_config.VerificationCallbackUrl + 
                                          $"?verificationToken={WebUtility.UrlEncode(securityStamp)}" + 
                                          $"&email={WebUtility.UrlEncode(dto.Email)}", 
                UriKind.Absolute).ToString();

            var (emailSubject, emailBody) = EmailTextGenerator.GenerateEmailVerificationEmail(
                Role.GlobalCoordinator.ToString(),
                verificationUrl,
                dto.Name);

            await _emailPublisherService.SendEmail((dto.Email, dto.Name), emailSubject, emailBody);

            return result;
        }
    }
}
