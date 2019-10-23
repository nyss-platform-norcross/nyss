using System;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Administration.GlobalCoordinator.Dto;
using RX.Nyss.Web.Features.Logging;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Administration.GlobalCoordinator
{
    public interface IGlobalCoordinatorService
    {
        Task<Result> RegisterGlobalCoordinator(RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto);
    }

    public class GlobalCoordinatorService : IGlobalCoordinatorService
    {
        private readonly IConfig _config;
        private readonly INyssContext _dataContext;
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly ILoggerAdapter _loggerAdapter;

        public GlobalCoordinatorService(
            IIdentityUserRegistrationService identityUserRegistrationService,
            INyssContext dataContext,
            ILoggerAdapter loggerAdapter,
            IConfig config,
            IEmailPublisherService emailPublisherService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _config = config;
            _emailPublisherService = emailPublisherService;
        }

        public async Task<Result> RegisterGlobalCoordinator(RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto)
        {
            try
            {
                var identityUser = await CreateUser(registerGlobalCoordinatorRequestDto);
                var securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                var baseUrl = new Uri(_config.BaseUrl);
                var verificationUrl = new Uri(baseUrl, $"verifyEmail?email={WebUtility.UrlEncode(registerGlobalCoordinatorRequestDto.Email)}&token={WebUtility.UrlEncode(securityStamp)}").ToString();

                var (emailSubject, emailBody) = EmailTextGenerator.GenerateEmailVerificationEmail(
                    Role.GlobalCoordinator.ToString(),
                    verificationUrl,
                    registerGlobalCoordinatorRequestDto.Name);

                await _emailPublisherService.SendEmail((registerGlobalCoordinatorRequestDto.Email, registerGlobalCoordinatorRequestDto.Name), emailSubject, emailBody);

                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task<IdentityUser> CreateUser(RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto)
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var identityUser = await _identityUserRegistrationService.CreateIdentityUser(registerGlobalCoordinatorRequestDto.Email, Role.GlobalCoordinator);
            await CreateGlobalCoordinator(identityUser, registerGlobalCoordinatorRequestDto);

            transactionScope.Complete();
            return identityUser;
        }

        private async Task CreateGlobalCoordinator(IdentityUser identityUser, RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto)
        {
            var globalCoordinator = new GlobalCoordinatorUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = registerGlobalCoordinatorRequestDto.Name,
                PhoneNumber = registerGlobalCoordinatorRequestDto.PhoneNumber,
                Role = Role.GlobalCoordinator
            };

            await _dataContext.AddAsync(globalCoordinator);
            await _dataContext.SaveChangesAsync();
        }
    }
}
